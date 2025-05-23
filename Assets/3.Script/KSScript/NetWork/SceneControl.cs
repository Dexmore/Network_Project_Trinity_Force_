using Mirror;

public class SceneControl : NetworkBehaviour
{
    [Command]
    public void CmdSwitchToLobby()
    {
        if (!isServer) return;
        NetworkManager.singleton.ServerChangeScene("LobbyScene");
    }
}
