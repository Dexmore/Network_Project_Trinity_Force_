// Assets/3.Script/KSScript/UserSlotUI.cs
using UnityEngine;
using TMPro;

public class UserSlotUI : MonoBehaviour
{
    [Header("Inspector에 연결")]
    public TMP_Text nicknameText;  
    public TMP_Text statusText;

    /// <summary>
    /// 슬롯 초기화
    /// </summary>
    public void Initialize(string nickname, bool isReady)
    {
        nicknameText.text = nickname;
        statusText.text   = isReady ? "Ready" : "Waiting";
    }
}
