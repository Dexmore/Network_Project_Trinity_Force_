using Mirror;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [SyncVar] public int playerIndex;

    private void Update()
    {
        if (!isLocalPlayer) return;

        TimeManager tm = FindObjectOfType<TimeManager>();
        if (tm == null) return;

        if (playerIndex == tm.GetCurrentPlayer())
            tm.LocalPlayerUpdateUI();
        else
            tm.HideAllPanels();
    }
}
