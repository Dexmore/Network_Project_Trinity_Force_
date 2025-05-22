using Mirror;
using UnityEngine;

public class MyNetworkManager : NetworkManager
{
    public GameObject timeManagerPrefab; // Drag your TimeManager prefab here
    private GameObject timeManagerInstance;

    private int playerCounter = 0;

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        // 기본 플레이어 추가
        base.OnServerAddPlayer(conn);

        // 플레이어 인덱스 지정
        Player player = conn.identity.GetComponent<Player>();
        player.playerIndex = playerCounter++;
        Debug.Log($"[Server] Player {player.playerIndex} joined.");

        // TimeManager는 한 번만 서버에 생성
        if (timeManagerInstance == null)
        {
            timeManagerInstance = Instantiate(timeManagerPrefab);
            NetworkServer.Spawn(timeManagerInstance);
            Debug.Log("[Server] TimeManager spawned.");
        }
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        base.OnServerDisconnect(conn);
        playerCounter--;
    }
}
