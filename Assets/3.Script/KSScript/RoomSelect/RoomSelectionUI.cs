using UnityEngine;

public class RoomSelectionUI : MonoBehaviour
{
    [Header("Inspector Binding")]
    [Tooltip("처음엔 비활성화돼 있는 '방 선택' 패널")]
    [SerializeField] private GameObject selectRoomPanel;
    [SerializeField] private CreateRoomHandler createRoom;

    // 초기 '입장하기' 버튼에 이 메서드 바인딩
    public void OnJoinRoomClicked()
    {
        if (selectRoomPanel == null)
        {
            Debug.LogError("selectRoomPanel이 할당되지 않았습니다!");
            return;
        }
        // 씬 전환이 아니라, 방 목록 UI 팝업만 띄워 줍니다.
        selectRoomPanel.SetActive(true);
    }

    public void OnCreateRoomClicked()
    {
        if (createRoom == null)
        {
            Debug.LogError("CreateRoomHandler가 할당되지 않았습니다!");
            return;
        }
        createRoom.ShowCreateRoomPopup();
    }
}
