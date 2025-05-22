//.net ���̺귯��
using System;
//.net���� ��Ʈ��ũ �� ��������� �ϱ� ���� ���̺귯��
using System.Net;
using System.Net.Sockets;
// �����͸� �б� / ���� �ϱ� ���� ���̺귯��
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

    //�⺻������ .net���� ���ϴ� ��Ŷ�� ������ stream

    StreamReader reader; // ������ �д� ��
    StreamWriter writer; // ������ ���� ��

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
        //���������� ���ư��� �ϸ� �޽����� ���ö����� ��������ϰ� �帧�� ����ó��
        try
        {
            TcpListener tcp = new TcpListener(IPAddress.Parse(IPadress.text), int.Parse(Port.text));
            tcp.Start();
            log.Enqueue("ServerOpen");

            TcpClient client = tcp.AcceptTcpClient();
            //TcpListener�� ����ɶ����� ��ٷȴٰ�
            ///client�� ������ �Ǹ� Tcp Client�� �Ҵ�
            log.Enqueue("client ���� Ȯ��");
            reader = new StreamReader(client.GetStream());
            writer = new StreamWriter(client.GetStream());

            writer.AutoFlush = true;

            while (client.Connected)
            {
                string readdata = reader.ReadLine();
                message.Message(readdata); // ���� �޽����� �״�� ���
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
                message.Message(readdata); // ���� �޽����� �״�� ���
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
            message.Message(formattedMessage); // �ڱ� �޽����� ���
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