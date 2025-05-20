using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance;

    [Header("UI")]
    public Image Clock_Filled;

    [Header("타이머 설정")]
    public float time_limit;
    private float time_start = 0f;
    private bool isClick = false;

    [Header("버튼")]
    public Button button;

    string[] SceneName = { "TextInputScene", "DrawScene" };
    private int i = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        SetupButton();
    }

    private void Update()
    {
        if (Clock_Filled == null) return;

        time_start += Time.deltaTime;
        float t = Mathf.Clamp01(time_start / time_limit);
        Clock_Filled.fillAmount = t;

        if (t == 1f || isClick)
        {
            isClick = false;
            time_start = 0f;

            GoToNextScene();
        }
    }

    public void ForceNextScene()
    {
        isClick = true;
    }

    private void GoToNextScene()
    {
        i++;
        if (i >= SceneName.Length)
        {
            i = 0;
            return;
        }

        SceneManager.LoadScene(SceneName[i]);
        StartCoroutine(WaitAndFindUI());
    }

    private IEnumerator WaitAndFindUI()
    {
        yield return new WaitForSeconds(0.1f); // 씬 로딩 대기

        // 타이머 이미지 다시 찾기
        Clock_Filled = GameObject.Find("Clock_Filled")?.GetComponent<Image>();
        if (Clock_Filled == null)
            Debug.LogWarning("Clock_Filled UI를 찾을 수 없습니다.");

        // 버튼 다시 찾기
        button = GameObject.Find("NextButton")?.GetComponent<Button>();
        if (button != null)
        {
            SetupButton();
        }
        else
        {
            Debug.LogWarning("NextButton을 찾을 수 없습니다.");
        }
    }

    private void SetupButton()
    {
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(ForceNextScene);
        }
    }
}
