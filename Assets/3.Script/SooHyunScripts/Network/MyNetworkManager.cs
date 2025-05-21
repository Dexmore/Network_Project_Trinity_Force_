using Mirror;
using UnityEngine;

public class MyNetworkManager : NetworkManager
{
    public GameObject timeManagerPrefab;
    private GameObject timeManagerInstance;

    private int playerCounter = 0;

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);

        // 플레이어 인덱스 할당
        Player player = conn.identity.GetComponent<Player>();
        player.playerIndex = playerCounter++;

        // 타임매니저 한 번만 생성
        if (timeManagerInstance == null)
        {
            timeManagerInstance = Instantiate(timeManagerPrefab);
            NetworkServer.Spawn(timeManagerInstance);
        }
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        base.OnServerDisconnect(conn);
        playerCounter--;
    }
}
