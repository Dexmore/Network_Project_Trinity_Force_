// Assets/3.Script/KSScript/LobbyUIManager.cs
using UnityEngine;
using UnityEngine.UI;

public class LobbyUIManager : MonoBehaviour
{
    [Header("Inspector에 모두 연결!")]
    public GameObject userSlotPrefab;   // Prefabs/UserSlot
    public Transform contentParent;     // Scroll View→Viewport→Content
    public ScrollRect scrollRect;       // Scroll View 루트

    private int testUserCount = 0;

    /// <summary>
    /// 버튼 OnClick에 연결 (Inspector에서)
    /// </summary>
    public void TestAddUserSingle()
    {
        testUserCount++;
        string nick    = $"TestUser_{testUserCount}";
        bool   isReady = Random.value > 0.5f;
        AddUser(nick, isReady);
    }

    /// <summary>
    /// 실제 슬롯 생성 & 초기화
    /// </summary>
    public void AddUser(string nickname, bool isReady)
    {
        // 1) 프리팹 인스턴스화
        GameObject slotGO = Instantiate(userSlotPrefab, contentParent);
        if (slotGO == null)
        {
            Debug.LogError("⚠️ 슬롯 인스턴스화 실패!");
            return;
        }

        // 2) UserSlotUI 컴포넌트로 초기화
        var ui = slotGO.GetComponent<UserSlotUI>();
        if (ui == null)
        {
            Debug.LogError("⚠️ UserSlotUI 컴포넌트를 찾을 수 없음!");
            return;
        }
        ui.Initialize(nickname, isReady);

        // 3) 스크롤을 맨 아래로 (최신 항목이 보이도록)
        Canvas.ForceUpdateCanvases();
        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 0f;
        else
            Debug.LogWarning("⚠️ ScrollRect가 할당되지 않음!");
    }
}
