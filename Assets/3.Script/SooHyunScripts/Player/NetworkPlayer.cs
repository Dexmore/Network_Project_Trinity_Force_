using Mirror;
using UnityEngine;
using System.Collections;

public class NetworkPlayer : NetworkBehaviour
{
    [SyncVar] public bool HasSubmitted = false;
    [SyncVar] public string lastText = "";
    [SyncVar] public int playerIndex = -1;
    [SyncVar(hook = nameof(OnNicknameChanged))] public string playerName;

    public override void OnStartLocalPlayer()
    {
        string nick = SQLManager.instance?.info?.User_Nickname ?? "Unknown";
        CmdSetNickname(nick);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (isLocalPlayer)
            CmdNotifySceneLoaded();
    }

    [Command]
    public void CmdSetNickname(string nick)
    {
        playerName = nick;
    }

    void OnNicknameChanged(string _, string __) { }

    [Command]
    public void CmdNotifySceneLoaded()
    {
        var serverChecker = FindObjectOfType<ServerChecker1>();
        serverChecker?.OnClientReadyInGame(connectionToClient);
    }

    [Command] public void CmdSetSubmitted(bool value) => HasSubmitted = value;

    [Command]
    public void CmdSetText(string value)
    {
        lastText = value;
        var serverChecker = FindObjectOfType<ServerChecker1>();
        serverChecker?.AddSentence(this, value);
    }

    [TargetRpc]
    public void TargetShowSentence(NetworkConnection target, string message)
    {
        var gm = FindObjectOfType<GameManager>();
        gm?.ShowReceivedSentence(message, playerIndex);
    }

    [Command]
    public void CmdSubmitDrawing(byte[] pngData)
    {
        var serverChecker = FindObjectOfType<ServerChecker1>();
        serverChecker?.AddDrawing(this, pngData);
    }

    [TargetRpc]
    public void TargetReceiveDrawing(NetworkConnection target, byte[] pngData)
    {
        var gm = FindObjectOfType<GameManager>();
        gm?.ShowReceivedDrawing(pngData, playerIndex);
    }

    [Command]
    public void CmdSetGuess(string guessText)
    {
        var serverChecker = FindObjectOfType<ServerChecker1>();
        serverChecker?.AddGuess(this, guessText);
    }

    [TargetRpc]
    public void TargetShowGuess(NetworkConnection target, string guess)
    {
        var gm = FindObjectOfType<GameManager>();
        gm?.ShowReceivedGuess(guess, playerIndex);
    }
}