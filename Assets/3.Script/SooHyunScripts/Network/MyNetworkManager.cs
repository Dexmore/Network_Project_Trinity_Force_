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

        // �÷��̾� �ε��� �Ҵ�
        Player player = conn.identity.GetComponent<Player>();
        player.playerIndex = playerCounter++;

        // Ÿ�ӸŴ��� �� ���� ����
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
