using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResultViewer : MonoBehaviour
{
    public TextMeshProUGUI textDisplay;
    public RawImage imageDisplay;
    public Button nextButton;

    private List<TurnChain> chains;
    private int chainIndex = 0;
    private int stepIndex = 0; // 0: 문장, 1: 그림, 2: 추측, 3: 그림, 4: 추측...

    private void Start()
    {
        TimeManager tm = FindObjectOfType<TimeManager>();
        if (tm == null)
        {
            textDisplay.text = "TimeManager 인스턴스를 찾을 수 없습니다.";
            nextButton.interactable = false;
            return;
        }

        chains = tm.chains;

        if (nextButton != null)
            nextButton.onClick.AddListener(NextStep);

        ShowStep();
    }

    private void ShowStep()
    {
        if (chains == null || chainIndex >= chains.Count)
        {
            textDisplay.text = "모든 결과를 확인했습니다!";
            imageDisplay.gameObject.SetActive(false);
            nextButton.gameObject.SetActive(false);
            return;
        }

        var chain = chains[chainIndex];

        if (stepIndex == 0)
        {
            textDisplay.text = $"[플레이어 {chain.ownerPlayerIndex + 1}의 시작 문장] : {chain.originalSentence}";
            textDisplay.gameObject.SetActive(true);
            imageDisplay.gameObject.SetActive(false);
        }
        else
        {
            int turn = (stepIndex - 1) / 2;
            bool isDrawStep = stepIndex % 2 == 1;

            if (isDrawStep && turn < chain.drawings.Count)
            {
                imageDisplay.texture = chain.drawings[turn];
                imageDisplay.gameObject.SetActive(true);
                textDisplay.gameObject.SetActive(false);
            }
            else if (!isDrawStep && turn < chain.guesses.Count)
            {
                textDisplay.text = $"[플레이어 {(chain.ownerPlayerIndex + 1 + turn + 1) % 4 + 1}의 문장] : {chain.guesses[turn]}";
                textDisplay.gameObject.SetActive(true);
                imageDisplay.gameObject.SetActive(false);
            }
            else
            {
                chainIndex++;
                stepIndex = 0;
                ShowStep();
                return;
            }
        }
    }

    private void NextStep()
    {
        stepIndex++;
        ShowStep();
    }
}
