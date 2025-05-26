using UnityEngine;
using Mirror;

public class ReadyButtonUI : MonoBehaviour
{
    public void OnReadyClicked()
    {
        if (NetworkClient.connection != null &&
            NetworkClient.connection.identity != null)
        {
            NetworkPlayer player = NetworkClient.connection.identity.GetComponent<NetworkPlayer>();
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
}
