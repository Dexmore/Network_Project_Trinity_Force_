using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

public class ButtonUI : MonoBehaviour
{
    // ✅ 준비 버튼 처리
    public void OnReadyClicked()
    {
        if (NetworkClient.connection != null &&
            NetworkClient.connection.identity != null)
        {
            NetworkChat player = NetworkClient.connection.identity.GetComponent<NetworkChat>();
            if (player != null)
            {
                player.CmdToggleReady();
            }
        }
        else
        {
            Debug.LogWarning("로컬 플레이어를 찾을 수 없습니다.");
        }
    }

    // ✅ 방 나가기 버튼 처리
    public void OnExitClicked()
    {
        Debug.Log("[ButtonUI] 방 나가기");

        if (NetworkServer.active && NetworkClient.isConnected)
        {
            // 호스트인 경우 (서버+클라)
            NetworkManager.singleton.StopHost(); // 자동으로 OfflineScene으로 감
        }
        else if (NetworkClient.isConnected)
        {
            // 클라이언트만 연결된 경우
            NetworkManager.singleton.StopClient(); // 역시 OfflineScene으로 감
        }
    }

}
