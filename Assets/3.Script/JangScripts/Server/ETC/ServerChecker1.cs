using Mirror;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System.IO;
using kcp2k;
using System;
using System.Collections;

public enum LicenseType { Empty = 0, Client, Server }

public class LicenseItem
{
    public string License;
    public string ServerIP;
    public string Port;
    public LicenseItem(string license, string ip, string port)
    {
        License = license;
        ServerIP = ip;
        Port = port;
    }
}

public class PlayerResult
{
    public string playerName;
    public string sentence;
    public byte[] drawing1;
    public string guess;
    public byte[] drawing2;
}

// NetworkPlayer는 따로 존재

public class ServerChecker1 : MonoBehaviour
{
    public LicenseType type;
    private NetworkManager manager;
    private KcpTransport kcp;

    [SerializeField] private string path;
    public string ServerIP { get; private set; }
    public string Port { get; private set; }
    private List<NetworkConnectionToClient> players = new List<NetworkConnectionToClient>();

    public class GameTurn
    {
        public string playerName;
        public string sentence;
        public byte[] drawing;
        public string guess;
        public string ownerName;
    }
    public List<GameTurn> gameLog = new List<GameTurn>();

    private List<NetworkPlayer> submittedPlayers = new List<NetworkPlayer>();
    private List<string> submittedSentences = new List<string>();
    private List<NetworkPlayer> drawingPlayers = new List<NetworkPlayer>();
    private List<byte[]> submittedDrawings = new List<byte[]>();
    private List<NetworkPlayer> guessPlayers = new List<NetworkPlayer>();
    private List<string> submittedGuesses = new List<string>();
    private List<string> sentenceOwners = new List<string>();

    private int playerCount = 4;
    private bool eventsRegistered = false; // 이벤트 중복 등록 방지용

    private void OnEnable()
    {
        path = Application.dataPath + "/License";
        Debug.Log("[ServerChecker1] OnEnable() 실행됨");

        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        if (!File.Exists(path + "/License.json")) CreateDefaultLicenseFile(path);
        manager = GetComponent<NetworkManager>();
        kcp = (KcpTransport)manager.transport;

        if (!eventsRegistered)
        {
            NetworkServer.OnConnectedEvent += OnPlayerConnected;
            NetworkServer.OnDisconnectedEvent += OnPlayerDisconnected;
            eventsRegistered = true;
            Debug.Log("[ServerChecker1] NetworkServer 이벤트 등록 완료");
        }
    }

    private void OnDisable()
    {
        if (eventsRegistered)
        {
            NetworkServer.OnConnectedEvent -= OnPlayerConnected;
            NetworkServer.OnDisconnectedEvent -= OnPlayerDisconnected;
            eventsRegistered = false;
            Debug.Log("[ServerChecker1] NetworkServer 이벤트 해제 완료");
        }
    }

    private void Start()
    {
        Debug.Log("[ServerChecker1] Start() 실행됨");
        type = LoadLicenseType();
        if (type == LicenseType.Server) Start_Server();
        else Start_Client();
    }

    private LicenseType LoadLicenseType()
    {
        try
        {
            string jsonString = File.ReadAllText(path + "/License.json");
            JsonData itemdata = JsonMapper.ToObject(jsonString);

            string type_s = itemdata[0]["License"].ToString();
            string ip_s = itemdata[0]["ServerIP"].ToString();
            string port_s = itemdata[0]["Port"].ToString();

            ServerIP = ip_s;
            Port = port_s;
            var parsedType = (LicenseType)int.Parse(type_s);

            manager.networkAddress = ServerIP;
            kcp.port = ushort.Parse(Port);

            Debug.Log($"[ServerChecker1] License 정보 로드됨: {ServerIP}:{Port}, 타입: {parsedType}");
            return parsedType;
        }
        catch (Exception e)
        {
            Debug.LogError("[ServerChecker1] License Load Error: " + e.Message);
            return LicenseType.Empty;
        }
    }

    private void CreateDefaultLicenseFile(string dirPath)
    {
        List<LicenseItem> items = new List<LicenseItem>
        {
            new LicenseItem("0", "127.0.0.1", "7777")
        };
        JsonData data = JsonMapper.ToJson(items);
        File.WriteAllText(dirPath + "/License.json", data.ToString());
        Debug.Log("[ServerChecker1] 기본 라이선스 파일 생성");
    }

