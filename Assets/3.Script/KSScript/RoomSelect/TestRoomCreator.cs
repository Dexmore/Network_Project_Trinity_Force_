// Assets/Scripts/TestRoomCreator.cs
using UnityEngine;

public class TestRoomCreator : MonoBehaviour
{
    [Header("생성할 방 프리팹")]
    [Tooltip("Instantiate 할 방 프리팹")]
    [SerializeField] private GameObject roomPrefab;

    [Header("부모 Transform")]
    [Tooltip("프리팹을 이 Transform 아래에 생성합니다")]
    [SerializeField] private Transform parentTransform;

    private int roomCounter = 1;

    /// <summary>
    /// 버튼 OnClick()에 바인딩.
    /// 누를 때마다 roomPrefab을 parentTransform 아래에 Instantiate 합니다.
    /// </summary>
    public void SpawnTestRoom()
    {
        if (roomPrefab == null)
        {
            Debug.LogError("[TestRoomCreator] roomPrefab이 할당되지 않았습니다!");
            return;
        }
        if (parentTransform == null)
        {
            Debug.LogError("[TestRoomCreator] parentTransform이 할당되지 않았습니다!");
            return;
        }

        // 방 번호를 텍스트나 이름으로 남기고 싶다면...
        string newRoomName = $"Room{roomCounter++}";
        GameObject go = Instantiate(roomPrefab, parentTransform);
        go.name = newRoomName;                  // Hierarchy 상에 구분하기 쉽게
        // (선택) go.GetComponent<EnterRoomHandler>()?.Initialize(...);

        Debug.Log($"[TestRoomCreator] '{newRoomName}' 방을 생성했습니다.");
    }
}
