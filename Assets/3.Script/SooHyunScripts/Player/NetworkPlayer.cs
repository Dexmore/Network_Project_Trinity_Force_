using Mirror;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{
    [SyncVar] public bool HasSubmitted = false;
    [SyncVar] public string lastText = "";
    [SyncVar] public int playerIndex = -1;
    [SyncVar] public string playerName;

    [Command]
    public void CmdSetSubmitted(bool value) { HasSubmitted = value; }

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
