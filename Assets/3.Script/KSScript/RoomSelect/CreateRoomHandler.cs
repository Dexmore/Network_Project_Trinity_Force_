using UnityEngine;
using Mirror;
using TMPro;
using System.Collections;

public class CreateRoomHandler : MonoBehaviour
{
    [SerializeField] private string serverIp = "3.36.126.156";
    private NetworkManager netMgr;

    void Awake()
    {
        netMgr = NetworkManager.singleton;
        if (netMgr == null)
            Debug.LogError("NetworkManager가 씬에 없습니다!");
    }

    public void ConfirmCreateRoom()

    {
        OnGoToLobbyButton();
    }
    public void OnGoToLobbyButton()
    {
        // 연결이 완료된 후에만 호출하세요
        if (!NetworkClient.isConnected) return;

        // 씬에 배치된 SceneControl 오브젝트 찾기
        var sceneCtrl = NetworkClient.connection.identity
                           .GetComponent<SceneControl>();
        sceneCtrl.CmdSwitchToLobby();
    }


}
