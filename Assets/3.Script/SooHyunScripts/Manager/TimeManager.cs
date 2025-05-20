using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance;

    [Header("UI")]
    public Image Clock_Filled_textScene;
    public Image Clock_Filled_drawScene;

    [Header("타이머 설정")]
    public float time_limit = 10f;
    private float time_start = 0f;
    private bool isClick = false;

    [Header("Canvas 설정")]
    public GameObject DrawCanvas;
    public GameObject TextCanvas;

    [Header("TexturePainter 연결")]
    public TexturePainter paint;

    [Header("Text / TextInput")]
    public TextMeshProUGUI textDisplay;    // 이전 데이터 보여주는 영역
    public TMP_InputField textInput;       // 텍스트 입력

    [Header("문장 입력 턴에 보여줄 그림")]
    public RawImage imageDisplay; // 그림 표시용 UI

    [Header("플레이어 턴 관리")]
    public int totalTurns = 4; // 총 턴 수
    [SerializeField]private int currentTurn = 0;

    private bool isDrawTurn => currentTurn % 2 == 1;

    // 저장 리스트
    public List<string> sentenceList = new List<string>();
    public List<Texture2D> drawingList = new List<Texture2D>();

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
        UpdateCanvasState();
    }

    private void Update()
    {
        if (isDrawTurn)
        {
            if (Clock_Filled_drawScene == null) return;

            time_start += Time.deltaTime;
            float t = Mathf.Clamp01(time_start / time_limit);
            Clock_Filled_drawScene.fillAmount = t;

            if (t >= 1f || isClick)
            {
                EndDrawingTurn();
            }
        }
        else
        {
            if (Clock_Filled_textScene == null) return;

            time_start += Time.deltaTime;
            float t = Mathf.Clamp01(time_start / time_limit);
            Clock_Filled_textScene.fillAmount = t;

            if (t >= 1f || isClick)
            {
                EndTextTurn();
            }
        }
    }

    public void ForceEndTurn()
    {
        isClick = true;
    }

    private void EndTextTurn()
    {
        isClick = false;
        time_start = 0f;

        string inputText = textInput != null ? textInput.text : "";
        sentenceList.Add(inputText);

        currentTurn++;

        UpdateCanvasState();
    }

    private void EndDrawingTurn()
    {
        isClick = false;
        time_start = 0f;

        // 현재 Texture 저장
        Texture2D savedTexture = paint.GetTextureCopy();
        drawingList.Add(savedTexture);

        paint.ClearTexture(); // 다음 사람을 위해 초기화

        currentTurn++;

        UpdateCanvasState();
    }

    private void UpdateCanvasState()
    {
        bool draw = isDrawTurn;

        TextCanvas.SetActive(!draw);
        DrawCanvas.SetActive(draw);

        if (draw)
        {
            // 그림 그리기 턴 → 문장 보여주기
            int index = drawingList.Count < sentenceList.Count ? drawingList.Count : sentenceList.Count - 1;

            if (textDisplay != null)
            {
                if (index >= 0 && index < sentenceList.Count)
                    textDisplay.text = sentenceList[index];
                else
                    textDisplay.text = "";
            }

            if (imageDisplay != null)
                imageDisplay.gameObject.SetActive(false); // 그림은 숨김
        }
        else
        {
            // 문장 입력 턴 → 이전 그림 보여주기
            int index = drawingList.Count - 1;

            if (imageDisplay != null)
            {
                if (index >= 0 && index < drawingList.Count)
                {
                    imageDisplay.texture = drawingList[index];
                    imageDisplay.gameObject.SetActive(true);
                }
                else
                {
                    imageDisplay.gameObject.SetActive(false);
                }
            }

            if (textDisplay != null)
                textDisplay.text = "그림을 보고 문장을 작성해 주세요.";
        }

        // 입력창 초기화
        if (textInput != null)
            textInput.text = "";

        if (Clock_Filled_textScene != null) Clock_Filled_textScene.fillAmount = 0f;
        if (Clock_Filled_drawScene != null) Clock_Filled_drawScene.fillAmount = 0f;

        // 턴 종료 시 결과 씬으로 이동
        if (currentTurn >= totalTurns)
        {
            SceneManager.LoadScene("ResultScene");
        }
    }

}