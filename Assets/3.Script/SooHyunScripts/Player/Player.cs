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
            tm.LocalPlayerUpdateUI(); // �� ���ʴϱ� UI ���̱�
        else
            tm.HideAllPanels();       // �� ���� �ƴϴϱ� UI �����
    }
}
