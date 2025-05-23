using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Mirror;

public class ChatUIManager : MonoBehaviour
{
    public TMP_InputField chatInput;
    public RectTransform contentTransform;
    public GameObject messagePrefab;
    private NetworkChat _localChat;

    void Start()
    {
        NetworkChat.OnChatMessage += HandleChatMessage;

        // ① onSubmit 으로 교체
        chatInput.onSubmit.AddListener(SubmitText);

        // ② 한 번만 포커스 잡아 주기
        chatInput.Select();
        chatInput.ActivateInputField();
    }


    void OnGUI()
    {
        // Event.current는 에디터, 빌드 구분 없이 키 이벤트를 줍니다
        if (Event.current.type == EventType.KeyDown &&
            (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter))
        {
            SubmitText(chatInput.text);
            // 이벤트 소비 방지
            Event.current.Use();
        }
    }

    void SubmitText(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        if (!NetworkClient.active || NetworkClient.connection?.identity == null) return;

        NetworkClient.connection.identity
            .GetComponent<NetworkChat>()
            .CmdSendMessage(text);

        chatInput.text = "";
        chatInput.ActivateInputField();
    }

    void HandleChatMessage(string message, int senderId)
    {
        var go = Instantiate(messagePrefab, contentTransform);
        go.GetComponent<TMP_Text>().text = $"[Player{senderId}]: {message}";
        Canvas.ForceUpdateCanvases();
        var sv = contentTransform.GetComponentInParent<ScrollRect>();
        sv.verticalNormalizedPosition = 0f;
    }

    void OnDestroy()
    {
        NetworkChat.OnChatMessage -= HandleChatMessage;
    }
}
