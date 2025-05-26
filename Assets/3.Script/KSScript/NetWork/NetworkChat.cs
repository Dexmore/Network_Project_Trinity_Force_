using Mirror;
using UnityEngine;
using System.Collections;

public class NetworkChat : NetworkBehaviour
{
    public Transform userListParent => userListParentSerialized;
    public GameObject userSlotPrefab => userSlotPrefabSerialized;

    [SerializeField] private Transform userListParentSerialized;
    [SerializeField] private GameObject userSlotPrefabSerialized;

    public delegate void ChatMessageHandler(string message, string senderName);
    public static event ChatMessageHandler OnChatMessage;

    [SyncVar(hook = nameof(OnReadyChanged))] public bool isReady;
    [SyncVar(hook = nameof(OnNicknameChanged))] public string playerName;

    private static Coroutine startGameRoutine;

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

    public override void OnStopClient()
    {
        if (LobbyUserManager.Instance != null && !string.IsNullOrEmpty(playerName))
            LobbyUserManager.Instance.RemoveUser(playerName);

        if (!string.IsNullOrEmpty(playerName))
            LobbyPopupUIManager.Instance?.ShowPopup($"{playerName}님이 퇴장했습니다.");
    }

    private IEnumerator WaitAndRegister()
    {
        yield return new WaitUntil(() => LobbyUserManager.Instance != null);
        yield return new WaitUntil(() => !string.IsNullOrEmpty(playerName));
        yield return new WaitUntil(() =>
            LobbyUserManager.Instance.userSlotPrefab != null &&
            LobbyUserManager.Instance.userListParent != null
        );

        LobbyUserManager.Instance.AddUser(playerName, isReady);
        LobbyPopupUIManager.Instance?.ShowPopup($"{playerName}님이 입장했습니다.");
    }

    void OnReadyChanged(bool _, bool newVal)
    {
        if (LobbyUserManager.Instance != null)
            LobbyUserManager.Instance.UpdateNicknameReady(playerName, newVal);
    }

    void OnNicknameChanged(string _, string __) { }

    [Command]
    public void CmdToggleReady()
    {
        isReady = !isReady;

        if (isServer)
        {
            CheckAllPlayersReady();
        }
    }

    private void CheckAllPlayersReady()
    {
        var players = FindObjectsOfType<NetworkChat>();
        int total = players.Length;
        int ready = 0;

        foreach (var p in players)
        {
            if (p.isReady) ready++;
        }

        if (total >= 2 && total <= 4 && ready == total)
        {
            if (startGameRoutine == null)
            {
                startGameRoutine = StartCoroutine(StartGameAfterDelay());
            }
        }
        else
        {
            if (startGameRoutine != null)
            {
                StopCoroutine(startGameRoutine);
                startGameRoutine = null;
                Debug.Log("[NetworkChat] 준비 취소됨 - 씬 전환 취소됨");
            }
        }
    }

    private IEnumerator StartGameAfterDelay()
    {
        Debug.Log("[NetworkChat] 모든 유저가 준비 완료됨 - 3초 후 게임 시작");
        yield return new WaitForSeconds(3f);

        if (isServer)
        {
            NetworkManager.singleton.ServerChangeScene("GameScene");
        }
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
