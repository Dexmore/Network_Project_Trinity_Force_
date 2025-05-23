using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.UI;
public class NetworkSpawner : MonoBehaviour // ← MonoBehaviour만 상속!
{
    public GameObject chatFieldPrefab;
    public Transform chatParent;

    // 서버에서만 호출할 것!
    public void SpawnChatField()
    {
        if (NetworkServer.active) // 서버에서만!
        {
            var chat = Instantiate(chatFieldPrefab, chatParent, false); // false: 부모 위치계에 맞게 생성

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
