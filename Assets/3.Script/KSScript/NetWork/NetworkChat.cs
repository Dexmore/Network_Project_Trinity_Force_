using Mirror;
using UnityEngine;
using System.Collections;

public class NetworkChat : NetworkBehaviour
{
    // 외부에서 연결할 슬롯 프리팹 및 부모 트랜스폼
    public Transform userListParent => userListParentSerialized;
    public GameObject userSlotPrefab => userSlotPrefabSerialized;

    [SerializeField] private Transform userListParentSerialized;
    [SerializeField] private GameObject userSlotPrefabSerialized;

    public delegate void ChatMessageHandler(string message, string senderName);
    public static event ChatMessageHandler OnChatMessage;

    [SyncVar(hook = nameof(OnReadyChanged))] public bool isReady;
    [SyncVar(hook = nameof(OnNicknameChanged))] public string playerName;

    // 🔹 로컬 플레이어 최초 시작 시 → 닉네임 전달
    public override void OnStartLocalPlayer()
    {
        string nick = SQLManager.instance?.info?.User_Nickname ?? "Unknown";
        CmdSetNickname(nick);
    }

    // 🔹 서버로 닉네임 전달
    [Command]
    public void CmdSetNickname(string nick)
    {
        playerName = nick;
    }

    // 🔹 클라이언트 시작 시 슬롯 등록 + 팝업은 코루틴에서!
    public override void OnStartClient()
    {
        base.OnStartClient();
        StartCoroutine(WaitAndRegister());
    }

    // 🔹 클라이언트 종료 시 슬롯 제거 + 퇴장 팝업
    public override void OnStopClient()
    {
        if (LobbyUserManager.Instance != null && !string.IsNullOrEmpty(playerName))
        {
            LobbyUserManager.Instance.RemoveUser(playerName);
        }

        if (!string.IsNullOrEmpty(playerName))
        {
            LobbyPopupUIManager.Instance?.ShowPopup($"{playerName}님이 퇴장했습니다.");
        }
    }

    // 🔹 코루틴: 닉네임 sync까지 기다렸다가 슬롯 등록 + 입장 팝업
    private IEnumerator WaitAndRegister()
    {
        // 1) LobbyUserManager 존재할 때까지 대기
        yield return new WaitUntil(() => LobbyUserManager.Instance != null);

        // 2) playerName이 동기화될 때까지 대기
        yield return new WaitUntil(() => !string.IsNullOrEmpty(playerName));

        // 3) 슬롯 연결 상태 대기
        yield return new WaitUntil(() =>
            LobbyUserManager.Instance.userSlotPrefab != null &&
            LobbyUserManager.Instance.userListParent != null
        );

        // 4) 슬롯 생성
        LobbyUserManager.Instance.AddUser(playerName, isReady);

        // 5) 입장 팝업 표시
        LobbyPopupUIManager.Instance?.ShowPopup($"{playerName}님이 입장했습니다.");
    }

    // 🔹 준비 상태 변경 SyncVar Hook
    void OnReadyChanged(bool _, bool newVal)
    {
        if (LobbyUserManager.Instance != null)
        {
            LobbyUserManager.Instance.UpdateNicknameReady(playerName, newVal);
        }
    }

    // 🔹 닉네임 변경 Hook (사용하지 않음)
    void OnNicknameChanged(string _, string __) { }

    // 🔹 준비 토글 Command
    [Command]
    public void CmdToggleReady()
    {
        isReady = !isReady;
    }

    // 🔹 채팅 Command
    [Command]
    public void CmdSendMessage(string message, string senderName)
    {
        RpcReceiveMessage(message, senderName);
    }

    // 🔹 채팅 ClientRpc
    [ClientRpc]
    void RpcReceiveMessage(string message, string senderName)
    {
        OnChatMessage?.Invoke(message, senderName);
    }
}
