using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;
using UnityEngine.SceneManagement;

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
        SceneManager.LoadScene("ResultScene");
    }

    public void ShowNextResult()
    {
        // ResultCanvas에서 순차적으로 결과 보여주는 로직(필요시 구현)
    }
}
