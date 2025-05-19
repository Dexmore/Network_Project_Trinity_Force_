using UnityEngine;
using TMPro;
using System.Collections;

public class LobbyUIManager : MonoBehaviour
{
    [Header("User Slot Settings")]
    [Tooltip("유저 슬롯 프리팹")]
    public GameObject userSlotPrefab;

    [Tooltip("Grid Layout Group이 붙은 Content 오브젝트")]
    public Transform contentParent;

    [Tooltip("최대 허용 유저 수")]
    public int maxUsers = 4;

    [Header("Notification UI")]
    [Tooltip("NotificationPanel의 CanvasGroup")]
    public CanvasGroup notificationGroup;

    [Tooltip("NotificationText 컴포넌트")]
    public TMP_Text notificationText;

    [Tooltip("팝업 표시 시간(초)")]
    public float notificationDuration = 2f;

    private int testUserCount = 0;

    /// <summary>
    /// Inspector에서 버튼 OnClick에 연결
    /// </summary>
    public void TestAddUserSingle()
    {
        // 최대 유저 수 체크
        if (contentParent.childCount >= maxUsers)
        {
            ShowNotification("로비가 가득 찼습니다.");
            return;
        }

        testUserCount++;
        string nick    = $"TestUser_{testUserCount}";
        bool   isReady = Random.value > 0.5f;

        AddUser(nick, isReady);
        ShowNotification($"{nick} 님이 로비에 입장했습니다");
    }

    /// <summary>
    /// 슬롯 생성 & 초기화
    /// </summary>
    public void AddUser(string nickname, bool isReady)
    {
        Debug.Log("▶ AddUser 호출됨");
        Debug.Log($"   prefab = {userSlotPrefab}, parent = {contentParent}");

        GameObject slotGO = Instantiate(userSlotPrefab, contentParent);
        Debug.Log($"   slotGO = {slotGO}, 자식 개수 = {contentParent.childCount}");

        // UserSlotUI 컴포넌트로 데이터 바인딩
        var ui = slotGO.GetComponent<UserSlotUI>();
        if (ui != null)
        {
            ui.Initialize(nickname, isReady);
        }
        else
        {
            Debug.LogError("⚠️ UserSlotUI 컴포넌트를 찾을 수 없습니다!");
        }
    }

    /// <summary>
    /// 화면 중앙에 알림 팝업 띄우기
    /// </summary>
    public void ShowNotification(string message)
    {
        StopAllCoroutines();
        notificationText.text = message;
        StartCoroutine(NotificationCoroutine());
    }

    private IEnumerator NotificationCoroutine()
    {
        // Fade In
        float t = 0f;
        while (t < 0.2f)
        {
            t += Time.deltaTime;
            notificationGroup.alpha = Mathf.Lerp(0f, 1f, t / 0.2f);
            yield return null;
        }
        notificationGroup.alpha = 1f;

        // 대기
        yield return new WaitForSeconds(notificationDuration);

        // Fade Out
        t = 0f;
        while (t < 0.2f)
        {
            t += Time.deltaTime;
            notificationGroup.alpha = Mathf.Lerp(1f, 0f, t / 0.2f);
            yield return null;
        }
        notificationGroup.alpha = 0f;
    }
}
