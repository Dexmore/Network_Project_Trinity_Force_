using UnityEngine;

public class RoomSelectionUI : MonoBehaviour
{
    [Header("Inspector Binding")]
    [SerializeField] private JoinRoomHandler joinRoom;
    [SerializeField] private CreateRoomHandler createRoom;

    public void OnJoinRoomClicked()
    {
        if (joinRoom == null)
        {
            Debug.LogError("JoinRoomHandler가 할당되지 않았습니다!");
            return;
        }
        joinRoom.JoinSelectedRoom();
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
