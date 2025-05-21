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

        // �÷��̾ ���� �ε��� �ο�
        Player player = conn.identity.GetComponent<Player>();
        player.playerIndex = playerCounter++;

        // ���� 1ȸ�� TimeManager ���� �� ����
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
