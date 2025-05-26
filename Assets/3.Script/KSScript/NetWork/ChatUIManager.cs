using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;
using System;

public class ChatUIManager : MonoBehaviour
{
    public TMP_InputField chatInput;
    public RectTransform contentTransform;
    public GameObject messagePrefab;

    void Start()
    {
        NetworkChat.OnChatMessage += HandleChatMessage;

        // onEndEdit 대신 onSubmit
        chatInput.onSubmit.AddListener(SubmitText);

        chatInput.ActivateInputField();
    }
    void OnEndEditSubmit(string text)
    {
        // Enter키로 입력 마쳤을 때만
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            SubmitText(text);
    }

    void OnDestroy()
    {
        NetworkChat.OnChatMessage -= HandleChatMessage;
    }

    void SubmitText(string text)
    {
       // Debug.Log($"[ChatUIManager] SubmitText 호출 → “{text}”");
        if (string.IsNullOrWhiteSpace(text)) return;
        if (!NetworkClient.active || NetworkClient.connection?.identity == null) return;

        //Debug.Log($"[ChatUIManager] CmdSendMessage 전송, nick={SQLManager.instance.info.User_Nickname}");
        var nc = NetworkClient.connection.identity.GetComponent<NetworkChat>();
        nc.CmdSendMessage(text, SQLManager.instance.info.User_Nickname);

        chatInput.text = "";
        chatInput.ActivateInputField();
    }

    void HandleChatMessage(string message, string senderName)
    {
        var go = Instantiate(messagePrefab, contentTransform);
        var tmp = go.GetComponent<TMP_Text>();

        string timeStamp = DateTime.Now.ToString("HH:mm");
        tmp.text = $"[{senderName}]: {message} <size=70%><color=#BBBBBB>({timeStamp})</color></size>";

        Canvas.ForceUpdateCanvases();
        contentTransform.GetComponentInParent<ScrollRect>().verticalNormalizedPosition = 0f;
    }
}
