using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResultViewer : MonoBehaviour
{
    public TextMeshProUGUI textDisplay;
    public RawImage imageDisplay;
    public Button nextButton;

    private List<string> sentences => TimeManager.Instance?.sentenceList;
    private List<Texture2D> drawings => TimeManager.Instance?.drawingList;

    private int currentIndex = 0;

    private void Start()
    {
        ShowCurrent();

        if (nextButton != null)
            nextButton.onClick.AddListener(Next);
    }

    private void ShowCurrent()
    {
        if (TimeManager.Instance == null) return;

        bool isSentence = currentIndex % 2 == 0;
        int dataIndex = currentIndex / 2;

        if (isSentence && dataIndex < sentences.Count)
        {
            textDisplay.gameObject.SetActive(true);
            imageDisplay.gameObject.SetActive(false);
            textDisplay.text = sentences[dataIndex];
        }
        else if (!isSentence && dataIndex < drawings.Count)
        {
            textDisplay.gameObject.SetActive(false);
            imageDisplay.gameObject.SetActive(true);
            imageDisplay.texture = drawings[dataIndex];
        }
        else
        {
            textDisplay.text = "°á°ú ³¡!";
            nextButton.interactable = false;
        }
    }

    private void Next()
    {
        currentIndex++;
        ShowCurrent();
    }
}