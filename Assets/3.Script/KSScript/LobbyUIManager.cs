using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyUIManager : MonoBehaviour
{
    [Header("Inspector에 모두 연결!")]
    public GameObject userSlotPrefab;   // Prefabs/UserSlot
    public Transform contentParent;     // Scroll View→Viewport→Content
    public ScrollRect scrollRect;       // Scroll View 루트

    private int testUserCount = 0;

    // 버튼 OnClick에 연결
    public void TestAddUserSingle()
    {
        testUserCount++;
        string nick    = $"TestUser_{testUserCount}";
        bool   isReady = Random.value > 0.5f;
        AddUser(nick, isReady);
    }

    public void AddUser(string nickname, bool isReady)
    {
        // 1) 프리팹 인스턴스화
        GameObject slotGO = Instantiate(userSlotPrefab, contentParent);
        if (slotGO == null) return;

        // --- 강제 Stretch 적용 (선택) ---
        var rt = slotGO.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, rt.anchorMin.y);
        rt.anchorMax = new Vector2(1, rt.anchorMax.y);
        rt.offsetMin = new Vector2(0, rt.offsetMin.y);
        rt.offsetMax = new Vector2(0, rt.offsetMax.y);

        // 2) 텍스트 세팅
        var ui = slotGO.GetComponent<UserSlotUI>();
        ui.Initialize(nickname, isReady);

        // 3) 스크롤을 맨 아래로
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }
}
