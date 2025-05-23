using Mirror;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{
    [SyncVar] public bool HasSubmitted = false;
    [SyncVar] public string lastText = "";
    [SyncVar] public int playerIndex = -1; // Player index (network order)

    [Command]
    public void CmdSetSubmitted(bool value)
    {
        HasSubmitted = value;
        var serverChecker = FindObjectOfType<ServerChecker>();
        if (serverChecker != null)
            serverChecker.CheckAllSubmitted();
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
        Debug.Log($"[Client] My playerIndex={playerIndex}, Received message={message}");
        var gm = FindObjectOfType<GameManager>();
        if (gm != null)
            gm.ShowReceivedSentence(message, playerIndex);
    }
}
