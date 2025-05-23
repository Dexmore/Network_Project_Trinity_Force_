using Mirror;
using UnityEngine;

public class NetworkSpawner : NetworkBehaviour
{
    public GameObject chatFieldPrefab;
    public Transform chatParent;

    public override void OnStartServer()
    {
        // 씬이 서버에서 완전히 로딩된 이후에 호출됨!
        Debug.Log("[NetworkSpawner] OnStartServer 호출!");
        var chat = Instantiate(chatFieldPrefab, chatParent, false);
        NetworkServer.Spawn(chat);
        Debug.Log("[NetworkSpawner] ChatField 스폰 완료!");
    }
}
