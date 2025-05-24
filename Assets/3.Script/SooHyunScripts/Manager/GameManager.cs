using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public struct GameStartMsg : NetworkMessage { }
public struct ProceedToNextPhaseMsg : NetworkMessage { }
public enum CanvasType { Text, Draw, Guess }

public class GameManager : MonoBehaviour
{
    public CanvasType type;
    public GameObject TextCanvas, DrawCanvas, GuessCanvas, WaitingCanvas, ResultCanvas;
    public TMP_InputField TextCanvasInput, GuessCanvasInput;
    public Button TextSubmitButton, DrawSubmitButton, GuessSubmitButton, NextResultButton;
    public Image Timer_Image_filled;
    public float TimeLimit = 20f;
    public TextMeshProUGUI text;
    public RawImage guessRawImage;

    public TextMeshProUGUI resultDescriptionText;
    public RawImage resultDrawingImage;
    public Button resultPrevButton;
    public Button resultNextButton;
    public Button resultCloseButton;

    private List<ServerChecker.GameTurn> resultList;
    private int resultIndex = 0;

    [SerializeField] private TexturePainter texturePainter;

    private float timeElapsed = 0f;
    private bool isTiming = false;
    private bool hasSubmitted = false;
    private int currentPhaseIndex = 0;
    private int maxPhases = 4; // 문장, 그림, 추측, 그림

    private string lastReceivedSentence = "";
    private string lastReceivedGuess = "";

    private void Start()
    {
        NetworkClient.RegisterHandler<GameStartMsg>(OnGameStart);
        NetworkClient.RegisterHandler<ProceedToNextPhaseMsg>(OnProceedToNextPhase);

        TextCanvas.SetActive(false);
        DrawCanvas.SetActive(false);
        GuessCanvas.SetActive(false);
        WaitingCanvas.SetActive(true);

        if (texturePainter == null)
            texturePainter = FindObjectOfType<TexturePainter>();

        TextSubmitButton.onClick.AddListener(SubmitTextToServer);
        DrawSubmitButton.onClick.AddListener(SubmitDrawingToServer);
        GuessSubmitButton.onClick.AddListener(SubmitGuessToServer);

        if (NextResultButton != null)
            NextResultButton.onClick.AddListener(ShowNextResult);

        if (ResultCanvas != null)
            ResultCanvas.SetActive(false);
    }

    void OnGameStart(GameStartMsg msg) { BeginGame(); }
    void OnProceedToNextPhase(ProceedToNextPhaseMsg msg) { ProceedToNextPhase(); }

    private void Update()
    {
        if (!isTiming) return;

        timeElapsed += Time.deltaTime;
        if (Timer_Image_filled)
            Timer_Image_filled.fillAmount = (timeElapsed / TimeLimit);

        if (timeElapsed >= TimeLimit)
            SubmitToServer();
    }

    public void BeginGame()
    {
        type = CanvasType.Text;
        TextCanvas.SetActive(true);
        WaitingCanvas.SetActive(false);
        StartTimer();
    }

    private void SubmitTextToServer()
    {
        if (hasSubmitted) return;
        hasSubmitted = true;
        if (NetworkClient.connection?.identity?.GetComponent<NetworkPlayer>() is NetworkPlayer player)
        {
            player.CmdSetSubmitted(true);
            player.CmdSetText(TextCanvasInput.text);
            ShowWaitingCanvas();
        }
    }

    private void SubmitDrawingToServer()
    {
        if (hasSubmitted) return;
        hasSubmitted = true;
        if (NetworkClient.connection?.identity?.GetComponent<NetworkPlayer>() is NetworkPlayer player)
        {
            byte[] pngData = texturePainter.GetPNG();
            player.CmdSubmitDrawing(pngData);
            ShowWaitingCanvas();
        }
    }

    private void SubmitGuessToServer()
    {
        if (hasSubmitted) return;
        hasSubmitted = true;
        if (NetworkClient.connection?.identity?.GetComponent<NetworkPlayer>() is NetworkPlayer player)
        {
            player.CmdSetGuess(GuessCanvasInput.text);
            ShowWaitingCanvas();
        }
    }

    private void SubmitToServer()
    {
        switch (type)
        {
            case CanvasType.Text: SubmitTextToServer(); break;
            case CanvasType.Draw: SubmitDrawingToServer(); break;
            case CanvasType.Guess: SubmitGuessToServer(); break;
        }
    }

