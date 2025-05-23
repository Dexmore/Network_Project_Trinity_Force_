using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public struct GameStartMsg : NetworkMessage { }
public struct ProceedToNextPhaseMsg : NetworkMessage { }

public enum CanvasType { Text, Draw, Guess }

public class GameManager : MonoBehaviour
{
    public CanvasType type;
    public GameObject TextCanvas, DrawCanvas, GuessCanvas, WaitingCanvas;
    public TMP_InputField TextCanvasInput, GuessCanvasInput;
    public Button TextSubmitButton, DrawSubmitButton, GuessSubmitButton;
    public Image Timer_Image_filled;
    public float TimeLimit = 20f;

    public TextMeshProUGUI text;
    public TextMeshProUGUI playerIndexText; // For displaying my playerIndex

    private float timeElapsed = 0f;
    private bool isTiming = false;
    private int drawGuessPhaseCount = 0;
    private int maxPhaseCount = 0;
    [SerializeField] private TexturePainter texturePainter;

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

        TextSubmitButton.onClick.AddListener(SubmitToServer);
        DrawSubmitButton.onClick.AddListener(SubmitToServer);
        GuessSubmitButton.onClick.AddListener(SubmitToServer);

        Invoke(nameof(DisplayMyPlayerIndex), 0.5f); // Wait for network sync
    }

    private void DisplayMyPlayerIndex()
    {
        if (NetworkClient.connection?.identity?.GetComponent<NetworkPlayer>() is NetworkPlayer np)
        {
            playerIndexText.text = $"My Network Index: {np.playerIndex + 1}";
            Debug.Log($"[GameManager] My Network Index: {np.playerIndex + 1}");
        }
        else
        {
            playerIndexText.text = $"My Network Index: ???";
        }
    }

    void OnGameStart(GameStartMsg msg)
    {
        BeginGame();
    }

    void OnProceedToNextPhase(ProceedToNextPhaseMsg msg)
    {
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
            SubmitToServer();
        }
    }

    public void BeginGame()
    {
        type = CanvasType.Text;
        TextCanvas.SetActive(true);
        WaitingCanvas.SetActive(false);
        StartTimer();
    }

    private void SubmitToServer()
    {
        if (NetworkClient.connection?.identity?.GetComponent<NetworkPlayer>() is NetworkPlayer player)
        {
            player.CmdSetSubmitted(true);

            switch (type)
            {
                case CanvasType.Text:
                    player.CmdSetText(TextCanvasInput.text);
                    break;
                case CanvasType.Draw:
                    // Drawing data submit
                    break;
                case CanvasType.Guess:
                    // Guess data submit
                    break;
            }

            ShowWaitingCanvas();
        }
    }

    private void StartTimer()
    {
        isTiming = true;
        timeElapsed = 0f;
        if (Timer_Image_filled) Timer_Image_filled.fillAmount = 1f;
    }

    private void ShowWaitingCanvas()
    {
        TextCanvas.SetActive(false);
        DrawCanvas.SetActive(false);
        GuessCanvas.SetActive(false);
        WaitingCanvas.SetActive(true);
    }

    public void ProceedToNextPhase()
    {
        TextCanvas.SetActive(false);
        DrawCanvas.SetActive(false);
        GuessCanvas.SetActive(false);
        WaitingCanvas.SetActive(false);

        switch (type)
        {
            case CanvasType.Text:
                type = CanvasType.Draw;
                DrawCanvas.SetActive(true);
                break;
            case CanvasType.Draw:
                type = CanvasType.Guess;
                GuessCanvas.SetActive(true);
                break;
            case CanvasType.Guess:
                drawGuessPhaseCount++;
                if (drawGuessPhaseCount >= maxPhaseCount)
                {
                    return;
                }
                type = CanvasType.Draw;
                DrawCanvas.SetActive(true);
                break;
        }

        StartTimer();
    }

    // Display received message and player index on UI
    public void ShowReceivedSentence(string message, int playerIndex)
    {
        text.text = $"My Network Index: {playerIndex + 1}\nReceived Message: {message}";
    }
}
