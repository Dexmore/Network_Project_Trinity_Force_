using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public GameObject textCanvas;
    public GameObject drawCanvas;
    public GameObject guessCanvas;

    public TMP_InputField textInput;
    public TMP_InputField guessInput;
    public RawImage guessImage;

    public TexturePainter painter;
    public TextMeshProUGUI referenceText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void ShowUIForTurn(int cycle, int chainIndex, List<TurnChain> chains)
    {
        bool showText = false;
        bool showDraw = false;
        bool showGuess = false;

        if (cycle == 0)
        {
            showText = true;
            textInput.text = "";
        }
        else if (cycle % 2 == 1)
        {
            showDraw = true;
            painter.ClearTexture();
            referenceText.text = (chainIndex < chains.Count && chains[chainIndex].texts.Count > 0)
                ? chains[chainIndex].texts[^1]
                : "(no text)";
        }
        else
        {
            showGuess = true;
            guessInput.text = "";
            guessImage.texture = (chainIndex < chains.Count && chains[chainIndex].drawings.Count > 0)
                ? chains[chainIndex].drawings[^1]
                : null;
        }

        textCanvas.SetActive(showText);
        drawCanvas.SetActive(showDraw);
        guessCanvas.SetActive(showGuess);
    }
}
