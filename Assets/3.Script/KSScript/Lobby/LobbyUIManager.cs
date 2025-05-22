using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using Mirror;

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
    [Tooltip("모두 준비되면 넘어갈 씬 이름")]
    public string gameSceneName = "KSScene";

    [Header("Buttons")]
    [Tooltip("한 명만 Ready 토글할 버튼")]
    public Button readyButton;             
    [Tooltip("전체 Ready 테스트용 버튼")]
    public Button testReadyButton;
    [Tooltip("Ready 버튼 안의 텍스트 컴포넌트")]
    public TMP_Text readyButtonText;

    // 내부 상태
    private bool localIsReady = false;
    private Coroutine countdownCoroutine = null;
    private Coroutine notificationCoroutine = null;

    private void Awake()
    {
        // Inspector: readyButton에는 OnReadyButtonClicked()만 연결해주세요.
        if (testReadyButton != null)
            testReadyButton.onClick.AddListener(OnTestReadyAll);

        // 알림 UI 초기 숨김
        notificationGroup.alpha = 0f;
        notificationGroup.interactable = notificationGroup.blocksRaycasts = false;
    }

    public void TestAddUserSingle()
    {
        if (contentParent.childCount >= maxUsers)
        {
            ShowNotification("로비가 가득 찼습니다.");
            return;
        }
        int id = contentParent.childCount + 1;
        AddUser($"TestUser_{id}", false);
        ShowNotification($"TestUser_{id} 님이 입장했습니다.");
    }

    public void TestRemoveUserSingle()
    {
        if (contentParent.childCount == 0)
        {
            ShowNotification("로비에 유저가 없습니다.");
            return;
        }
        var last = contentParent.GetChild(contentParent.childCount - 1);
        var ui = last.GetComponent<UserSlotUI>();
        string name = ui != null ? ui.nicknameText.text : "Unknown";
        Destroy(last.gameObject);
        ShowNotification($"{name} 님이 퇴장했습니다.");
    }

    public void AddUser(string nickname, bool isReady)
    {
        var slotGO = Instantiate(userSlotPrefab, contentParent);
        var ui = slotGO.GetComponent<UserSlotUI>();
        if (ui != null)
            ui.Initialize(nickname, isReady);
        else
            Debug.LogError("UserSlotUI 컴포넌트를 찾을 수 없습니다!");
    }

    public void ExitLobby()
    {
        if (notificationCoroutine != null)
            StopCoroutine(notificationCoroutine);
        StartCoroutine(DoExit());
        NetworkManager.singleton.StopHost();
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
        SceneManager.LoadScene("RoomSelectScene");
    }

    public void ShowNotification(string message, float duration = -1f)
    {
        if (notificationCoroutine != null)
            StopCoroutine(notificationCoroutine);

        float useDur = duration < 0 ? notificationDuration : duration;
        notificationCoroutine = StartCoroutine(NotificationCoroutine(message, useDur));
    }

    private IEnumerator NotificationCoroutine(string message, float duration)
    {
        notificationText.text = message;
        float t = 0f;
        while (t < 0.2f)
        {
            t += Time.deltaTime;
            notificationGroup.alpha = Mathf.Lerp(0f, 1f, t / 0.2f);
            yield return null;
        }
        notificationGroup.alpha = 1f;
        notificationGroup.interactable = notificationGroup.blocksRaycasts = true;

        yield return new WaitForSeconds(duration);

        t = 0f;
        while (t < 0.2f)
        {
            t += Time.deltaTime;
            notificationGroup.alpha = Mathf.Lerp(1f, 0f, t / 0.2f);
            yield return null;
        }
        notificationGroup.alpha = 0f;
        notificationGroup.interactable = notificationGroup.blocksRaycasts = false;
    }

    /// <summary>
    /// READY 버튼: 1명일 때 차단 → 토글 → 2명 이상 & 모두 준비 시 합쳐진 카운트다운
    /// </summary>
    public void OnReadyButtonClicked()
    {
        // 1명일 때 준비 시도 차단
        if (!localIsReady && contentParent.childCount < 2)
        {
            ShowNotification("혼자서는 게임을 실행할 수 없어요!\n다른 인원을 기다려 보아요!", 2f);
            return;
        }

        // 진행 중인 카운트다운 취소
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
            countdownCoroutine = null;
        }

        // 상태 토글
        localIsReady = !localIsReady;
        readyButtonText.text = localIsReady ? "준비 완료" : "준비 하기";

        // 마지막 슬롯에 반영
        if (contentParent.childCount > 0)
        {
            var ui = contentParent
                       .GetChild(contentParent.childCount - 1)
                       .GetComponent<UserSlotUI>();
            if (ui != null)
                ui.SetReadyState(localIsReady);
        }

        // 토글 팝업
        ShowNotification(localIsReady ? "준비 되었습니다." : "준비가 취소되었습니다.");

        // 2명 이상 & 모두 준비 시 카운트다운
        if (contentParent.childCount >= 2 && AllSlotsReady())
        {
            countdownCoroutine = StartCoroutine(StartGameCountdownCombined());
        }
    }

    private bool AllSlotsReady()
    {
        foreach (Transform t in contentParent)
        {
            var ui = t.GetComponent<UserSlotUI>();
            if (ui == null || !ui.IsReady)
                return false;
        }
        return true;
    }

    /// <summary>
    /// 특수 메시지 1초 → 3초 카운트다운 → 씬 전환
    /// </summary>
    private IEnumerator StartGameCountdownCombined()
    {
        ShowNotification("모든 유저가 준비되었습니다\n게임을 시작합니다!", 1f);
        yield return new WaitForSeconds(1f);

        for (int i = 3; i >= 1; i--)
        {
            ShowNotification($"{i}초 후 게임 시작", 1f);
            yield return new WaitForSeconds(1f);
        }

        SceneManager.LoadScene("RoomSelectScene");
    }

    public void OnTestReadyAll()
    {
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
            countdownCoroutine = null;
        }

        foreach (Transform t in contentParent)
        {
            var ui = t.GetComponent<UserSlotUI>();
            if (ui != null)
                ui.SetReadyState(true);
        }

        countdownCoroutine = StartCoroutine(StartGameCountdownCombined());
    }
}
