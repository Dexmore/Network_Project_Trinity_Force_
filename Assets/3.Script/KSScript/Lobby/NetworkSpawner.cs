using UnityEngine;
using Mirror;

public class NetworkSpawner : MonoBehaviour // ← MonoBehaviour만 상속!
{
    public GameObject chatFieldPrefab;
    public Transform chatParent;

    // 서버에서만 호출할 것!
    public void SpawnChatField()
    {
        if (NetworkServer.active) // 서버에서만!
        {
            var chat = Instantiate(chatFieldPrefab, chatParent);
            NetworkServer.Spawn(chat);
        }
    }

    // 필요시 Start나 커스텀 메서드에서 SpawnChatField() 호출
    private void Start()
    {
        if (NetworkServer.active)
        {
            SpawnChatField();
        }
    }
}
