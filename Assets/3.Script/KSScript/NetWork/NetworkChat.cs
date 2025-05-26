using Mirror;
using UnityEngine;
using System.Collections; // ← 코루틴 쓰려면 필요

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
        // 1) LobbyUserManager 살아날 때까지 기다림
        yield return new WaitUntil(() => LobbyUserManager.Instance != null);

        // 2) playerName까지 세팅될 때까지 기다림
        yield return new WaitUntil(() => !string.IsNullOrEmpty(playerName));

        // 3) LobbyUserManager 안에 슬롯 프리팹/부모가 연결되어 있는지 확인
        yield return new WaitUntil(() =>
            LobbyUserManager.Instance != null &&
            LobbyUserManager.Instance.userSlotPrefab != null &&
            LobbyUserManager.Instance.userListParent != null
        );

        // 4) 안전하게 생성
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
