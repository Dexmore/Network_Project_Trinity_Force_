using Mirror;
using UnityEngine;

public class NetworkChat : NetworkBehaviour
{
    // 채팅 이벤트
    public delegate void ChatMessageHandler(string message, string senderName);
    public static event ChatMessageHandler OnChatMessage;

    // 로비 정보
    [SyncVar(hook = nameof(OnNicknameChanged))]
    public string playerName;

    [SyncVar(hook = nameof(OnReadyChanged))]
    public bool isReady;

    // ✅ 채팅 커맨드
    [Command]
    public void CmdSendMessage(string message, string senderName)
    {
        RpcReceiveMessage(message, senderName);
    }

    [ClientRpc]
    void RpcReceiveMessage(string message, string senderName)
    {
        OnChatMessage?.Invoke(message, senderName);
    }

    // ✅ 닉네임 전달
    public override void OnStartLocalPlayer()
    {
        string nick = SQLManager.instance?.info?.User_Nickname ?? "Unknown";
        CmdSetNickname(nick);
    }

    [Command]
    public void CmdSetNickname(string nick)
    {
        playerName = nick;
    }

    void OnNicknameChanged(string _, string newName)
    {
        if (LobbyUserManager.Instance != null)
        {
            LobbyUserManager.Instance.AddUser(newName, isReady);
        }
        else
        {
            Debug.LogWarning("[NetworkChat] LobbyUserManager.Instance is null during OnNicknameChanged");
        }
    }

    void OnReadyChanged(bool _, bool newVal)
    {
        if (LobbyUserManager.Instance != null)
        {
            LobbyUserManager.Instance.UpdateNicknameReady(playerName, newVal);
        }
    }

    [Command]
    public void CmdToggleReady()
    {
        isReady = !isReady;
    }
}
