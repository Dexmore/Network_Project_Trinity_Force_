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
    [Header("UI")]
    public GameObject sentencePanel;
    public GameObject drawingPanel;
    public GameObject guessPanel;

    public TMP_InputField sentenceInput;
    public TMP_InputField guessInput;
    public RawImage guessImage;
    public TexturePainter paint;

    [Tooltip("게임 중에는 비활성화하고 결과 씬에서만 표시")]
    public TextMeshProUGUI playerInfoText;

    [Header("설정")]
    public int totalPlayers = 4;
    public float timeLimit = 30f;

    private float timeStart = 0f;
    private bool isClick = false;

    public List<TurnChain> chains = new();

    [SyncVar] private int currentPlayer = 0;
    [SyncVar] private int currentCycle = 0;

    // === Called by Player.cs ===
    public int GetCurrentPlayer() => currentPlayer;

    public void LocalPlayerUpdateUI()
    {
        sentencePanel.SetActive(false);
        drawingPanel.SetActive(false);
        guessPanel.SetActive(false);

        if (currentCycle == 0)
        {
            sentencePanel.SetActive(true);
            sentenceInput.text = "";
        }
        else if (IsDrawingTurn())
        {
            drawingPanel.SetActive(true);
            paint.ClearTexture();
        }
        else if (IsGuessTurn())
        {
            guessPanel.SetActive(true);
            guessInput.text = "";

            int idx = GetTargetChainIndex();
            if (idx < chains.Count && chains[idx].drawings.Count > 0)
            {
                guessImage.texture = chains[idx].drawings[chains[idx].drawings.Count - 1];
            }
        }
    }

    public void HideAllPanels()
    {
        sentencePanel.SetActive(false);
        drawingPanel.SetActive(false);
        guessPanel.SetActive(false);
    }

    // === Turn Update Logic ===

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
            string sentence = sentenceInput != null ? sentenceInput.text : $"Default {currentPlayer}";
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
