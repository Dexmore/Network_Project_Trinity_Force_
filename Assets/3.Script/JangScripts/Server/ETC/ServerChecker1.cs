using Mirror;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System.IO;
using kcp2k;
using System;

public enum LicenseType
{
    Empty = 0,
    Client,
    Server
}

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

// ★ NetworkPlayer는 따로 존재! (여기 포함하지 않음)

public class ServerChecker1 : MonoBehaviour
{
    public LicenseType type;
    private NetworkManager manager;
    private KcpTransport kcp;

    [SerializeField] private string path;
    public string ServerIP { get; private set; }
    public string Port { get; private set; }
    private List<NetworkConnectionToClient> players = new List<NetworkConnectionToClient>();

    public List<GameTurn> gameLog = new List<GameTurn>();

    private List<NetworkPlayer> submittedPlayers = new List<NetworkPlayer>();
    private List<string> submittedSentences = new List<string>();
    private List<NetworkPlayer> drawingPlayers = new List<NetworkPlayer>();
    private List<byte[]> submittedDrawings = new List<byte[]>();
    private List<NetworkPlayer> guessPlayers = new List<NetworkPlayer>();
    private List<string> submittedGuesses = new List<string>();
    private List<string> sentenceOwners = new List<string>();

    private int playerCount = 4;

    private void OnEnable()
    {
        path = Application.dataPath + "/License";
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        if (!File.Exists(path + "/License.json")) CreateDefaultLicenseFile(path);
        manager = GetComponent<NetworkManager>();
        kcp = (KcpTransport)manager.transport;
    }

    private void Start()
    {
        type = LoadLicenseType();
        if (type == LicenseType.Server) Start_Server();
        else Start_Client();
    }

    private void CreateDefaultLicenseFile(string dirPath)
    {
        List<LicenseItem> items = new List<LicenseItem>
        {
            new LicenseItem("0", "127.0.0.1", "7777")
        };
        JsonData data = JsonMapper.ToJson(items);
        File.WriteAllText(dirPath + "/License.json", data.ToString());
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

            return parsedType;
        }
        catch (Exception e)
        {
            Debug.LogError("License Load Error: " + e.Message);
            return LicenseType.Empty;
        }
    }

    public void Start_Server()
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            Debug.Log("WebGL cannot be Server");
            return;
        }
        manager.StartServer();
        Debug.Log($"{manager.networkAddress} Start Server");

        NetworkServer.OnConnectedEvent += (conn) =>
        {
            if (players.Count >= playerCount)
            {
                conn.Disconnect();
                return;
            }
            if (!players.Contains(conn)) players.Add(conn);

            if (players.Count == playerCount)
            {
                foreach (var c in players)
                    c.Send(new GameStartMsg());
            }
        };
        NetworkServer.OnDisconnectedEvent += (conn) =>
        {
            if (players.Contains(conn)) players.Remove(conn);
        };
    }

    public void Start_Client()
    {
        manager.StartClient();
        Debug.Log($"{manager.networkAddress} : Start Client...");
    }

    // 갈틱폰 게임 데이터 관리 함수들
    public void AddSentence(NetworkPlayer player, string sentence)
    {
        if (!submittedPlayers.Contains(player))
        {
            submittedPlayers.Add(player);
            submittedSentences.Add(sentence);
        }
        if (submittedPlayers.Count == playerCount)
        {
            // phase 끝났으니 소유자 리스트 갱신
            sentenceOwners.Clear();
            for (int i = 0; i < playerCount; i++)
            {
                sentenceOwners.Add(submittedPlayers[i].playerName);
                // gameLog에 이 phase의 실제 값 push
                gameLog.Add(new GameTurn
                {
                    playerName = submittedPlayers[i].playerName,
                    sentence = submittedSentences[i],
                    ownerName = submittedPlayers[i].playerName,
                    isText = true
                });
            }
            ShowSentencesToEachPlayer();
            NextPhaseToAll();
            submittedPlayers.Clear();
            submittedSentences.Clear();
        }
    }


    public void AddDrawing(NetworkPlayer player, byte[] pngData)
    {
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
                    ownerName = ownerName,
                    isText = false
                });
            }
            DistributeDrawings();
            NextPhaseToAll();
            drawingPlayers.Clear();
            submittedDrawings.Clear();
        }
    }


    public void AddGuess(NetworkPlayer player, string guessText)
    {
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
                    ownerName = ownerName,
                    isText = true
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
        for (int i = 0; i < count; i++)
        {
            int targetIndex = (i + 1) % count;
            NetworkPlayer receiver = submittedPlayers[targetIndex];
            string sentence = submittedSentences[i];
            receiver.TargetShowSentence(receiver.connectionToClient, sentence);
        }
    }

    public void DistributeDrawings()
    {
        int count = drawingPlayers.Count;
        for (int i = 0; i < count; i++)
        {
            int targetIndex = (i + 1) % count;
            NetworkPlayer receiver = drawingPlayers[targetIndex];
            byte[] pngData = submittedDrawings[i];
            receiver.TargetReceiveDrawing(receiver.connectionToClient, pngData);
        }
    }

    public void ShowGuessesToEachPlayer()
    {
        int count = guessPlayers.Count;
        for (int i = 0; i < count; i++)
        {
            int targetIndex = (i + 1) % count;
            NetworkPlayer receiver = guessPlayers[targetIndex];
            string guess = submittedGuesses[i];
            receiver.TargetShowGuess(receiver.connectionToClient, guess);
        }
    }

    private void NextPhaseToAll()
    {
        foreach (var conn in NetworkServer.connections.Values)
        {
            conn.Send(new ProceedToNextPhaseMsg());
        }
    }

    public List<PlayerResult> ConvertGameLogToPlayerResults()
    {
        var result = new List<PlayerResult>();
        int bundle = 4;
        int playerCountLocal = gameLog.Count / bundle;
        for (int i = 0; i < playerCountLocal; i++)
        {
            int offset = i * bundle;
            var playerResult = new PlayerResult
            {
                playerName = gameLog[offset + 0].playerName,
                sentence = gameLog[offset + 0].sentence,
                drawing1 = gameLog[offset + 1].drawing,
                guess = gameLog[offset + 2].guess,
                drawing2 = gameLog[offset + 3].drawing
            };
            result.Add(playerResult);
        }
        return result;
    }

    public void SendResultsToClients()
    {
        List<PlayerResult> resultList = ConvertGameLogToPlayerResults();

        Debug.Log($"[서버] 결과 개수: {resultList.Count}");
        foreach (var result in resultList)
        {
            Debug.Log($"[서버] 플레이어: {result.playerName}, 문장: {result.sentence}, 추측: {result.guess}, 그림1: {(result.drawing1 != null ? "O" : "X")}, 그림2: {(result.drawing2 != null ? "O" : "X")}");
        }

        // PlayerResult → PlayerResultData로 변환
        List<PlayerResultData> dataList = new List<PlayerResultData>();
        foreach (var r in resultList)
        {
            dataList.Add(new PlayerResultData
            {
                playerName = r.playerName,
                sentence = r.sentence,
                drawing1 = r.drawing1,
                guess = r.guess,
                drawing2 = r.drawing2
            });
        }

        NetworkServer.SendToAll(new GameResultMsg { results = dataList });
    }
    private void OnApplicationQuit()
    {
        if (NetworkClient.isConnected) manager.StopClient();
        if (NetworkServer.active) manager.StopServer();
    }
}
