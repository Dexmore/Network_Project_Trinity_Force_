using Mirror;
using UnityEngine;
using System.Collections; // ← 코루틴 쓰려면 필요

public class NetworkChat : NetworkBehaviour
{
    public delegate void ChatMessageHandler(string message, string senderName);
    public static event ChatMessageHandler OnChatMessage;

    [SyncVar(hook = nameof(OnReadyChanged))] public bool isReady;
    [SyncVar(hook = nameof(OnNicknameChanged))] public string playerName;

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

    public override void OnStartClient()
    {
        base.OnStartClient();
        StartCoroutine(WaitAndRegister());
    }


    private IEnumerator WaitAndRegister()
    {
        // ① 로비 매니저가 준비될 때까지 기다림
        yield return new WaitUntil(() => LobbyUserManager.Instance != null);

        // ② playerName이 유효해질 때까지 기다림
        yield return new WaitUntil(() => !string.IsNullOrEmpty(playerName));

        // ③ 이제 안전하게 호출
        LobbyUserManager.Instance.AddUser(playerName, isReady);
    }

    void OnReadyChanged(bool _, bool newVal)
    {
        if (LobbyUserManager.Instance != null)
        {
            LobbyUserManager.Instance.UpdateNicknameReady(playerName, newVal);
        }
    }

    void OnNicknameChanged(string _, string __) { }

    [Command]
    public void CmdToggleReady()
    {
        isReady = !isReady;
    }

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
}
