using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class EnterRoomHandler : MonoBehaviour
{
    [SerializeField] private string sceneName = "LobbyScene";

    // 실제 참가 (네트워크 연결)
    public void JoinRoom(string hostIp)
    {
        if (!NetworkClient.active)
        {
            NetworkManager.singleton.networkAddress = hostIp;
            NetworkManager.singleton.StartClient();
            SceneManager.LoadScene(sceneName); // 연결 후 씬 이동
        }
        else
        {
            Debug.LogWarning("이미 네트워크가 활성화되어 있습니다.");
        }
    }
}
