using System;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class ChatManager : MonoBehaviour
{
    [Header("User Settings")]
    [Tooltip("내 채팅에 표시될 유저 닉네임")]
    public string localUserName = "User";

    [Header("UI References")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private RectTransform chatField;
    [SerializeField] private GameObject messagePrefab;

    [Header("Layout Settings")]
    [Tooltip("메시지들 사이 간격")]
    [SerializeField] private float messageSpacing = 10f;
    [Tooltip("상단 삭제할 때 남길 여유(px)")]
    [SerializeField] private float deleteMargin = 20f;

    private List<RectTransform> messages = new List<RectTransform>();
    private Vector2 startPos;

    private void Awake()
    {
        // ChatField 피벗이 (0,0)이라면,
        // 좌하단 기준으로 +padding 만큼 올려서 찍기
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

        AddMessage(text);

        inputField.text = "";
        inputField.ActivateInputField();
    }

    private void AddMessage(string text)
    {
        // 1) 메시지 생성
        var go = Instantiate(messagePrefab, chatField, false);
        var rtNew = go.GetComponent<RectTransform>();
        var tmp = go.GetComponentInChildren<TMP_Text>();
        if (tmp == null)
        {
            Debug.LogError("MessagePrefab에 TMP_Text 컴포넌트가 없습니다.");
            Destroy(go);
            return;
        }

        // 2) 타임스탬프 생성
        string timestamp = DateTime.Now.ToString("HH:mm:ss");

        // 3) [시간] 유저이름 : 채팅내용 포맷
        tmp.richText = true;
        tmp.text = $"<size=80%>[{timestamp}]</size> {localUserName} : {text}";

        // 4) 레이아웃 계산 및 배치
        Canvas.ForceUpdateCanvases();
        float h = rtNew.rect.height;
        float shift = h + messageSpacing;
        foreach (var rt in messages)
            rt.anchoredPosition += Vector2.up * shift;
        rtNew.anchoredPosition = startPos;
        messages.Insert(0, rtNew);

        // 5) 화면 위로 벗어난 메시지 삭제
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