    public void Start_Server()
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            Debug.Log("WebGL cannot be Server");
            return;
        }
        manager.StartServer();
        Debug.Log($"[ServerChecker1] {manager.networkAddress} Start Server");
    }

    public void Start_Client()
    {
        manager.StartClient();
        Debug.Log($"[ServerChecker1] {manager.networkAddress} : Start Client...");
    }

    private void OnPlayerConnected(NetworkConnectionToClient conn)
    {
        Debug.Log($"[ServerChecker1] Player connected! : {players.Count + 1}");

        if (players.Contains(conn)) return;
        if (players.Count >= playerCount)
        {
            conn.Disconnect();
            Debug.Log("[ServerChecker1] Sry 4 Players");
            return;
        }
        players.Add(conn);

        Debug.Log($"[ServerChecker1] Player : {players.Count}");

        if (players.Count == playerCount)
        {
            Debug.Log("[ServerChecker1] Get Ready...");
            NetworkManager.singleton.ServerChangeScene("GameScene");
            StartCoroutine(WaitAndSendGameStart());
        }
    }

    private IEnumerator WaitAndSendGameStart()
    {
        yield return new WaitForSeconds(1.0f);
        foreach (var c in players)
        {
            Debug.Log($"[ServerChecker1] GameStartMsg : connID={c.connectionId}");
            c.Send(new GameStartMsg());
        }
    }

    private void OnPlayerDisconnected(NetworkConnectionToClient conn)
    {
        if (players.Contains(conn)) players.Remove(conn);
        Debug.Log($"[ServerChecker1] Player out: {players.Count} / {playerCount}");

        if (players.Count < playerCount)
        {
            foreach (var c in players)
                c.Send(new GameResultMsg { results = new List<PlayerResultData>() });
            Debug.Log("[ServerChecker1] PLayer Out / Empty Message");
        }
    }

    // ========================= 게임 진행 데이터 관리 =========================

    public void AddSentence(NetworkPlayer player, string sentence)
    {
        Debug.Log($"[ServerChecker1] AddSentence : {player.playerName}, {sentence}");

        if (!submittedPlayers.Contains(player))
        {
            submittedPlayers.Add(player);
            submittedSentences.Add(sentence);
        }
        if (submittedPlayers.Count == playerCount)
        {
            sentenceOwners.Clear();
            for (int i = 0; i < playerCount; i++)
            {
                sentenceOwners.Add(submittedPlayers[i].playerName);
                gameLog.Add(new GameTurn
                {
                    playerName = submittedPlayers[i].playerName,
                    sentence = submittedSentences[i],
                    ownerName = submittedPlayers[i].playerName
                });
                Debug.Log($"[ServerChecker1] gameLog: {submittedPlayers[i].playerName}, {submittedSentences[i]}");
            }
            ShowSentencesToEachPlayer();
            NextPhaseToAll();
            submittedPlayers.Clear();
            submittedSentences.Clear();
        }
    }

    public void AddDrawing(NetworkPlayer player, byte[] pngData)
    {
        Debug.Log($"[ServerChecker1] AddDrawing : {player.playerName}, DataLen={pngData?.Length}");

        if (!drawingPlayers.Contains(player))
        {
            drawingPlayers.Add(player);
            submittedDrawings.Add(pngData);
        }
        if (drawingPlayers.Count == playerCount)
        {
            for (int i = 0; i < playerCount; i++)
            {
                int ownerIdx = (i + playerCount - 1) % playerCount;
                string ownerName = sentenceOwners[ownerIdx];
                gameLog.Add(new GameTurn
                {
                    playerName = drawingPlayers[i].playerName,
                    drawing = submittedDrawings[i],
                    ownerName = ownerName
                });
                Debug.Log($"[ServerChecker1] gameLog : {drawingPlayers[i].playerName}, ownerName={ownerName}");
            }
            DistributeDrawings();
            NextPhaseToAll();
            drawingPlayers.Clear();
            submittedDrawings.Clear();
        }
    }

    public void AddGuess(NetworkPlayer player, string guessText)
    {
        Debug.Log($"[ServerChecker1] AddGuess / {player.playerName}, {guessText}");

        if (!guessPlayers.Contains(player))
        {
            guessPlayers.Add(player);
            submittedGuesses.Add(guessText);
        }
        if (guessPlayers.Count == playerCount)
        {
            for (int i = 0; i < playerCount; i++)
            {
                int ownerIdx = (i + playerCount - 2) % playerCount;
                string ownerName = sentenceOwners[ownerIdx];
                gameLog.Add(new GameTurn
                {
                    playerName = guessPlayers[i].playerName,
                    guess = submittedGuesses[i],
                    ownerName = ownerName
                });
            }
            ShowGuessesToEachPlayer();
            NextPhaseToAll();
            guessPlayers.Clear();
            submittedGuesses.Clear();
        }
    }

    public void ShowSentencesToEachPlayer()
    {
        int count = submittedPlayers.Count;
        Debug.Log("[ServerChecker1] ShowSentencesToEachPlayer");
        for (int i = 0; i < count; i++)
        {
            int targetIndex = (i + 1) % count;
            NetworkPlayer receiver = submittedPlayers[targetIndex];
            string sentence = submittedSentences[i];
            Debug.Log($"[ServerChecker1] -> {receiver.playerName} / {sentence}");
            receiver.TargetShowSentence(receiver.connectionToClient, sentence);
        }
    }

    public void DistributeDrawings()
    {
        int count = drawingPlayers.Count;
        Debug.Log("[ServerChecker1] DistributeDrawings");
        for (int i = 0; i < count; i++)
        {
            int targetIndex = (i + 1) % count;
            NetworkPlayer receiver = drawingPlayers[targetIndex];
            byte[] pngData = submittedDrawings[i];
            Debug.Log($"[ServerChecker1] -> {receiver.playerName} / {pngData?.Length}");
            receiver.TargetReceiveDrawing(receiver.connectionToClient, pngData);
        }
    }

    public void ShowGuessesToEachPlayer()
    {
        int count = guessPlayers.Count;
        Debug.Log("[ServerChecker1] ShowGuessesToEachPlayer");
        for (int i = 0; i < count; i++)
        {
            int targetIndex = (i + 1) % count;
            NetworkPlayer receiver = guessPlayers[targetIndex];
            string guess = submittedGuesses[i];
            Debug.Log($"[ServerChecker1] -> {receiver.playerName} -> {guess}");
            receiver.TargetShowGuess(receiver.connectionToClient, guess);
        }
    }

    private void NextPhaseToAll()
    {
        Debug.Log("[ServerChecker1] NextPhaseToAll");
        foreach (var conn in NetworkServer.connections.Values)
        {
            Debug.Log($"[ServerChecker1] ProceedToNextPhaseMsg : connID={conn.connectionId}");
            conn.Send(new ProceedToNextPhaseMsg());
        }
    }

    public List<PlayerResult> ConvertGameLogToPlayerResults()
    {
        var result = new List<PlayerResult>();
        HashSet<string> ownerNames = new HashSet<string>();
        foreach (var log in gameLog)
        {
            if (!string.IsNullOrEmpty(log.ownerName))
                ownerNames.Add(log.ownerName);
        }

        foreach (var owner in ownerNames)
        {
            var sentenceObj = gameLog.Find(x => x.ownerName == owner && !string.IsNullOrEmpty(x.sentence));
            var drawing1Obj = gameLog.Find(x => x.ownerName == owner && x.drawing != null && string.IsNullOrEmpty(x.guess));
            var guessObj = gameLog.Find(x => x.ownerName == owner && !string.IsNullOrEmpty(x.guess));
            var drawing2Obj = gameLog.FindLast(x => x.ownerName == owner && x.drawing != null && !string.IsNullOrEmpty(x.guess));

            result.Add(new PlayerResult
            {
                playerName = owner,
                sentence = sentenceObj?.sentence ?? "",
                drawing1 = drawing1Obj?.drawing,
                guess = guessObj?.guess ?? "",
                drawing2 = drawing2Obj?.drawing
            });
            Debug.Log($"[ServerChecker1] 결과 변환: {owner}");
        }
        return result;
    }

    private void OnApplicationQuit()
    {
        Debug.Log("[ServerChecker1] OnApplicationQuit 호출됨");
        if (NetworkClient.isConnected) manager.StopClient();
        if (NetworkServer.active) manager.StopServer();
    }
}