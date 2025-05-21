using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResultViewer : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI textDisplay;
    public RawImage imageDisplay;
    public Button nextButton;

    private List<TurnChain> chains;
    private int chainIndex = 0;
    private int stepIndex = 0;

    private void Start()
    {
        chains = FindObjectOfType<TimeManager>()?.chains;

        if (chains == null || chains.Count == 0)
        {
            textDisplay.text = "❌ 결과 데이터를 불러올 수 없습니다.";
            nextButton.interactable = false;
            return;
        }

        nextButton.onClick.AddListener(Next);
        ShowStep();
    }

    private void ShowStep()
    {
        textDisplay.gameObject.SetActive(false);
        imageDisplay.gameObject.SetActive(false);

        if (chainIndex >= chains.Count)
        {
            textDisplay.text = "🎉 모든 결과를 확인했습니다!";
            textDisplay.gameObject.SetActive(true);
            nextButton.interactable = false;
            return;
        }

        TurnChain chain = chains[chainIndex];
        titleText.text = $"플레이어 {chain.ownerPlayerIndex + 1}의 체인";

        // 시작 문장
        if (stepIndex == 0)
        {
            textDisplay.text = $"[시작 문장]\n\"{chain.texts[0]}\"";
            textDisplay.gameObject.SetActive(true);
        }
        else
        {
            int idx = (stepIndex - 1) / 2;

            if (stepIndex % 2 == 1) // 그림
            {
                if (idx < chain.drawings.Count)
                {
                    imageDisplay.texture = chain.drawings[idx];
                    imageDisplay.gameObject.SetActive(true);
                }
            }
            else // 문장
            {
                int textIdx = (stepIndex / 2);
                if (textIdx < chain.texts.Count)
                {
                    textDisplay.text = $"[추측 문장]\n\"{chain.texts[textIdx]}\"";
                    textDisplay.gameObject.SetActive(true);
                }
            }
        }
    }

    private void Next()
    {
        TurnChain chain = chains[chainIndex];

        int maxSteps = 1 + Mathf.Max(chain.drawings.Count, chain.texts.Count - 1) * 2;

        stepIndex++;

        if (stepIndex >= maxSteps)
        {
            chainIndex++;
            stepIndex = 0;
        }

        ShowStep();
    }
}
