// PlayerController.cs
using Mirror;
using UnityEngine;
using System.Collections;

public class PlayerController : NetworkBehaviour
{
    [SyncVar] public int playerIndex;

    public override void OnStartServer()
    {
        StartCoroutine(WaitForGameManager());
    }

    private IEnumerator WaitForGameManager()
    {
        while (GarticPhoneGameManager.Instance == null)
            yield return null;

        playerIndex = GarticPhoneGameManager.Instance.RegisterPlayer(connectionToClient);
    }

    public override void OnStartLocalPlayer()
    {
        Debug.Log($"[Local Player] Index: {playerIndex}");
    }
}
