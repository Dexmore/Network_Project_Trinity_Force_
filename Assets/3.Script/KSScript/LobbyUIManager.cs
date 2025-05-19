using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class LobbyUIManager : MonoBehaviour
{
    [Header("User Slot Settings")]
    [Tooltip("유저 슬롯 프리팹")]
    public GameObject userSlotPrefab;
    [Tooltip("슬롯을 배치할 Content (Grid or Vertical)")]
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

    [Header("Scene Settings")]
    [Tooltip("로비 나간 뒤 돌아갈 씬 이름")]
    public string menuSceneName = "MainMenu";

    [Header("Ready Settings")]
    [Tooltip("내가 누를 준비 버튼")]
    public Button readyButton;
    [Tooltip("준비 버튼 안의 텍스트 컴포넌트")]
    public TMP_Text readyButtonText;
    [Tooltip("모두 준비되면 넘어갈 씬 이름")]
    public string gameSceneName = "KSScene";

    // 내부 상태
    private int testUserCount = 0;
    private bool localIsReady = false;
    private Coroutine countdownCoroutine;
    // 기존 countdownCoroutine 외에
    private Coroutine notificationCoroutine;


    private void Awake()
    {
        // 준비 버튼 콜백 연결 (Inspector에서도 연결 가능)
        if (readyButton != null)
            readyButton.onClick.AddListener(OnReadyButtonClicked);
    }

    /// <summary>
    /// IN USER 테스트 버튼
    /// </summary>
    public void TestAddUserSingle()
    {
        if (contentParent.childCount >= maxUsers)
        {
            ShowNotification("로비가 가득 찼습니다.");
            return;
        }

        testUserCount++;
        string nick = $"TestUser_{testUserCount}";

        // 새로 추가된 유저는 기본 대기 상태
        AddUser(nick, false);

        ShowNotification($"{nick} 님이 로비에 입장했습니다");
    }


    /// <summary>
    /// 슬롯 생성 & 초기화
    /// </summary>
    public void AddUser(string nickname, bool isReady)
    {
        GameObject slotGO = Instantiate(userSlotPrefab, contentParent);
        var ui = slotGO.GetComponent<UserSlotUI>();
        if (ui != null)
            ui.Initialize(nickname, isReady);
        else
            Debug.LogError("UserSlotUI 컴포넌트를 찾을 수 없습니다!");
    }

    /// <summary>
    /// OUT USER 테스트 버튼
    /// </summary>
    public void TestRemoveUserSingle()
    {
        int cnt = contentParent.childCount;
        if (cnt == 0)
        {
            ShowNotification("로비에 유저가 없습니다.");
            return;
        }

        Transform lastSlot = contentParent.GetChild(cnt - 1);
        var ui = lastSlot.GetComponent<UserSlotUI>();
        string removedName = ui != null ? ui.nicknameText.text : "Unknown";
        Destroy(lastSlot.gameObject);

        ShowNotification($"{removedName} 님이 로비에서 나갔습니다");
    }

    /// <summary>
    /// EXIT 버튼
    /// </summary>
    public void ExitLobby()
    {
        StopAllCoroutines();
        StartCoroutine(DoExit());
    }

    private IEnumerator DoExit()
    {
        notificationText.text = "로비에서 나갑니다";
        float t = 0f;
        while (t < 0.2f)
        {
            t += Time.deltaTime;
            notificationGroup.alpha = Mathf.Lerp(0f, 1f, t / 0.2f);
            yield return null;
        }
        notificationGroup.alpha = 1f;

        yield return new WaitForSeconds(notificationDuration);
        SceneManager.LoadScene(menuSceneName);
    }

    /// <summary>
    /// 중앙 팝업
    /// </summary>
    public void ShowNotification(string message, float duration = -1f)
    {
        // 이전 알림만 중단
        if (notificationCoroutine != null)
            StopCoroutine(notificationCoroutine);

        // duration < 0이면 기본 notificationDuration 사용
        float useDur = duration < 0 ? notificationDuration : duration;
        notificationCoroutine = StartCoroutine(NotificationCoroutine(message, useDur));
    }


    private IEnumerator NotificationCoroutine(string message, float duration)
    {
        notificationText.text = message;

        // Fade In (0.2초)
        float t = 0f;
        while (t < 0.2f)
        {
            t += Time.deltaTime;
            notificationGroup.alpha = Mathf.Lerp(0f, 1f, t / 0.2f);
            yield return null;
        }
        notificationGroup.alpha = 1f;

        // message 표시 시간만큼 대기
        yield return new WaitForSeconds(duration);

        // Fade Out (0.2초)
        t = 0f;
        while (t < 0.2f)
        {
            t += Time.deltaTime;
            notificationGroup.alpha = Mathf.Lerp(1f, 0f, t / 0.2f);
            yield return null;
        }
        notificationGroup.alpha = 0f;
    }


    /// <summary>
    /// 준비 버튼 클릭
    /// </summary>
    public void OnReadyButtonClicked()
    {
        // 1) 로컬 준비 상태 토글
        localIsReady = !localIsReady;

        // 2) 버튼 텍스트 갱신
        readyButtonText.text = localIsReady ? "준비 완료" : "준비 하기";

        // 3) 내 슬롯(마지막 슬롯)에 준비 상태 반영
        if (contentParent.childCount > 0)
        {
            var lastSlot = contentParent.GetChild(contentParent.childCount - 1);
            var ui = lastSlot.GetComponent<UserSlotUI>();
            if (ui != null)
                ui.SetReadyState(localIsReady);
        }

        // 4) 모두 준비되었는지 확인
        TryStartGameCountdown();
    }

    /// <summary>
    /// 모두 준비되면 3초 카운트 후 씬 전환
    /// </summary>
    private void TryStartGameCountdown()
    {
        if (countdownCoroutine != null) return;

        foreach (Transform t in contentParent)
        {
            var ui = t.GetComponent<UserSlotUI>();
            if (ui == null || !ui.IsReady)
                return;
        }

        countdownCoroutine = StartCoroutine(StartGameCountdown());
    }

    private IEnumerator StartGameCountdown()
    {
        for (int i = 3; i >= 1; i--)
        {
            ShowNotification($"{i}초 후 게임 시작", 1f);
            yield return new WaitForSeconds(1f);
        }
        SceneManager.LoadScene(gameSceneName);
    }


    /// <summary>
    /// 테스트용: 모든 슬롯을 Ready 상태로 세팅하고 카운트다운 시작
    /// </summary>
    /// <summary>
    /// 테스트용: 모든 슬롯을 Ready로 바꾸고 즉시 3초 카운트다운 시작
    /// </summary>
    public void TestReadyAll()
    {
        int cnt = contentParent.childCount;
        if (cnt == 0)
        {
            ShowNotification("로비에 유저가 없습니다.");
            return;
        }

        // 1) 모든 슬롯에 Ready 상태 반영
        foreach (Transform t in contentParent)
        {
            var ui = t.GetComponent<UserSlotUI>();
            if (ui != null)
                ui.SetReadyState(true);
        }

        // 2) 이미 카운트다운 중이면 중단
        if (countdownCoroutine != null)
            StopCoroutine(countdownCoroutine);

        // 3) 3초 카운트다운 코루틴 실행
        countdownCoroutine = StartCoroutine(StartGameCountdown());
    }
}
