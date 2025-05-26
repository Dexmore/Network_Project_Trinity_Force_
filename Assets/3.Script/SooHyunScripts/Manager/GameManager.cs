// GameManager.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;
using System.Collections.Generic;

public struct GameStartMsg : NetworkMessage { }
public struct ProceedToNextPhaseMsg : NetworkMessage { }
public struct GameResultMsg : NetworkMessage
{
    public List<PlayerResultData> results;
}

public enum CanvasType { Text, Draw, Guess }

[System.Serializable]
public struct PlayerResultData
{
    public string playerName;
    public string sentence;
    public byte[] drawing1;
    public string guess;
    public byte[] drawing2;
}

public class GameManager : MonoBehaviour
{
    // UI elements
    public CanvasType type;
    public GameObject TextCanvas, DrawCanvas, GuessCanvas, WaitingCanvas, ResultCanvas;
    public TMP_InputField TextCanvasInput, GuessCanvasInput;
    public Button TextSubmitButton, DrawSubmitButton, GuessSubmitButton;
    public Image Timer_Image_filled;
    public TextMeshProUGUI text;
    public RawImage guessRawImage;
    public TextMeshProUGUI playerNameText, sentenceText, guessText;
    public RawImage drawingImage, guessDrawingImage;
    public Button prevButton, nextButton, closeButton;

    [SerializeField] private TexturePainter texturePainter;

    private List<PlayerResult> allResults = new();
    private List<PlayerResult> receivedResults = new();
    private int playerResultIndex = 0;
    private float timeElapsed = 0f;
    private bool isTiming = false;
    private bool hasSubmitted = false;
    private int currentPhaseIndex = -1;
    private const int maxPhases = 4;

    private string lastReceivedSentence = "";
    private string lastReceivedGuess = "";

    void Start()
    {
        NetworkClient.RegisterHandler<GameStartMsg>(_ => BeginGame());
        NetworkClient.RegisterHandler<ProceedToNextPhaseMsg>(_ => ProceedToNextPhase());
        NetworkClient.RegisterHandler<GameResultMsg>(OnReceiveResultFromServer);

        TextCanvas.SetActive(false);
        DrawCanvas.SetActive(false);
        GuessCanvas.SetActive(false);
        WaitingCanvas.SetActive(true);
        if (ResultCanvas != null) ResultCanvas.SetActive(false);

        texturePainter ??= FindObjectOfType<TexturePainter>();

        TextSubmitButton.onClick.AddListener(SubmitTextToServer);
        DrawSubmitButton.onClick.AddListener(SubmitDrawingToServer);
        GuessSubmitButton.onClick.AddListener(SubmitGuessToServer);
    }

    void Update()
    {
        if (!isTiming) return;

        timeElapsed += Time.deltaTime;
        if (Timer_Image_filled)
            Timer_Image_filled.fillAmount = timeElapsed / TimeLimit;

        if (timeElapsed >= TimeLimit)
            SubmitToServer();
    }

    public float TimeLimit => 20f; // 기본 제한 시간

    public void BeginGame()
    {
        currentPhaseIndex = 0;
        SwitchPhase(CanvasType.Text);
    }

    public void ProceedToNextPhase()
    {
        currentPhaseIndex++;
        if (currentPhaseIndex >= maxPhases)
        {
            if (NetworkServer.active)
            {
                var checker = FindObjectOfType<ServerChecker1>();
                if (checker != null)
                {
                    var rawResults = checker.ConvertGameLogToPlayerResults();
                    var data = new List<PlayerResultData>();
                    foreach (var r in rawResults)
                    {
                        data.Add(new PlayerResultData
                        {
                            playerName = r.playerName,
                            sentence = r.sentence,
                            drawing1 = r.drawing1,
                            guess = r.guess,
                            drawing2 = r.drawing2
                        });
                    }
                    NetworkServer.SendToAll(new GameResultMsg { results = data });
                    Debug.Log("[서버] 결과 메시지 전송 완료");
                }
            }
            ResultCanvas.SetActive(true);
            GoToResultScene();
            return;
        }

        SwitchPhase(currentPhaseIndex switch
        {
            1 or 3 => CanvasType.Draw,
            2 => CanvasType.Guess,
            _ => CanvasType.Text
        });
    }

