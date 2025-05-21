using UnityEngine;
using TMPro;

public class EnterRoomPopup : MonoBehaviour
{
    [Header("팝업 패널 & 텍스트")]
    [Tooltip("팝업 전체를 감싸는 GameObject")]
    [SerializeField] private GameObject popupPanel;
    [Tooltip("팝업 안의 제목 표시용 TMP_Text")]
    

    /// <summary>
    /// 팝업 띄우기
    /// </summary>
    /// <param name="roomName">방 제목</param>
    public void Show(string roomName)
    {
        if (popupPanel == null)
        {
            Debug.LogError("[EnterRoomPopup] 필수 UI가 할당되지 않았습니다!");
            return;
        }
        popupPanel.SetActive(true);
    }

    /// <summary>
    /// 팝업 닫기
    /// </summary>
    public void Hide()
    {
        if (popupPanel == null)
        {
            Debug.LogError("[EnterRoomPopup] popupPanel이 할당되지 않았습니다!");
            return;
        }
        popupPanel.SetActive(false);
    }
}
