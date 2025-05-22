using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RoomListItem : MonoBehaviour
{
    public TMP_Text roomNameText;
    public TMP_Text playerCountText;
    public Button joinButton;

    private string hostIp;

    // Room 정보 세팅
    public void Setup(string roomName, int userCount, string hostIpAddress, System.Action<string> onJoin)
    {
        roomNameText.text = roomName;
        playerCountText.text = $"{userCount}/4";
        hostIp = hostIpAddress;

        // 기존 리스너 제거 (중복 방지)
        joinButton.onClick.RemoveAllListeners();
        // JoinButton 클릭 시, onJoin 콜백에 내 hostIp 전달
        joinButton.onClick.AddListener(() => onJoin?.Invoke(hostIp));
    }
}
