using Mirror;
using UnityEngine;
using System.Collections.Generic;
using LitJson;
using System.IO;
using kcp2k;
using System;

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

public class ServerChecker1 : MonoBehaviour
{
    public LicenseType type;
    private NetworkManager manager;
    private KcpTransport kcp;
    private string path;
    public string ServerIP { get; private set; }
    public string Port { get; private set; }
    private List<NetworkConnectionToClient> players = new();

    public class GameTurn
    {
        public string playerName;
        public string sentence;
        public byte[] drawing;
        public string guess;
        public string ownerName;
    }

    public List<GameTurn> gameLog = new();

    private List<NetworkPlayer> submittedPlayers = new();
    private List<string> submittedSentences = new();
    private List<NetworkPlayer> drawingPlayers = new();
    private List<byte[]> submittedDrawings = new();
    private List<NetworkPlayer> guessPlayers = new();
    private List<string> submittedGuesses = new();
    private List<string> sentenceOwners = new();

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
        List<LicenseItem> items = new() { new("0", "127.0.0.1", "7777") };
        string json = JsonMapper.ToJson(items);
        File.WriteAllText(dirPath + "/License.json", json);
    }

    private LicenseType LoadLicenseType()
    {
        try
        {
            string jsonString = File.ReadAllText(path + "/License.json");
            JsonData data = JsonMapper.ToObject(jsonString);
            ServerIP = data[0]["ServerIP"].ToString();
            Port = data[0]["Port"].ToString();
            int typeVal = int.Parse(data[0]["License"].ToString());

            manager.networkAddress = ServerIP;
            kcp.port = ushort.Parse(Port);

            return (LicenseType)typeVal;
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

        NetworkServer.OnConnectedEvent += conn =>
        {
            if (players.Count >= playerCount) { conn.Disconnect(); return; }
            if (!players.Contains(conn)) players.Add(conn);

            if (players.Count == playerCount)
                foreach (var c in players)
                    c.Send(new GameStartMsg());
        };

        NetworkServer.OnDisconnectedEvent += conn =>
        {
            if (players.Contains(conn)) players.Remove(conn);
        };
    }

    public void Start_Client()
    {
        manager.StartClient();
        Debug.Log("Start Client...");
    }

    public void AddSentence(NetworkPlayer player, string sentence)
    {
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
                    ownerName = ownerName
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
                    ownerName = ownerName
                });
            }
            ShowGuessesToEachPlayer();
            NextPhaseToAll();
            guessPlayers.Clear();
            submittedGuesses.Clear();
        }
    }

    private void ShowSentencesToEachPlayer()
    {
        for (int i = 0; i < submittedPlayers.Count; i++)
        {
            int targetIndex = (i + 1) % submittedPlayers.Count;
            submittedPlayers[targetIndex].TargetShowSentence(
                submittedPlayers[targetIndex].connectionToClient,
                submittedSentences[i]
            );
        }
    }

    private void DistributeDrawings()
    {
        for (int i = 0; i < drawingPlayers.Count; i++)
        {
            int targetIndex = (i + 1) % drawingPlayers.Count;
            drawingPlayers[targetIndex].TargetReceiveDrawing(
                drawingPlayers[targetIndex].connectionToClient,
                submittedDrawings[i]
            );
        }
    }

    private void ShowGuessesToEachPlayer()
    {
        for (int i = 0; i < guessPlayers.Count; i++)
        {
            int targetIndex = (i + 1) % guessPlayers.Count;
            guessPlayers[targetIndex].TargetShowGuess(
                guessPlayers[targetIndex].connectionToClient,
                submittedGuesses[i]
            );
        }
    }

    private void NextPhaseToAll()
    {
        foreach (var conn in NetworkServer.connections.Values)
            conn.Send(new ProceedToNextPhaseMsg());
    }

    public List<PlayerResult> ConvertGameLogToPlayerResults()
    {
        List<PlayerResult> result = new();
        HashSet<string> ownerNames = new();

        foreach (var log in gameLog)
            if (!string.IsNullOrEmpty(log.ownerName)) ownerNames.Add(log.ownerName);

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
        }

        return result;
    }

    private void OnApplicationQuit()
    {
        if (NetworkClient.isConnected) manager.StopClient();
        if (NetworkServer.active) manager.StopServer();
    }
}
