using UnityEngine;
using TMPro;
using Mirror;
using UnityEngine.SceneManagement;


public class CreateRoomHandler : MonoBehaviour
{
    [SerializeField] private GameObject createRoomPopup;
    [SerializeField] private TMP_InputField roomNameInput;

    private NetworkManager netMgr;
    private string serverIp = "3.38.169.196"; // 실제 서버 IP

    void Awake()
    {
        netMgr = NetworkManager.singleton;
        if (netMgr == null)
            Debug.LogError("NetworkManager가 없습니다!");
    }

    // void Start()
    // {

    //     createRoomPopup.SetActive(false);
    // }

    public void ShowCreateRoomPopup()
    {
        createRoomPopup.SetActive(true);
        roomNameInput.text = "";
        roomNameInput.ActivateInputField();
    }

    public void HideCreateRoomPopup()
    {
        createRoomPopup.SetActive(false);
    }

    public void ConfirmCreateRoom()
    {
        // string title = roomNameInput.text.Trim();
        // if (string.IsNullOrEmpty(title))
        // {
        //     Debug.LogWarning("방 제목을 입력해주세요.");
        //     return;
        // }

        // RoomInfoName.CurrentRoomTitle = title;

        // // **호스트/클라/서버 모두 동작중이 아니면만 StartHost 실행**
        // if (!NetworkServer.active && !NetworkClient.active)
        // {
        //     netMgr.networkAddress = serverIp;
        //     netMgr.StartHost()
        // }
        // else
        // {
        //     Debug.LogWarning("이미 네트워크가 활성화되어 있습니다.");
        // }

        // createRoomPopup.SetActive(false);
        SceneManager.LoadScene("LobbyScene");
    }
}
