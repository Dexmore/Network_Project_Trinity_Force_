using Mirror;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System.IO;
using kcp2k;
using System;

public enum Type { Empty = 0, Client, Server }

public class Item
{
    public string Lisence;
    public string ServerIP;
    public string Port;
    public Item(string L_index, string IPValue, string port)
    {
        Lisence = L_index;
        ServerIP = IPValue;
        Port = port;
    }
}

public class ServerChecker : MonoBehaviour
{
    public Type type;
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
        public bool isText;
        public string guess;
    }
    public List<GameTurn> gameLog = new List<GameTurn>();

    private List<string> submittedSentences = new List<string>();
    private List<NetworkPlayer> submittedPlayers = new List<NetworkPlayer>();
    private List<byte[]> submittedDrawings = new List<byte[]>();
    private List<NetworkPlayer> drawingPlayers = new List<NetworkPlayer>();
    private List<string> submittedGuesses = new List<string>();
    private List<NetworkPlayer> guessPlayers = new List<NetworkPlayer>();

    private int playerCount = 4;

    private void OnEnable()
    {
        path = Application.dataPath + "/License";
        if (!File.Exists(path)) Directory.CreateDirectory(path);
        if (!File.Exists(path + "/License.Json")) DefaultData(path);
        manager = GetComponent<NetworkManager>();
        kcp = (KcpTransport)manager.transport;
    }

    private void Start()
    {
        type = License_Type();
        if (type.Equals(Type.Server)) Start_Server();
        else Start_Client();
    }

    private void DefaultData(string path)
    {
        List<Item> item = new List<Item>();
        item.Add(new Item("0", "127.0.0.1", "7777"));
        JsonData data = JsonMapper.ToJson(item);
        File.WriteAllText(path + "/License.json", data.ToString());
    }

    private Type License_Type()
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
            type = (Type)Enum.Parse(typeof(Type), type_s);

            manager.networkAddress = ServerIP;
            kcp.port = ushort.Parse(Port);

            return type;
        }
        catch (Exception)
        {
            return Type.Empty;
        }
    }

    public void Start_Server()
    {
        manager.StartServer();
        NetworkServer.OnConnectedEvent += (conn) =>
        {
            if (players.Count >= 4)
            {
                conn.Disconnect();
                return;
            }
            if (!players.Contains(conn)) players.Add(conn);

            if (players.Count == 4)
            {
                foreach (var c in players) c.Send(new GameStartMsg());
            }
        };
        NetworkServer.OnDisconnectedEvent += (conn) =>
        {
            if (players.Contains(conn)) players.Remove(conn);
        };
    }

    public void Start_Client() { manager.StartClient(); }

    public void AddSentence(NetworkPlayer player, string sentence)
    {
        if (!submittedPlayers.Contains(player))
        {
            player.playerIndex = submittedPlayers.Count;
            submittedPlayers.Add(player);
            submittedSentences.Add(sentence);
        }
        if (submittedPlayers.Count == playerCount)
        {
            for (int i = 0; i < playerCount; i++)
            {
                string pn = submittedPlayers[i]?.playerName ?? $"Player{i + 1}";
                string st = !string.IsNullOrEmpty(submittedSentences[i]) ? submittedSentences[i] : $"문장{i + 1}";
                gameLog.Add(new GameTurn
                {
                    isText = true,
                    sentence = st,
                    playerName = pn
                });
            }
            ShowSentencesToEachPlayer();
            NextPhaseToAll();
            submittedPlayers.Clear();
            submittedSentences.Clear();
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

    public void AddDrawing(NetworkPlayer player, byte[] pngData)
    {
        if (!drawingPlayers.Contains(player))
        {
            player.playerIndex = drawingPlayers.Count;
            drawingPlayers.Add(player);
            submittedDrawings.Add(pngData);
        }
        if (drawingPlayers.Count == playerCount)
        {
            for (int i = 0; i < playerCount; i++)
            {
                string pn = drawingPlayers[i]?.playerName ?? $"Player{i + 1}";
                byte[] dr = submittedDrawings[i] != null ? submittedDrawings[i] : new byte[0];
                gameLog.Add(new GameTurn
                {
                    isText = false,
                    drawing = dr,
                    playerName = pn
                });
            }
            DistributeDrawings();
            NextPhaseToAll();
            drawingPlayers.Clear();
            submittedDrawings.Clear();
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

    public void AddGuess(NetworkPlayer player, string guessText)
    {
        if (!guessPlayers.Contains(player))
        {
            player.playerIndex = guessPlayers.Count;
            guessPlayers.Add(player);
            submittedGuesses.Add(guessText);
        }
        if (guessPlayers.Count == playerCount)
        {
            for (int i = 0; i < playerCount; i++)
            {
                string pn = guessPlayers[i]?.playerName ?? $"Player{i + 1}";
                string gs = !string.IsNullOrEmpty(submittedGuesses[i]) ? submittedGuesses[i] : $"추측{i + 1}";
                gameLog.Add(new GameTurn
                {
                    isText = true,
                    sentence = gs,
                    playerName = pn,
                    guess = gs
                });
            }
            ShowGuessesToEachPlayer();
            NextPhaseToAll();
            guessPlayers.Clear();
            submittedGuesses.Clear();
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
                guess = gameLog[offset + 2].guess ?? gameLog[offset + 2].sentence,
                drawing2 = gameLog[offset + 3].drawing
            };
            result.Add(playerResult);
        }
        // 부족하면 더미라도 4개 채워서 빈 결과 방지
        while (result.Count < 4)
        {
            result.Add(new PlayerResult
            {
                playerName = $"Player{result.Count + 1}",
                sentence = "(비어있음)",
                drawing1 = new byte[0],
                guess = "(비어있음)",
                drawing2 = new byte[0]
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
