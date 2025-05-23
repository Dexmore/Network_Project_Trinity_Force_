using Mirror;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System.IO;
using kcp2k;
using System;

public enum Type
{
    Empty = 0,
    Client,
    Server
}

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

    private List<string> submittedSentences = new List<string>();
    private List<NetworkPlayer> submittedPlayers = new List<NetworkPlayer>();

    private void OnEnable()
    {
        path = Application.dataPath + "/License";
        if (!File.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        if (!File.Exists(path + "/License.Json"))
        {
            DefaultData(path);
        }
        manager = GetComponent<NetworkManager>();
        kcp = (KcpTransport)manager.transport;
    }

    private void Start()
    {
        type = License_Type();

        if (type.Equals(Type.Server))
        {
            Start_Server();
        }
        else
        {
            Start_Client();
        }
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
        Debug.Log($"{manager.networkAddress} Start Server");

        NetworkServer.OnConnectedEvent += (conn) =>
        {
            if (players.Count >= 4)
            {
                Debug.LogWarning("Max player reached. Connection rejected.");
                conn.Disconnect();
                return;
            }

            if (!players.Contains(conn))
            {
                players.Add(conn);
                Debug.Log($"Network Client Connected : {conn.address} // Current Player Count : {players.Count}");
            }

            if (players.Count == 4)
            {
                Debug.Log("Game START!");
                foreach (var c in players)
                {
                    c.Send(new GameStartMsg());
                }
            }
        };

        NetworkServer.OnDisconnectedEvent += (conn) =>
        {
            if (players.Contains(conn))
            {
                players.Remove(conn);
                Debug.Log($"Network Client Disconnected : {conn.address} // Current Player Count : {players.Count}");
            }
        };
    }

    public void Start_Client()
    {
        manager.StartClient();
        Debug.Log($"{manager.networkAddress} : Start Client...");
    }

    // Assign playerIndex in the order they submit
    public void AddSentence(NetworkPlayer player, string sentence)
    {
        if (!submittedPlayers.Contains(player))
        {
            player.playerIndex = submittedPlayers.Count;
            submittedPlayers.Add(player);
            submittedSentences.Add(sentence);
            Debug.Log($"Player {player.playerIndex} registered. Sentence: {sentence}");
        }
    }

    public void CheckAllSubmitted()
    {
        var allPlayers = GameObject.FindObjectsOfType<NetworkPlayer>();
        foreach (var p in allPlayers)
        {
            if (!p.HasSubmitted)
                return;
        }

        Debug.Log("All players submitted / Next Phase");

        ShowSentencesToEachPlayer();

        foreach (var conn in NetworkServer.connections.Values)
        {
            conn.Send(new ProceedToNextPhaseMsg());
        }

        foreach (var p in allPlayers)
        {
            p.HasSubmitted = false;
        }

        submittedPlayers.Clear();
        submittedSentences.Clear();
    }

    // Distribute messages one to the right (circle)
    public void ShowSentencesToEachPlayer()
    {
        int count = submittedPlayers.Count;
        Debug.Log("===== [Server] Sentence Distribution Begin =====");
        for (int i = 0; i < count; i++)
        {
            int targetIndex = (i+1) % count;
            NetworkPlayer targetPlayer = submittedPlayers[targetIndex];
            string sentence = submittedSentences[i];
            Debug.Log($"{i}th sentence({sentence}) -> {targetIndex}th player (playerIndex:{targetPlayer.playerIndex}) netId:{targetPlayer.netId}");

            // Debug network object status
            if (targetPlayer == null)
                Debug.LogError($"targetPlayer is null: {targetIndex}");
            else if (targetPlayer.connectionToClient == null)
                Debug.LogError($"connectionToClient is null: {targetIndex}");

            targetPlayer.TargetShowSentence(targetPlayer.connectionToClient, sentence);
        }
    }

    private void OnApplicationQuit()
    {
        if (NetworkClient.isConnected)
        {
            manager.StopClient();
        }
        if (NetworkServer.active)
        {
            manager.StopServer();
        }
    }
}