    private void StartTimer()
    {
        isTiming = true;
        timeElapsed = 0f;
        if (Timer_Image_filled) Timer_Image_filled.fillAmount = 1f;
        hasSubmitted = false;
    }

    private void ShowWaitingCanvas()
    {
        TextCanvas.SetActive(false);
        DrawCanvas.SetActive(false);
        GuessCanvas.SetActive(false);
        WaitingCanvas.SetActive(true);
        isTiming = false;
    }

    public void ProceedToNextPhase()
    {
        TextCanvas.SetActive(false);
        DrawCanvas.SetActive(false);
        GuessCanvas.SetActive(false);
        WaitingCanvas.SetActive(false);

        currentPhaseIndex++;

        if (currentPhaseIndex >= maxPhases)
        {
            GoToResultScene();
            return;
        }

        if (currentPhaseIndex == 0)
        {
            type = CanvasType.Text;
            TextCanvas.SetActive(true);
        }
        else if (currentPhaseIndex == 1 || currentPhaseIndex == 3)
        {
            type = CanvasType.Draw;
            DrawCanvas.SetActive(true);
            if (texturePainter != null) texturePainter.EraseAll();
        }
        else if (currentPhaseIndex == 2)
        {
            type = CanvasType.Guess;
            GuessCanvas.SetActive(true);
            GuessCanvasInput.text = "";
        }

        StartTimer();
    }

    public void ShowResultCanvas(List<ServerChecker.GameTurn> resultLog)
    {
        if (resultLog == null || resultLog.Count == 0) return;

        resultList = resultLog;
        resultIndex = 0;
        ResultCanvas.SetActive(true);
        ShowSingleResult(resultIndex);

        resultPrevButton.onClick.RemoveAllListeners();
        resultNextButton.onClick.RemoveAllListeners();
        resultCloseButton.onClick.RemoveAllListeners();

        resultPrevButton.onClick.AddListener(() => {
            if (resultIndex > 0) { resultIndex--; ShowSingleResult(resultIndex); }
        });
        resultNextButton.onClick.AddListener(() => {
            if (resultIndex < resultList.Count - 1) { resultIndex++; ShowSingleResult(resultIndex); }
        });
        resultCloseButton.onClick.AddListener(() => {
            ResultCanvas.SetActive(false);
            // 게임 재시작/방 나가기 등 추가 동작 필요시 여기서!
        });
    }

    private void ShowSingleResult(int index)
    {
        if (resultList == null || index < 0 || index >= resultList.Count) return;
        var turn = resultList[index];

        if (turn.isText)
        {
            resultDescriptionText.text = $"Step {index + 1}: {turn.sentence}";
            resultDrawingImage.gameObject.SetActive(false);
        }
        else
        {
            resultDescriptionText.text = $"Step {index + 1}: Drawing";
            resultDrawingImage.gameObject.SetActive(true);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(turn.drawing);
            resultDrawingImage.texture = tex;
        }
        resultPrevButton.interactable = (index > 0);
        resultNextButton.interactable = (index < resultList.Count - 1);
    }

    public void ShowReceivedSentence(string message, int playerIndex)
    {
        text.text = $"My Network Index: {playerIndex + 1}\nReceived Message: {message}";
        lastReceivedSentence = message;
    }

    public void ShowReceivedDrawing(byte[] pngData, int playerIndex)
    {
        Texture2D receivedDrawing = new Texture2D(2, 2);
        receivedDrawing.LoadImage(pngData);
        if (guessRawImage != null)
            guessRawImage.texture = receivedDrawing;
        text.text = $"My Network Index: {playerIndex + 1}\nReceived Drawing!";
    }

    public void ShowReceivedGuess(string guess, int playerIndex)
    {
        text.text = $"My Network Index: {playerIndex + 1}\nReceived Guess: {guess}";
        lastReceivedGuess = guess;
    }

    private void GoToResultScene()
    {
        var checker = FindObjectOfType<ServerChecker>();
        if (checker != null)
            ShowResultCanvas(checker.gameLog);
    }


    public void ShowNextResult()
    {
        // ResultCanvas에서 순차적으로 결과 보여주는 로직(필요시 구현)
    }
}
