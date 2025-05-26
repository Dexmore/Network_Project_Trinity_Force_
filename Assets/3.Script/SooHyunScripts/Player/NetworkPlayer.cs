using Mirror;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{
    [SyncVar] public string playerName;
    [SyncVar] public bool HasSubmitted = false;
    [SyncVar] public int playerIndex = -1;

    [Command]
    public void CmdSetSubmitted(bool value) { HasSubmitted = value; }

    [Command]
    public void CmdSetText(string value)
    {
        var serverChecker = FindObjectOfType<ServerChecker1>();
        if (serverChecker != null)
            serverChecker.AddSentence(this, value);
    }

    [Command]
    public void CmdSubmitDrawing(byte[] pngData)
    {
        var serverChecker = FindObjectOfType<ServerChecker1>();
        if (serverChecker != null)
            serverChecker.AddDrawing(this, pngData);
    }

    [Command]
    public void CmdSetGuess(string guessText)
    {
        var serverChecker = FindObjectOfType<ServerChecker1>();
        if (serverChecker != null)
            serverChecker.AddGuess(this, guessText);
    }

    [TargetRpc]
    public void TargetGameStart(NetworkConnection target)
    {
        var gm = FindObjectOfType<GameManager>();
        if (gm != null)
            gm.BeginGame();
    }

    [TargetRpc]
    public void TargetProceedToNextPhase(NetworkConnection target)
    {
        var gm = FindObjectOfType<GameManager>();
        if (gm != null)
            gm.ProceedToNextPhase();
    }

    [TargetRpc]
    public void TargetShowResult(NetworkConnection target)
    {
        var gm = FindObjectOfType<GameManager>();
        if (gm != null)
            gm.GoToResultScene();
    }

    // 각 페이즈 데이터 전달
    [TargetRpc]
    public void TargetShowSentence(NetworkConnection target, string sentence)
    {
        var gm = FindObjectOfType<GameManager>();
        if (gm != null)
            gm.ShowReceivedSentence(sentence, playerIndex);
    }

    [TargetRpc]
    public void TargetReceiveDrawing(NetworkConnection target, byte[] pngData)
    {
        var gm = FindObjectOfType<GameManager>();
        if (gm != null)
            gm.ShowReceivedDrawing(pngData, playerIndex);
    }

    [TargetRpc]
    public void TargetShowGuess(NetworkConnection target, string guess)
    {
        var gm = FindObjectOfType<GameManager>();
        if (gm != null)
            gm.ShowReceivedGuess(guess, playerIndex);
    }
}
