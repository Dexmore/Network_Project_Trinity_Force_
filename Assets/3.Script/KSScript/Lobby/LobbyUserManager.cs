using System.Collections.Generic;
using UnityEngine;

public class LobbyUserManager : MonoBehaviour
{
    public static LobbyUserManager Instance;

    [SerializeField] public Transform userListParent;
    [SerializeField] public GameObject userSlotPrefab;

    private readonly Dictionary<string, UserSlotController> slots = new();

    void Awake()
    {
        Instance = this;
    }

    public void AddUser(string nickname, bool isReady)
    {
        // 🔍 디버깅: null 체크
        if (userSlotPrefab == null)
        {
            Debug.LogError("[LobbyUserManager] userSlotPrefab이 연결되지 않았습니다!");
            return;
        }

        if (userListParent == null)
        {
            Debug.LogError("[LobbyUserManager] userListParent가 연결되지 않았습니다!");
            return;
        }

        if (string.IsNullOrEmpty(nickname))
        {
            Debug.LogWarning("[LobbyUserManager] nickname이 비어있습니다.");
            return;
        }

        if (slots.ContainsKey(nickname)) return;

        var go = Instantiate(userSlotPrefab, userListParent);
        var slot = go.GetComponent<UserSlotController>();
        slot.SetUserInfo(nickname, isReady);
        slots[nickname] = slot;

        Debug.Log($"[LobbyUserManager] 사용자 추가됨: {nickname}, 상태: {(isReady ? "준비 완료" : "준비 중")}");
    }

    public void UpdateNicknameReady(string nickname, bool isReady)
    {
        if (slots.TryGetValue(nickname, out var slot))
        {
            slot.SetReady(isReady);
        }
    }

}
