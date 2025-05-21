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

        bool isMyTurn = playerIndex == tm.GetCurrentPlayer();

        if (isMyTurn)
            tm.LocalPlayerUpdateUI(); // 내 차례니까 UI 보이기
        else
            tm.HideAllPanels();       // 내 차례 아니니까 UI 숨기기
    }
}
