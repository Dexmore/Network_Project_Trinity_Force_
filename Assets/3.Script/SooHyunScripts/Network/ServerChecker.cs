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
        public string sentence;
        public byte[] drawing;
        public bool isText;
    }
    public List<GameTurn> gameLog = new List<GameTurn>();

    private List<string> submittedSentences = new List<string>();
    private List<NetworkPlayer> submittedPlayers = new List<NetworkPlayer>();
    private List<byte[]> submittedDrawings = new List<byte[]>();
    private List<NetworkPlayer> drawingPlayers = new List<NetworkPlayer>();
    private List<string> submittedGuesses = new List<string>();
    private List<NetworkPlayer> guessPlayers = new List<NetworkPlayer>();

    private int currentPhaseIndex = 0;
    private int maxPhases = 4; // 문장-그림-추측-그림

    private int playerCount = 0;

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
        catch (Exception e)
        {
            Debug.Log(e.Message);
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
                playerCount = 4;
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
        var allPlayers = GameObject.FindObjectsOfType<NetworkPlayer>();
        if (!submittedPlayers.Contains(player))
        {
            player.playerIndex = submittedPlayers.Count;
            submittedPlayers.Add(player);
            submittedSentences.Add(sentence);
        }
        if (submittedPlayers.Count == allPlayers.Length)
        {
            gameLog.Add(new GameTurn { isText = true, sentence = submittedSentences[0], drawing = null });
            ShowSentencesToEachPlayer();
            NextPhaseToAll();
            submittedPlayers.Clear();
            submittedSentences.Clear();
            currentPhaseIndex++;
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
        var allPlayers = GameObject.FindObjectsOfType<NetworkPlayer>();
        if (!drawingPlayers.Contains(player))
        {
            player.playerIndex = drawingPlayers.Count;
            drawingPlayers.Add(player);
            submittedDrawings.Add(pngData);
        }
        if (drawingPlayers.Count == allPlayers.Length)
        {
            gameLog.Add(new GameTurn { isText = false, sentence = null, drawing = submittedDrawings[0] });
            DistributeDrawings();
            NextPhaseToAll();
            drawingPlayers.Clear();
            submittedDrawings.Clear();
            currentPhaseIndex++;
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
        var allPlayers = GameObject.FindObjectsOfType<NetworkPlayer>();
        if (!guessPlayers.Contains(player))
        {
            player.playerIndex = guessPlayers.Count;
            guessPlayers.Add(player);
            submittedGuesses.Add(guessText);
        }
        if (guessPlayers.Count == allPlayers.Length)
        {
            gameLog.Add(new GameTurn { isText = true, sentence = submittedGuesses[0], drawing = null });
            ShowGuessesToEachPlayer();
            NextPhaseToAll();
            guessPlayers.Clear();
            submittedGuesses.Clear();
            currentPhaseIndex++;
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

    private void OnApplicationQuit()
    {
        if (NetworkClient.isConnected) manager.StopClient();
        if (NetworkServer.active) manager.StopServer();
    }
}
