using UnityEngine;

public class CreateRoomHandler : MonoBehaviour
{
    [Header("Popup Panel")]
    [Tooltip("최초에 비활성화 상태로 두세요")]
    public GameObject createRoomPopup;

    void Start()
    {
        // 게임 시작 시 팝업 숨기기
        if (createRoomPopup != null)
            createRoomPopup.SetActive(false);
    }

    /// <summary>
    /// “방 만들기” 버튼 OnClick() → 이 메서드를 바인딩
    /// </summary>
    public void ShowCreateRoomPopup()
    {
        if (createRoomPopup != null)
            createRoomPopup.SetActive(true);
    }

    /// <summary>
    /// 팝업 안의 “취소” 버튼 OnClick() → 이 메서드를 바인딩
    /// </summary>
    public void HideCreateRoomPopup()
    {
        if (createRoomPopup != null)
            createRoomPopup.SetActive(false);
    }
    public void CloseUi()
    {
        createRoomPopup.SetActive(false);
    }
}
