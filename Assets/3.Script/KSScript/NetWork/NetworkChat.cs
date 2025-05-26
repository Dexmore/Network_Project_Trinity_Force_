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

    // ✅ 닉네임 설정 (로컬 플레이어일 때 서버에 전달)
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

    // ✅ 로컬 플레이어가 클라이언트에 등장했을 때 UI 갱신 (안정적인 타이밍)
    public override void OnStartClient()
    {
        base.OnStartClient();

        if (LobbyUserManager.Instance != null)
        {
            LobbyUserManager.Instance.AddUser(playerName, isReady);
        }
        else
        {
            Debug.LogWarning("[NetworkChat] LobbyUserManager.Instance is null in OnStartClient");
        }
    }

    // ✅ 준비 상태 변경 시 UI 갱신
    void OnReadyChanged(bool _, bool newVal)
    {
        if (LobbyUserManager.Instance != null)
        {
            LobbyUserManager.Instance.UpdateNicknameReady(playerName, newVal);
        }
    }

    // ✅ 준비 상태 토글
    [Command]
    public void CmdToggleReady()
    {
        isReady = !isReady;
    }

    // ❌ 제거된 부분: 너무 빠른 호출로 null 예외 발생
    void OnNicknameChanged(string _, string newName)
    {
        // AddUser() 호출 제거됨
        // 대신 OnStartClient에서 호출
    }
}
