using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;
using System.Collections.Generic;

public struct GameStartMsg : NetworkMessage { }
public struct ProceedToNextPhaseMsg : NetworkMessage { }
public enum CanvasType { Text, Draw, Guess }

public class PlayerResult
{
    public string playerName;
    public string sentence;
    public byte[] drawing1;
    public string guess;
    public byte[] drawing2;
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

    public TextMeshProUGUI playerNameText;
    public TextMeshProUGUI sentenceText;
    public RawImage drawingImage;
    public TextMeshProUGUI guessText;
    public RawImage guessDrawingImage;
    public Button prevButton;
    public Button nextButton;
    public Button closeButton;

    [SerializeField] private TexturePainter texturePainter;

    private List<PlayerResult> allResults = new List<PlayerResult>();
    private int playerResultIndex = 0;
    private float timeElapsed = 0f;
    private bool isTiming = false;
    private bool hasSubmitted = false;
    private int currentPhaseIndex = 0;
    private int maxPhases = 4;

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
        {
            List<PlayerResult> playerResults = checker.ConvertGameLogToPlayerResults();
            if (playerResults == null || playerResults.Count == 0)
            {
                playerResults = new List<PlayerResult>();
                for (int i = 0; i < 4; ++i)
                {
                    playerResults.Add(new PlayerResult
                    {
                        playerName = $"Player{i + 1}",
                        sentence = $"(값없음) 문장{i + 1}",
                        drawing1 = new byte[0],
                        guess = $"(값없음) 추측{i + 1}",
                        drawing2 = new byte[0]
                    });
                }
            }
            ShowAllResults(playerResults);
        }
        else
        {
            var dummyResults = new List<PlayerResult>();
            for (int i = 0; i < 4; ++i)
            {
                dummyResults.Add(new PlayerResult
                {
                    playerName = $"Player{i + 1}",
                    sentence = $"(Server없음) 문장{i + 1}",
                    drawing1 = new byte[0],
                    guess = $"(Server없음) 추측{i + 1}",
                    drawing2 = new byte[0]
                });
            }
            ShowAllResults(dummyResults);
        }
    }

    public void ShowAllResults(List<PlayerResult> results)
    {
        allResults = results;
        playerResultIndex = 0;
        ResultCanvas.SetActive(true);
        ShowSinglePlayerResult(playerResultIndex);

        prevButton.onClick.RemoveAllListeners();
        nextButton.onClick.RemoveAllListeners();
        closeButton.onClick.RemoveAllListeners();

        prevButton.onClick.AddListener(() => {
            if (playerResultIndex > 0)
            {
                playerResultIndex--;
                ShowSinglePlayerResult(playerResultIndex);
            }
        });
        nextButton.onClick.AddListener(() => {
            if (playerResultIndex < allResults.Count - 1)
            {
                playerResultIndex++;
                ShowSinglePlayerResult(playerResultIndex);
            }
            else
            {
                EndGame();
            }
        });
        closeButton.onClick.AddListener(() => {
            ResultCanvas.SetActive(false);
        });
    }

    private void ShowSinglePlayerResult(int index)
    {
        if (allResults == null || index < 0 || index >= allResults.Count) return;

        var res = allResults[index];
        playerNameText.text = $"Player: {res.playerName}";
        sentenceText.text = $"문장: {res.sentence}";

        if (res.drawing1 != null && res.drawing1.Length > 0)
        {
            Texture2D tex1 = new Texture2D(2, 2);
            tex1.LoadImage(res.drawing1);
            drawingImage.texture = tex1;
            drawingImage.gameObject.SetActive(true);
        }
        else
        {
            drawingImage.gameObject.SetActive(false);
        }

        guessText.text = $"추측: {res.guess}";

        if (res.drawing2 != null && res.drawing2.Length > 0)
        {
            Texture2D tex2 = new Texture2D(2, 2);
            tex2.LoadImage(res.drawing2);
            guessDrawingImage.texture = tex2;
            guessDrawingImage.gameObject.SetActive(true);
        }
        else
        {
            guessDrawingImage.gameObject.SetActive(false);
        }

        prevButton.interactable = (index > 0);
        nextButton.interactable = (index < allResults.Count - 1);
    }

    private void EndGame()
    {
        ResultCanvas.SetActive(false);
        // 게임 종료 추가 처리
    }
}
