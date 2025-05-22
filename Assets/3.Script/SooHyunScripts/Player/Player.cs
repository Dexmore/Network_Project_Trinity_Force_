using Mirror;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [SyncVar] public int playerIndex;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (isServer)
        {
            TimeManager.Instance?.RegisterPlayer(playerIndex);
        }
    }

    [Command]
    public void CmdSubmitTextToServer(string text)
    {
        TimeManager.Instance?.CmdSubmitText(text, playerIndex);
    }

    [Command]
    public void CmdSubmitDrawingToServer(byte[] data)
    {
        TimeManager.Instance?.CmdSubmitDrawing(data, playerIndex);
    }
}
