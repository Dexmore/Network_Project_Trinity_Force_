using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Mirror;

[System.Serializable]
public class TurnChain
{
    public string originalSentence;
    public int ownerPlayerIndex;
    public List<Texture2D> drawings = new();
    public List<string> guesses = new();

    public TurnChain(string sentence, int playerIndex)
    {
        originalSentence = sentence;
        ownerPlayerIndex = playerIndex;
    }
}

public class TimeManager : NetworkBehaviour
{
    // UI 연결은 런타임에 자동
    private GameObject sentencePanel;
    private GameObject drawingPanel;
    private GameObject guessPanel;

    private TMP_InputField sentenceInput;
    private TMP_InputField guessInput;
    private RawImage guessImage;
    private TexturePainter paint;

    [Header("설정")]
    public int totalPlayers = 4;
    public float timeLimit = 30f;

    private float timeStart = 0f;
    private bool isClick = false;

    public List<TurnChain> chains = new();

    [SyncVar] private int currentPlayer = 0;
    [SyncVar] private int currentCycle = 0;

    public override void OnStartClient()
    {
        base.OnStartClient();

        // UI 컴포넌트 런타임 연결
        sentencePanel = GameObject.Find("TextCanvas");
        drawingPanel = GameObject.Find("DrawCanvas");
        guessPanel = GameObject.Find("GuessCanvas");

        sentenceInput = GameObject.Find("SentenceInput")?.GetComponent<TMP_InputField>();
        guessInput = GameObject.Find("GuessInput")?.GetComponent<TMP_InputField>();
        guessImage = GameObject.Find("GuessImage")?.GetComponent<RawImage>();
        paint = GameObject.Find("Brush")?.GetComponent<TexturePainter>();

        Debug.Log("[Client] UI 연결 완료");
    }

    // Player.cs에서 호출됨
    public int GetCurrentPlayer() => currentPlayer;

    public void LocalPlayerUpdateUI()
    {
        HideAllPanels();

        if (currentCycle == 0 && sentencePanel != null)
        {
            sentencePanel.SetActive(true);
            if (sentenceInput != null) sentenceInput.text = "";
        }
        else if (IsDrawingTurn() && drawingPanel != null)
        {
            drawingPanel.SetActive(true);
            if (paint != null) paint.ClearTexture();
        }
        else if (IsGuessTurn() && guessPanel != null)
        {
            guessPanel.SetActive(true);
            if (guessInput != null) guessInput.text = "";

            int idx = GetTargetChainIndex();
            if (idx < chains.Count && chains[idx].drawings.Count > 0)
            {
                if (guessImage != null)
                    guessImage.texture = chains[idx].drawings[^1];
            }
        }
    }

    public void HideAllPanels()
    {
        if (sentencePanel != null) sentencePanel.SetActive(false);
        if (drawingPanel != null) drawingPanel.SetActive(false);
        if (guessPanel != null) guessPanel.SetActive(false);
    }

    private void Update()
    {
        if (!isServer) return;

        timeStart += Time.deltaTime;
        if (timeStart >= timeLimit || isClick)
        {
            ForceEndTurn();
        }
    }

    private void ForceEndTurn()
    {
        isClick = false;
        timeStart = 0f;

        if (currentCycle == 0)
        {
            string sentence = sentenceInput != null ? sentenceInput.text : $"Default Sentence {currentPlayer}";
            chains.Add(new TurnChain(sentence, currentPlayer));
        }
        else if (IsDrawingTurn())
        {
            Texture2D drawing = paint.GetTextureCopy();
            int target = GetTargetChainIndex();
            chains[target].drawings.Add(drawing);
        }
        else if (IsGuessTurn())
        {
            int target = GetTargetChainIndex();
            string guess = guessInput != null ? guessInput.text : $"Guess by {currentPlayer}";
            chains[target].guesses.Add(guess);
        }

        AdvanceTurn();
    }

    [Server]
    private void AdvanceTurn()
    {
        currentPlayer++;

        if (currentPlayer >= totalPlayers)
        {
            currentPlayer = 0;
            currentCycle++;

            if (currentCycle >= totalPlayers)
            {
                SceneManager.LoadScene("ResultScene");
                return;
            }
        }

        timeStart = 0f;
    }

    private int GetTargetChainIndex()
    {
        return (currentPlayer - currentCycle + totalPlayers) % totalPlayers;
    }

    private bool IsDrawingTurn() => currentCycle % 2 == 1;
    private bool IsGuessTurn() => currentCycle > 0 && currentCycle % 2 == 0;
}
