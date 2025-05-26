using Mirror;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{
    [SyncVar] public bool HasSubmitted = false;
    [SyncVar] public string lastText = "";
    [SyncVar] public int playerIndex = -1;
    [SyncVar] public string playerName; // SQLManager에서 가져올 이름

    [SyncVar(hook = nameof(OnReadyChanged))]
    public bool isReady = false;

    #region 기존 커맨드/타겟 RPC

    [Command]
    public void CmdSetSubmitted(bool value)
    {
        HasSubmitted = value;
    }

    [Command]
    public void CmdSetText(string value)
    {
        lastText = value;
        var serverChecker = FindObjectOfType<ServerChecker>();
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
        var serverChecker = FindObjectOfType<ServerChecker>();
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
        var serverChecker = FindObjectOfType<ServerChecker>();
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

    #endregion

    #region 로비: 닉네임 / 준비 상태

    public override void OnStartLocalPlayer()
    {
        string myNick = SQLManager.instance?.info?.User_Nickname ?? "Unknown";
        CmdSetNickname(myNick);
    }

    [Command]
    void CmdSetNickname(string nick)
    {
        playerName = nick;
    }

    void OnReadyChanged(bool oldVal, bool newVal)
    {
        // 상태 업데이트 (UI 반영)
        if (LobbyUserManager.Instance != null)
            LobbyUserManager.Instance.UpdateNicknameReady(playerName, newVal);
    }

    public override void OnStartClient()
    {
        // 클라이언트 시작 시 AddUser 실행
        if (LobbyUserManager.Instance != null)
        {
            LobbyUserManager.Instance.AddUser(playerName, isReady);
        }
    }

    [Command]
    public void CmdToggleReady()
    {
        isReady = !isReady;
    }

    #endregion
}
