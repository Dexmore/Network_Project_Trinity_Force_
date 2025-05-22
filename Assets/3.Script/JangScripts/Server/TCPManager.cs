//.net 라이브러리
using System;
//.net에서 네트워크 및 소켓통신을 하기 위한 라이브러리
using System.Net;
using System.Net.Sockets;
// 데이터를 읽기 / 쓰기 하기 위한 라이브러리
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;

public class TCPManager : MonoBehaviour
{
    public InputField IPadress;
    public InputField Port;
    [SerializeField] private Text t_log;

    //기본적으로 .net에서 말하는 패킷의 단위는 stream

    StreamReader reader; // 데이터 읽는 놈
    StreamWriter writer; // 데이터 쓰는 놈

    public InputField Message_Input;
    private MessagePooling message;

    private Queue<string> log = new Queue<string>();

    private void Log_Message()
    {
        if (log.Count > 0)
        {
            t_log.text = log.Dequeue();
        }
    }

    #region Server

    public void Server_Open()
    {
        message = FindAnyObjectByType<MessagePooling>();
        Thread th = new Thread(ServerConnect);
        th.IsBackground = true;
        th.Start();
    }

    private void ServerConnect()
    {
        //지속적으로 돌아가야 하며 메시지가 들어올때마다 열어줘야하고 흐름에 예외처리
        try
        {
            TcpListener tcp = new TcpListener(IPAddress.Parse(IPadress.text), int.Parse(Port.text));
            tcp.Start();
            log.Enqueue("ServerOpen");

            TcpClient client = tcp.AcceptTcpClient();
            //TcpListener에 연결될때까지 기다렸다가
            ///client가 연결이 되면 Tcp Client에 할당
            log.Enqueue("client 접속 확인");
            reader = new StreamReader(client.GetStream());
            writer = new StreamWriter(client.GetStream());

            writer.AutoFlush = true;

            while (client.Connected)
            {
                string readdata = reader.ReadLine();
                message.Message(readdata); // 받은 메시지를 그대로 출력
            }
        }

        catch (Exception e)
        {
            log.Enqueue(e.Message);
        }
    }
    #endregion

    #region Client
    public void client_Connect()
    {
        message = FindAnyObjectByType<MessagePooling>();
        log.Enqueue("client connect");
        Thread th = new Thread(client_connect);
        th.IsBackground = true;
        th.Start();
    }

    public void client_connect()
    {
        try
        {
            TcpClient client = new TcpClient();
            IPEndPoint iPEnd = new IPEndPoint(IPAddress.Parse(IPadress.text), int.Parse(Port.text));

            client.Connect(iPEnd);
            log.Enqueue("Server connect_Complete");

            reader = new StreamReader(client.GetStream());
            writer = new StreamWriter(client.GetStream());
            writer.AutoFlush = true;

            while (client.Connected)
            {
                string readdata = reader.ReadLine();
                message.Message(readdata); // 받은 메시지를 그대로 출력
            }
        }
        catch (Exception e)
        {
            log.Enqueue(e.Message);
        }
    }
    #endregion

    public void Sending_btn()
    {
        string formattedMessage = $"{SQLManager1.instance.info.User_UID} : {Message_Input.text}";

        if (Sendingmessage(formattedMessage))
        {
            message.Message(formattedMessage); // 자기 메시지도 출력
            Message_Input.text = string.Empty;
        }
    }


    private bool Sendingmessage(string message)
    {
        if (writer != null)
        {
            writer.WriteLine(message);
            return true;
        }
        else
        {
            log.Enqueue("Writer Null!!");
            return false;
        }
    }

    private void Update()
    {
        Log_Message();
    }
}