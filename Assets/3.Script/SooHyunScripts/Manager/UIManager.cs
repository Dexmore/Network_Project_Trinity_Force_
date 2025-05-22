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
    public GameObject waitingCanvas;

    public TMP_InputField textInput;
    public TMP_InputField guessInput;
    public RawImage guessImage;
    public TexturePainter painter;
    public TextMeshProUGUI referenceText;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    public void ShowTextCanvas()
    {
        HideAll();
        textCanvas.SetActive(true);
        if (textInput != null)
            textInput.text = "";
    }

    public void ShowUIForTurn(int cycle, int chainIndex, List<TurnChain> chains)
    {
        if (cycle == 0)
        {
            Debug.LogWarning("ShowUIForTurn was called during cycle 0, falling back to ShowTextCanvas()");
            ShowTextCanvas();
            return;
        }

        HideAll();
        if (cycle % 2 == 1)
        {
            drawCanvas.SetActive(true);
            painter?.ClearTexture();
            referenceText.text = (chainIndex < chains.Count && chains[chainIndex].texts.Count > 0)
                ? chains[chainIndex].texts[^1] : "(no text)";
        }
        else
        {
            guessCanvas.SetActive(true);
            guessInput.text = "";
            guessImage.texture = (chainIndex < chains.Count && chains[chainIndex].drawings.Count > 0)
                ? chains[chainIndex].drawings[^1] : null;
        }
    }

    public void ShowWaitingCanvas()
    {
        HideAll();
        waitingCanvas.SetActive(true);
    }

    public void HideAll()
    {
        textCanvas?.SetActive(false);
        drawCanvas?.SetActive(false);
        guessCanvas?.SetActive(false);
        waitingCanvas?.SetActive(false);
    }
}