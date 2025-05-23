using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class ConnetServer : MonoBehaviour
{
    private NetworkManager netMgr;
    [SerializeField] private string serverIp = "3.38.169.196"; // AWS 서버 IP

    private void Start()
    {
        netMgr = NetworkManager.singleton;
        if (netMgr == null)
            Debug.LogError("NetworkManager가 없습니다!");
    }

    public void ConnectToServer()
    {
        // 이미 연결 중이 아니면만 시도
        if (!NetworkServer.active && !NetworkClient.active)
        {
            netMgr.networkAddress = serverIp;
            netMgr.StartClient(); // 클라이언트로 연결 (서버면 StartHost)
                                  // SceneManager.LoadScene()은 Mirror의 OnlineScene 사용 시 필요 없음!
            
        }
        else
        {
            Debug.LogWarning("이미 네트워크가 활성화되어 있습니다.");
        }
    }
}
