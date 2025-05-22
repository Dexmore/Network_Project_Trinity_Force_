using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessagePooling : MonoBehaviour
{
    [SerializeField] private Text[] Message_Box;

    public Action<string> Message;

    private string current_message = string.Empty;
    private string past_message;


    private void Start()
    {
        Message_Box = transform.GetComponentsInChildren<Text>();
        Message = AddingMessage;
        past_message = current_message;
    }

    private void Update()
    {
        if (past_message.Equals(current_message)) return;
        ReadText(current_message);
        past_message = current_message;

    }
    public void AddingMessage(string message)
    {
        current_message = message;
    }

    public void ReadText(string message)
    {
        bool isInput = false;
        for (int i = 0; i < Message_Box.Length; i++)
        {
            if (Message_Box[i].text.Equals(""))
            {
                Message_Box[i].text = message;
                isInput = true;
                break;
            }
        }

        if (!isInput)
        {
            for (int i = 1; i < Message_Box.Length; i++)
            {
                // 메세지 미는 작업
                Message_Box[i - 1].text = Message_Box[i].text;
            }
            Message_Box[Message_Box.Length - 1].text = message;
        }
    }
}