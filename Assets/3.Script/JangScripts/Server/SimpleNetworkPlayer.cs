using Mirror;
using UnityEngine;

public class SimpleNetworkPlayer : NetworkBehaviour
{
    [SyncVar]
    public string playerName;

    [Command]
    public void CmdSendChat(string message)
    {
        RpcReceiveChat($"{playerName}: {message}");
    }

    [ClientRpc]
    void RpcReceiveChat(string msg)
    {
        Debug.Log($"💬 {msg}"); // 실제 채팅 UI에 연결하면 여기에 출력
    }
}
