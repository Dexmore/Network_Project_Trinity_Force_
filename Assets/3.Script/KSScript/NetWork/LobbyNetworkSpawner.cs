using Mirror;
using UnityEngine;

public class LobbyNetworkSpawner : NetworkBehaviour
{
    public GameObject chatFieldPrefab;
    public Transform chatParent; // Inspector에 "ChatPanel" 등 drag&drop

    public GameObject userSlotPrefab;
    public Transform userSlotParent; // Inspector에 "UserSlotPanel" 등 drag&drop

    public override void OnStartServer()
    {
        // ChatField를 chatParent(예: Canvas/ChatPanel) 아래에 생성
        GameObject chat = Instantiate(chatFieldPrefab, chatParent);
        NetworkServer.Spawn(chat);

        // UserSlot도 필요하면 userSlotParent 아래에 생성
        // GameObject slot = Instantiate(userSlotPrefab, userSlotParent);
        // NetworkServer.Spawn(slot);
    }
}
