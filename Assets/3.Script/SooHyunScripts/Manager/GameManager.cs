using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;
using System.Collections.Generic;
using System.Collections;

public enum CanvasType { Text, Draw, Guess }

public class GameTurn
{
    public string playerName;
    public string sentence;
    public byte[] drawing;
    public string guess;
    public string ownerName;
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

    private void Start()
    {
        RegisterMessageHandlers();

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

        SetAllCanvasOffExceptWaiting();

        // 무조건 Ready 신호는 Player 생성될 때까지 기다려서 보냄!
        StartCoroutine(WaitForNetworkPlayerAndReady());
    }
    void SetAllCanvasOffExceptWaiting()
    {
        if (TextCanvas) TextCanvas.SetActive(false);
        if (DrawCanvas) DrawCanvas.SetActive(false);
        if (GuessCanvas) GuessCanvas.SetActive(false);
        if (ResultCanvas) ResultCanvas.SetActive(false);
        if (WaitingCanvas) WaitingCanvas.SetActive(true);
    }

    private IEnumerator WaitForNetworkPlayerAndReady()
    {
        NetworkPlayer player = null;
        int retry = 0;
        while (player == null && retry < 60)
        {
            if (NetworkClient.connection != null && NetworkClient.connection.identity != null)
                player = NetworkClient.connection.identity.GetComponent<NetworkPlayer>();
            if (player == null)
            {
                yield return null;
                retry++;
            }
        }
        if (player != null)
        {
            player.CmdReadyForGame();
        }
        else
        {
            Debug.LogError("NetworkPlayer 못찾아서 Ready 호출 불가");
        }
    }

    public void RegisterMessageHandlers()
    {
        NetworkClient.UnregisterHandler<GameStartMsg>();
        NetworkClient.RegisterHandler<GameStartMsg>(OnGameStart);
        // 필요하면 나머지도 여기에 등록
    }

    void OnGameStart(GameStartMsg msg)
    {
        // 본게임 시작 시 UI 등 보여주기
        WaitingCanvas.SetActive(false);
        if (TextCanvas) TextCanvas.SetActive(true); // 예시
        BeginGame();
    }
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

    private void EndGame()
    {
        ResultCanvas.SetActive(false);
        // 게임 종료 추가 처리
    }
}
