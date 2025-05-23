using System;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using Mirror;

public class ChatManager : NetworkBehaviour
{
    [Header("User Settings")]
    [Tooltip("내 채팅에 표시될 유저 닉네임")]
    public string localUserName = "User";

    [Header("UI References")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private RectTransform chatField;
    [SerializeField] private GameObject messagePrefab;

    [Header("Layout Settings")]
    [SerializeField] private float messageSpacing = 10f;
    [SerializeField] private float deleteMargin = 20f;

    private List<RectTransform> messages = new List<RectTransform>();
    private Vector2 startPos;

    private void Awake()
    {
        float paddingX = 10f;
        float paddingY = 10f;
        startPos = new Vector2(paddingX, paddingY);
    }

    private void Start()
    {
        inputField.onSubmit.AddListener(OnSubmit);
        inputField.DeactivateInputField();
    }

    private void Update()
    {
        if (!inputField.isFocused && Keyboard.current.enterKey.wasPressedThisFrame)
        {
            inputField.ActivateInputField();
            inputField.Select();
        }
    }

    private void OnSubmit(string text)
    {
        text = text.Trim();
        if (string.IsNullOrEmpty(text))
        {
            inputField.text = "";
            inputField.ActivateInputField();
            return;
        }

        // 네트워크로 채팅 전송 (로컬 말고 네트워크 전체에 브로드캐스트)
        CmdSendChat(text, localUserName);

        inputField.text = "";
        inputField.ActivateInputField();
    }

    // 서버로 메시지 전송
    [Command(requiresAuthority = false)]
    private void CmdSendChat(string text, string sender)
    {
        RpcReceiveChat(text, sender, DateTime.Now.ToString("HH:mm:ss"));
    }

    // 모든 클라이언트에 메시지 동기화
    [ClientRpc]
    private void RpcReceiveChat(string text, string sender, string timestamp)
    {
        AddMessage(text, sender, timestamp);
    }

    // AddMessage 오버로드
    private void AddMessage(string text)
    {
        AddMessage(text, localUserName, DateTime.Now.ToString("HH:mm:ss"));
    }

    // 네트워크용 AddMessage
    private void AddMessage(string text, string sender, string timestamp)
    {
        var go = Instantiate(messagePrefab, chatField, false);
        var rtNew = go.GetComponent<RectTransform>();
        var tmp = go.GetComponentInChildren<TMP_Text>();
        if (tmp == null)
        {
            Debug.LogError("MessagePrefab에 TMP_Text 컴포넌트가 없습니다.");
            Destroy(go);
            return;
        }

        tmp.richText = true;
        tmp.text = $"<size=80%>[{timestamp}]</size> {sender} : {text}";

        Canvas.ForceUpdateCanvases();
        float h = rtNew.rect.height;
        float shift = h + messageSpacing;
        foreach (var rt in messages)
            rt.anchoredPosition += Vector2.up * shift;
        rtNew.anchoredPosition = startPos;
        messages.Insert(0, rtNew);

        float threshold = chatField.rect.height + deleteMargin;
        for (int i = messages.Count - 1; i >= 0; i--)
        {
            if (messages[i].anchoredPosition.y >= threshold)
            {
                Destroy(messages[i].gameObject);
                messages.RemoveAt(i);
            }
            else break;
        }
    }
}
