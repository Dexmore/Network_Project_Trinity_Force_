using UnityEngine;
using Mirror;

public class MyNetworkManager : NetworkManager
{
    public GameObject timeManagerPrefab; // Drag & Drop
    private GameObject timeManagerInstance;

    private int playerCounter = 0;

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);

        // 플레이어 인덱스 부여
        Player player = conn.identity.GetComponent<Player>();
        player.playerIndex = playerCounter++;
        Debug.Log($"[Server] Player {player.playerIndex} joined");

        // 서버에서만 TimeManager 생성
        if (timeManagerInstance == null && NetworkServer.active)
        {
            Debug.Log("[Server] Spawning TimeManager");
            timeManagerInstance = Instantiate(timeManagerPrefab);
            NetworkServer.Spawn(timeManagerInstance);
        }
    }
}
