using Mirror;
using UnityEngine;
using System.Collections;

public class NetworkPlayer : NetworkBehaviour
{
    [Header("Game Data")]
    [SyncVar] public bool HasSubmitted = false;
    [SyncVar] public string lastText = "";
    [SyncVar] public int playerIndex = -1;

    [Header("Lobby Info")]
    [SyncVar(hook = nameof(OnReadyChanged))] public bool isReady;
    [SyncVar(hook = nameof(OnNicknameChanged))] public string playerName;

    public delegate void ChatMessageHandler(string message, string senderName);
    public static event ChatMessageHandler OnChatMessage;

    private static Coroutine startGameRoutine;

    public override void OnStartLocalPlayer()
    {
        string nick = SQLManager.instance?.info?.User_Nickname ?? "Unknown";
        CmdSetNickname(nick);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        StartCoroutine(WaitAndRegisterToLobby());
    }

    public override void OnStopClient()
    {
        if (LobbyUserManager.Instance != null && !string.IsNullOrEmpty(playerName))
            LobbyUserManager.Instance.RemoveUser(playerName);

        if (!string.IsNullOrEmpty(playerName))
            LobbyPopupUIManager.Instance?.ShowPopup($"{playerName}님이 퇴장했습니다.");
    }

    private IEnumerator WaitAndRegisterToLobby()
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
        LobbyUserManager.Instance?.UpdateNicknameReady(playerName, newVal);
    }

    void OnNicknameChanged(string _, string __) { }

    [Command]
    public void CmdSetNickname(string nick)
    {
        playerName = nick;
    }

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
        var players = FindObjectsOfType<NetworkPlayer>();
        int total = players.Length;
        int ready = 0;

        foreach (var p in players)
        {
            if (p.isReady) ready++;
        }

        if (total >= 2 && total <= 4 && ready == total)
        {
            if (startGameRoutine == null)
                startGameRoutine = StartCoroutine(StartGameAfterDelay());
        }
        else
        {
            if (startGameRoutine != null)
            {
                StopCoroutine(startGameRoutine);
                startGameRoutine = null;
                Debug.Log("[NetworkPlayer] 준비 취소됨 - 씬 전환 취소됨");
            }
        }
    }

    private IEnumerator StartGameAfterDelay()
    {
        Debug.Log("[NetworkPlayer] 모든 유저가 준비 완료됨 - 3초 후 게임 시작");
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

    // ---------- 게임 제출 관련 ----------
    [Command]
    public void CmdSetSubmitted(bool value)
    {
        HasSubmitted = value;
    }

    [Command]
    public void CmdSetText(string value)
    {
        lastText = value;
        var serverChecker = FindObjectOfType<ServerChecker1>();
        if (serverChecker != null)
            serverChecker.AddSentence(this, value);
    }

    [TargetRpc]
    public void TargetShowSentence(NetworkConnection target, string message)
    {
        var gm = FindObjectOfType<GameManager>();
        if (gm != null)
            gm.ShowReceivedSentence(message, playerIndex);
    }

    [Command]
    public void CmdSubmitDrawing(byte[] pngData)
    {
        var serverChecker = FindObjectOfType<ServerChecker1>();
        if (serverChecker != null)
            serverChecker.AddDrawing(this, pngData);
    }

    [TargetRpc]
    public void TargetReceiveDrawing(NetworkConnection target, byte[] pngData)
    {
        var gm = FindObjectOfType<GameManager>();
        if (gm != null)
            gm.ShowReceivedDrawing(pngData, playerIndex);
    }

    [Command]
    public void CmdSetGuess(string guessText)
    {
        var serverChecker = FindObjectOfType<ServerChecker1>();
        if (serverChecker != null)
            serverChecker.AddGuess(this, guessText);
    }

    [TargetRpc]
    public void TargetShowGuess(NetworkConnection target, string guess)
    {
        var gm = FindObjectOfType<GameManager>();
        if (gm != null)
            gm.ShowReceivedGuess(guess, playerIndex);
    }
}
