using UnityEngine;
using TMPro;

public class NicknameUIManager : MonoBehaviour
{
    [SerializeField] private TMP_Text nicknameText;
    [SerializeField] private TMP_Text statusText;

    private void Start()
    {
        // 닉네임 표시
        if (SQLManager.instance != null && SQLManager.instance.info != null)
        {
            nicknameText.text = $"닉네임: {SQLManager.instance.info.User_Nickname}";
        }
        else
        {
            nicknameText.text = "닉네임: 없음";
        }

        // 초기 상태 설정
        statusText.text = "상태: 준비 전";
    }

    // 상태 변경용 메소드
    public void SetReadyState(bool isReady)
    {
        statusText.text = isReady ? "상태: 준비 완료" : "상태: 준비 전";
    }
}
