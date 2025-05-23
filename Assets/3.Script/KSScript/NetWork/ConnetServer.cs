using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class ConnetServer : MonoBehaviour
{
    private NetworkManager netMgr;
    [SerializeField] private string serverIp = "3.36.126.156"; // AWS 서버 IP
//    private string RoomSelectScene = "RoomSelectScene"; // 씬 이름 직접 할당

    private void Start()
    {
        netMgr = NetworkManager.singleton;
        if (netMgr == null)
            Debug.LogError("NetworkManager가 없습니다!");
    }

    public void ConnectToServer()
    {
        if (!NetworkServer.active && !NetworkClient.active)
        {
            netMgr.networkAddress = serverIp;
            netMgr.StartClient();
            //SceneManager.LoadScene(RoomSelectScene); // 씬 이름 직접 명시!
        }
        else
        {
            Debug.LogWarning("이미 네트워크가 활성화되어 있습니다.");
        }
    }
}
