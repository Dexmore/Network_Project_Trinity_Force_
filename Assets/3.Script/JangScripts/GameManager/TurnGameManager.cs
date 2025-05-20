using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;

public enum TurnType { Sentence, Drawing, Result }

[System.Serializable]
public class TurnInfo
{
    public TurnType turnType;
    public string prompt;
    public string playerId;
    public string imagePath;
}

public class TurnGameManager : MonoBehaviour
{
    [Header("Sentence UI")]
    public GameObject sentencePanel;
    public Text promptText;
    public InputField sentenceInput;
    public Button submitSentenceButton;

    [Header("Drawing UI")]
    public GameObject drawingPanel;
    public Text drawingPromptText;
    public Button submitDrawingButton;

    [Header("Drawing Objects")]
    public CustomTexturePainter drawingSystem;
    public GameObject Drawing;

    [Header("Result UI")]
    public GameObject resultPanel;
    public Transform resultContent;
    public GameObject resultTextPrefab;
    public GameObject resultImagePrefab;

    [Header("Game Settings")]
    [SerializeField] private int playerCount = 4;

    private List<TurnInfo> turns = new List<TurnInfo>();
    private int currentTurnIndex = 0;

    void Start()
    {
        if (Drawing != null) Drawing.SetActive(false);
        drawingSystem.gameObject.SetActive(false);
        sentencePanel.SetActive(false);
        drawingPanel.SetActive(false);
        resultPanel.SetActive(false);

        submitSentenceButton.onClick.AddListener(OnSubmitSentence);
        submitDrawingButton.onClick.AddListener(() => StartCoroutine(OnSubmitDrawing()));

        turns.Add(new TurnInfo { turnType = TurnType.Sentence, prompt = "문장을 입력하세요!", playerId = "Player 1" });
        ShowSentence(turns[0].prompt);
    }

    void OnSubmitSentence()
    {
        string sentence = sentenceInput.text.Trim();
        if (string.IsNullOrEmpty(sentence)) return;

        turns[currentTurnIndex].prompt = sentence;

        turns.Add(new TurnInfo { turnType = TurnType.Drawing, prompt = sentence, playerId = $"Player {turns.Count + 1}" });
        currentTurnIndex++;

        ShowDrawing(sentence);
    }

    IEnumerator OnSubmitDrawing()
    {
        Texture2D texture = drawingSystem.GetTexture();
        if (texture == null) yield break;

        byte[] bytes = texture.EncodeToPNG();
        string path = Path.Combine(Application.persistentDataPath, $"drawing_{System.DateTime.Now.Ticks}.png");
        File.WriteAllBytes(path, bytes);

        turns[currentTurnIndex].imagePath = path;
        drawingSystem.ClearTexture();

        HideAllPanels();
        sentenceInput.text = "";
        currentTurnIndex++;

        if (currentTurnIndex >= playerCount * 2)
        {
            ShowResult();
        }
        else
        {
            turns.Add(new TurnInfo { turnType = TurnType.Sentence, prompt = "다음 문장을 입력하세요!", playerId = $"Player {turns.Count + 1}" });
            ShowSentence("다음 문장을 입력하세요!");
        }
    }

    void ShowSentence(string prompt)
    {
        HideAllPanels();
        promptText.text = prompt;
        sentencePanel.SetActive(true);
    }

    void ShowDrawing(string prompt)
    {
        HideAllPanels();
        drawingPromptText.text = prompt;
        drawingPanel.SetActive(true);
        Drawing.SetActive(true);
        drawingSystem.gameObject.SetActive(true);
    }

    void HideAllPanels()
    {
        sentencePanel.SetActive(false);
        drawingPanel.SetActive(false);
        Drawing.SetActive(false);
        drawingSystem.gameObject.SetActive(false);
        resultPanel.SetActive(false);
    }

    void ShowResult()
    {
        Debug.Log("showresult호출됨");
        HideAllPanels();
        if (resultPanel != null)
            resultPanel.SetActive(true);

        // 기존 결과 지우기
        if (resultContent != null)
        {
            foreach (Transform child in resultContent)
                Destroy(child.gameObject);
        }

        foreach (var turn in turns)
        {
            if (turn.turnType == TurnType.Sentence)
            {
                if (resultTextPrefab != null && resultContent != null)
                {
                    GameObject txt = Instantiate(resultTextPrefab, resultContent);
                    var Text = txt.GetComponentInChildren<TextMeshProUGUI>();
                    if (Text != null)
                    {
                        Text.text = $"{turn.playerId}의 문장: {turn.prompt}";
                    }
                    else
                    {
                        Debug.LogError("❌ Text 컴포넌트가 resultTextPrefab 안에 없습니다.");
                    }
                }
            }
            else if (turn.turnType == TurnType.Drawing)
            {
                if (resultImagePrefab != null && resultContent != null)
                {
                    GameObject img = Instantiate(resultImagePrefab, resultContent);
                    var rawImg = img.GetComponentInChildren<RawImage>();
                    if (rawImg != null)
                    {
                        Debug.Log("📁 imagePath: " + turn.imagePath);
                        Debug.LogError("❌ RawImage 못 찾음!");
                        StartCoroutine(LoadImage("file://" + turn.imagePath, rawImg));
                    }
                    else
                    {
                        Debug.LogError("❌ RawImage 컴포넌트가 resultImagePrefab 안에 없습니다.");
                    }
                }
            }
        }
    }


    IEnumerator LoadImage(string path, RawImage target)
    {
        using (WWW www = new WWW(path))
        {
            yield return www;
            if (!string.IsNullOrEmpty(www.error))
            {
                Debug.LogError("❌ 이미지 로드 실패: " + www.error);
            }
            else
            {
                target.texture = www.texture;
                Debug.Log("✅ 이미지 로딩 성공!");
            }
        }
    }

}
