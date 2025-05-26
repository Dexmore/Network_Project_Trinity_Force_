using System.Collections.Generic;
using UnityEngine;

public class LobbyUserManager : MonoBehaviour
{
    public static LobbyUserManager Instance;

    [SerializeField] private Transform userListParent;
    [SerializeField] private GameObject userSlotPrefab;

    private readonly Dictionary<string, UserSlotController> slots = new();

    void Awake()
    {
        LobbyUserManager.Instance = this;
    }

    public void AddUser(string nickname, bool isReady)
    {
        if (slots.ContainsKey(nickname)) return;

        var go = Instantiate(userSlotPrefab, userListParent);
        var slot = go.GetComponent<UserSlotController>();
        slot.SetUserInfo(nickname, isReady);
        slots[nickname] = slot;
    }

    public void UpdateNicknameReady(string nickname, bool isReady)
    {
        if (slots.TryGetValue(nickname, out var slot))
        {
            slot.SetReady(isReady);
        }
    }
}
