using System.Collections.Generic;
using UnityEngine;

public class RoomListManager : MonoBehaviour
{
    public Transform roomListParent;
    public GameObject roomListItemPrefab;
    public EnterRoomHandler enterRoomHandler; // 인스펙터 연결

    // 전체 방 리스트 갱신
    public void RefreshRoomList(List<RoomInfo> rooms)
    {
        foreach (Transform child in roomListParent)
            Destroy(child.gameObject);

        foreach (var room in rooms)
        {
            var go = Instantiate(roomListItemPrefab, roomListParent);
            var item = go.GetComponent<RoomListItem>();
            // 참가 함수 콜백으로 enterRoomHandler.JoinRoom 사용
            item.Setup(room.title, room.userCount, room.hostIp, enterRoomHandler.JoinRoom);
        }
    }
}
