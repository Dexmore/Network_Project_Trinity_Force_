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

        // 플레이어에 고유 인덱스 부여
        Player player = conn.identity.GetComponent<Player>();
        player.playerIndex = playerCounter++;

        // 최초 1회만 TimeManager 생성 및 스폰
        if (timeManagerInstance == null)
        {
            timeManagerInstance = Instantiate(timeManagerPrefab);
            DontDestroyOnLoad(timeManagerInstance);
            NetworkServer.Spawn(timeManagerInstance);
        }
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        base.OnServerDisconnect(conn);
        playerCounter--;
    }
}
