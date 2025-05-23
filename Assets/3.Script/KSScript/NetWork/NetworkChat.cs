// NetworkChat.cs (Player Prefab)
using Mirror;
using UnityEngine;

public class NetworkChat : NetworkBehaviour
{
    public delegate void ChatMessageHandler(string message, int senderId);
    public static event ChatMessageHandler OnChatMessage;

    [Command]
    public void CmdSendMessage(string message)
    {
        RpcReceiveMessage(message, connectionToClient.connectionId);
    }

    [ClientRpc]
    void RpcReceiveMessage(string message, int senderId)
    {
        OnChatMessage?.Invoke(message, senderId);
    }
}
