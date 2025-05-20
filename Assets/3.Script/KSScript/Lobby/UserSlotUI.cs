// Assets/3.Script/KSScript/UserSlotUI.cs
using UnityEngine;
using TMPro;

public class UserSlotUI : MonoBehaviour
{
    public TMP_Text nicknameText;
    public TMP_Text statusText;

    public bool IsReady { get; private set; }

    public void Initialize(string nickname, bool isReady)
    {
        nicknameText.text = nickname;
        SetReadyState(isReady);
    }

    // 준비 토글 반영
    public void SetReadyState(bool isReady)
    {
        IsReady = isReady;
        statusText.text = isReady ? "Ready" : "Waiting";
        // 필요하면 색상이나 아이콘 변경도 여기서
    }
}
