using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;
using System.Collections.Generic;

public enum CanvasType { Text, Draw, Guess }
public class GameTurn
{
    public string playerName;
    public string sentence;
    public byte[] drawing;
    public string guess;
    public string ownerName; // 추가
}

public class GameManager : MonoBehaviour
{
    public CanvasType type;
    public GameObject TextCanvas, DrawCanvas, GuessCanvas, WaitingCanvas, ResultCanvas;
    public TMP_InputField TextCanvasInput, GuessCanvasInput;
    public Button TextSubmitButton, DrawSubmitButton, GuessSubmitButton;
    public Image Timer_Image_filled;
    public float TimeLimit = 20f;
    public TextMeshProUGUI text;
    public RawImage guessRawImage;

    [SerializeField] private TexturePainter texturePainter;

    private List<PlayerResult> receivedResults = new List<PlayerResult>();
    private float timeElapsed = 0f;
    private bool isTiming = false;
    private bool hasSubmitted = false;
    private int currentPhaseIndex = 0;
    private int maxPhases = 4;

    private void RegisterHandlersSafe()
    {
        Debug.Log("[GameManager] RegisterHandlersSafe 호출");
        NetworkClient.UnregisterHandler<GameStartMsg>();
        NetworkClient.UnregisterHandler<ProceedToNextPhaseMsg>();
        NetworkClient.UnregisterHandler<GameResultMsg>();
        NetworkClient.RegisterHandler<GameStartMsg>(OnGameStart);
        NetworkClient.RegisterHandler<ProceedToNextPhaseMsg>(OnProceedToNextPhase);
        NetworkClient.RegisterHandler<GameResultMsg>(OnReceiveResultFromServer);
    }

    private void Awake()
    {
        Debug.Log("[GameManager] Awake 실행됨");
        RegisterHandlersSafe();
    }
    private void OnEnable()
    {
        Debug.Log("[GameManager] OnEnable 실행됨");
        RegisterHandlersSafe();

    }
    private void Start()
    {
        Debug.Log("[GameManager] Start 실행됨, 핸들러 등록");
        RegisterHandlersSafe();

        TextCanvas.SetActive(false);
        DrawCanvas.SetActive(false);
        GuessCanvas.SetActive(false);
        WaitingCanvas.SetActive(true);

        if (texturePainter == null)
            texturePainter = FindObjectOfType<TexturePainter>();

        TextSubmitButton.onClick.AddListener(SubmitTextToServer);
        DrawSubmitButton.onClick.AddListener(SubmitDrawingToServer);
        GuessSubmitButton.onClick.AddListener(SubmitGuessToServer);

        if (ResultCanvas != null)
            ResultCanvas.SetActive(false);
    }

    void OnGameStart(GameStartMsg msg)
    {
        Debug.Log("[GameManager] OnGameStart(GameStartMsg)");
        BeginGame();
    }

    void OnProceedToNextPhase(ProceedToNextPhaseMsg msg)
    {
        Debug.Log("[GameManager] OnProceedToNextPhase");
        ProceedToNextPhase();
    }

    private void Update()
    {
        if (!isTiming) return;

        timeElapsed += Time.deltaTime;
        if (Timer_Image_filled)
            Timer_Image_filled.fillAmount = (timeElapsed / TimeLimit);

        if (timeElapsed >= TimeLimit)
        {
            Debug.Log("[GameManager] Auto Submit");
            SubmitToServer();
        }
    }

    public void BeginGame()
    {
        Debug.Log("[GameManager] BeginGame! -> TextCanvas");
        type = CanvasType.Text;
        TextCanvas.SetActive(true);
        WaitingCanvas.SetActive(false);
        StartTimer();
    }

    private void SubmitTextToServer()
    {
        if (hasSubmitted) return;
        hasSubmitted = true;
        Debug.Log("[GameManager] SubmitTextToServer");
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
        Debug.Log("[GameManager] SubmitDrawingToServer");
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
        Debug.Log("[GameManager] SubmitGuessToServer");
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
        Debug.Log("[GameManager] Timer Start");
        isTiming = true;
        timeElapsed = 0f;
        if (Timer_Image_filled) Timer_Image_filled.fillAmount = 1f;
        hasSubmitted = false;
    }

    private void ShowWaitingCanvas()
    {
        Debug.Log("[GameManager] ShowWaitingCanvas");
        TextCanvas.SetActive(false);
        DrawCanvas.SetActive(false);
        GuessCanvas.SetActive(false);
        WaitingCanvas.SetActive(true);
    }

    public void ProceedToNextPhase()
    {
        Debug.Log($"[GameManager] ProceedToNextPhase! Current Phase Index: {currentPhaseIndex}");
        TextCanvas.SetActive(false);
        DrawCanvas.SetActive(false);
        GuessCanvas.SetActive(false);
        WaitingCanvas.SetActive(false);

        currentPhaseIndex++;

        if (currentPhaseIndex >= maxPhases)
        {
            Debug.Log("[GameManager] Go To Result Scene");
            ResultCanvas.SetActive(true);
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
    }

    private void OnReceiveResultFromServer(GameResultMsg msg)
    {
        receivedResults.Clear();
        if (msg.results != null)
        {
            foreach (var r in msg.results)
            {
                receivedResults.Add(new PlayerResult
                {
                    playerName = r.playerName,
                    sentence = r.sentence,
                    drawing1 = r.drawing1,
                    guess = r.guess,
                    drawing2 = r.drawing2
                });
            }
        }
        if (ResultCanvas != null) ResultCanvas.SetActive(true);
    }

    private void GoToResultScene()
    {
        ResultCanvas.SetActive(true);
    }
}