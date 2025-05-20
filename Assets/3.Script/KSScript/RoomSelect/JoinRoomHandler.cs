using UnityEngine;

public class JoinRoomHandler : MonoBehaviour
{
    // “입장하기” 버튼 OnClick() → 이 메서드 바인딩
    public void JoinSelectedRoom()
    {
        Debug.Log("[JoinRoomHandler] 서버에 방 입장 요청");
        // TODO: 선택된 방 ID를 가져와서 서버에 요청 → 성공 시 씬 전환
    }
}
