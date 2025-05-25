using Mirror;
using UnityEngine;

public class NetworkChat : NetworkBehaviour
{
    public delegate void ChatMessageHandler(string message, string senderName);
    public static event ChatMessageHandler OnChatMessage;

    // SyncVar 완전 삭제!
    // [SyncVar] public string playerName;

    // 더 이상 OnStartLocalPlayer도 필요없습니다.

    // 메시지와 보낸 사람 이름을 같이 커맨드로!
    [Command]
    public void CmdSendMessage(string message, string senderName)
    {
        // 서버 → 모든 클라이언트에게 메시지와 이름을 바로 뿌려줍니다.
        RpcReceiveMessage(message, senderName);
    }

    [ClientRpc]
    void RpcReceiveMessage(string message, string senderName)
    {
        OnChatMessage?.Invoke(message, senderName);
    }
}