    private void SwitchPhase(CanvasType newType)
    {
        type = newType;
        TextCanvas.SetActive(type == CanvasType.Text);
        DrawCanvas.SetActive(type == CanvasType.Draw);
        GuessCanvas.SetActive(type == CanvasType.Guess);
        WaitingCanvas.SetActive(false);

        if (type == CanvasType.Draw) texturePainter?.EraseAll();
        if (type == CanvasType.Guess) GuessCanvasInput.text = "";

        StartTimer();
    }

    private void StartTimer()
    {
        isTiming = true;
        hasSubmitted = false;
        timeElapsed = 0f;
        if (Timer_Image_filled) Timer_Image_filled.fillAmount = 1f;
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

    private void SubmitTextToServer()
    {
        if (hasSubmitted) return;
        hasSubmitted = true;
        var player = NetworkClient.connection.identity.GetComponent<NetworkPlayer>();
        player.CmdSetSubmitted(true);
        player.CmdSetText(TextCanvasInput.text);
        ShowWaitingCanvas();
    }

    private void SubmitDrawingToServer()
    {
        if (hasSubmitted) return;
        hasSubmitted = true;
        var player = NetworkClient.connection.identity.GetComponent<NetworkPlayer>();
        player.CmdSubmitDrawing(texturePainter.GetPNG());
        ShowWaitingCanvas();
    }

    private void SubmitGuessToServer()
    {
        if (hasSubmitted) return;
        hasSubmitted = true;
        var player = NetworkClient.connection.identity.GetComponent<NetworkPlayer>();
        player.CmdSetGuess(GuessCanvasInput.text);
        ShowWaitingCanvas();
    }

    public void ShowReceivedSentence(string message, int playerIndex)
    {
        text.text = $"My Network Index: {playerIndex + 1}\nReceived Message: {message}";
        lastReceivedSentence = message;
    }

    public void ShowReceivedDrawing(byte[] pngData, int playerIndex)
    {
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(pngData);
        if (guessRawImage != null)
            guessRawImage.texture = tex;
        text.text = $"My Network Index: {playerIndex + 1}\nReceived Drawing!";
    }

    public void ShowReceivedGuess(string guess, int playerIndex)
    {
        if (guessText != null)
            guessText.text = $"My Network Index: {playerIndex + 1}\nReceived Guess: {guess}";
    }

    private void ShowWaitingCanvas()
    {
        TextCanvas.SetActive(false);
        DrawCanvas.SetActive(false);
        GuessCanvas.SetActive(false);
        WaitingCanvas.SetActive(true);
        isTiming = false;
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
        ShowAllResults(receivedResults);
    }

    private void GoToResultScene()
    {
        if (receivedResults.Count > 0) ShowAllResults(receivedResults);
        else ShowNoResultMessage();
    }

    private void ShowNoResultMessage()
    {
        playerNameText.text = "";
        sentenceText.text = "<b>저장된 결과 데이터가 없습니다.</b>";
        drawingImage.gameObject.SetActive(false);
        guessText.text = "";
        guessDrawingImage.gameObject.SetActive(false);
        prevButton.interactable = nextButton.interactable = false;
        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(() => ResultCanvas.SetActive(false));
        ResultCanvas.SetActive(true);
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
            if (playerResultIndex > 0) ShowSinglePlayerResult(--playerResultIndex);
        });
        nextButton.onClick.AddListener(() => {
            if (playerResultIndex < allResults.Count - 1) ShowSinglePlayerResult(++playerResultIndex);
            else EndGame();
        });
        closeButton.onClick.AddListener(() => ResultCanvas.SetActive(false));
    }

    private void ShowSinglePlayerResult(int index)
    {
        if (index < 0 || index >= allResults.Count) return;

        var res = allResults[index];
        playerNameText.text = $"Player: {res.playerName}";
        sentenceText.text = $"문장: {res.sentence}";
        drawingImage.texture = res.drawing1 != null ? LoadTexture(res.drawing1) : null;
        drawingImage.gameObject.SetActive(res.drawing1 != null);
        guessText.text = $"추측: {res.guess}";
        guessDrawingImage.texture = res.drawing2 != null ? LoadTexture(res.drawing2) : null;
        guessDrawingImage.gameObject.SetActive(res.drawing2 != null);

        prevButton.interactable = index > 0;
        nextButton.interactable = index < allResults.Count - 1;
    }

    private Texture2D LoadTexture(byte[] data)
    {
        var tex = new Texture2D(2, 2);
        tex.LoadImage(data);
        return tex;
    }

    private void EndGame()
    {
        ResultCanvas.SetActive(false);
        // 추가 종료 처리 가능
    }
}
