using Mirror;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System.IO;
using kcp2k;
using System;

public enum Type1
{
    Empty = 0,
    Client,
    Server
}

/*
 
 [
    {
        "License": "2",
        "Server_IP": "3.36.74.143",
        "Port": "7777"
    }
]
 */

public class Item1
{
    public string Lisence;
    public string ServerIP;
    public string Port;

    public Item1(string L_index, string IPValue, string port)
    {
        Lisence = L_index;
        ServerIP = IPValue;
        Port = port;
    }
}
public class ServerChecker1 : MonoBehaviour
{
    public Type1 type;

    private NetworkManager manager;
    private KcpTransport kcp;

    [SerializeField] private string path;

    public string ServerIP { get; private set; }
    public string Port { get; private set; }

    private void OnEnable()
    {
        path = Application.dataPath + "/License";

        // 폴더 검사
        if (!File.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        // JSON 파일 있는지 검사
        if (!File.Exists(path + "/License.Json"))
        {
            DefaultData(path);
        }
        manager = GetComponent<NetworkManager>();
        kcp = (KcpTransport)manager.transport;
    }

    private void DefaultData(string path)
    {
        //JSON 만드는 작업
        List<Item1> item = new List<Item1>();
        item.Add(new Item1("0", "127.0.0.1", "7777"));

        JsonData data = JsonMapper.ToJson(item);
        File.WriteAllText(path + "/License.json", data.ToString());
    }

    private Type1 License_Type()
    {
        Type1 t = Type1.Empty;
        try
        {
            string jsonString = File.ReadAllText(path + "/License.json");
            JsonData itemdata = JsonMapper.ToObject(jsonString);

            string type_s = itemdata[0]["License"].ToString();
            string ip_s = itemdata[0]["ServerIP"].ToString();
            string port_s = itemdata[0]["Port"].ToString();

            ServerIP = ip_s;
            Port = port_s;
            type = (Type1)Enum.Parse(typeof(Type1), type_s);

            manager.networkAddress = ServerIP;
            kcp.port = ushort.Parse(Port);

            return type;
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            return Type1.Empty;
        }
    }

    private void Start()
    {
        type = License_Type();

        if (type.Equals(Type1.Server))
        {
            Start_Server();
        }
        else
        {
            Start_Client();
        }
    }

    public void Start_Server()
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            Debug.Log("WebGL cannot be Server");
        }
        else
        {
            manager.StartServer();
            Debug.Log($"{manager.networkAddress} Start Server");
            NetworkServer.OnConnectedEvent += (NetworkConnectionToClient) =>
            {
                Debug.Log($"Network Client Connect : {NetworkConnectionToClient.address}");
            };

            NetworkServer.OnDisconnectedEvent += (NetworkConnectionToClient) =>
            {
                Debug.Log($"Network Client DisConnect : {NetworkConnectionToClient.address}");
            };
        }
    }

    public void Start_Client()
    {
        manager.StartClient();
        Debug.Log($"{manager.networkAddress} : Start Client...");
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
