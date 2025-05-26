using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    public Sprite[] tutorialImages;          // 5개 이미지
    

    public Image tutorialImageDisplay;       // UI 이미지 컴포넌트
    

    public Button nextButton;
    public Button prevButton;

    private int currentIndex = 0;

    void Start()
    {
        ShowSlide(0);
        nextButton.onClick.AddListener(NextSlide);
        prevButton.onClick.AddListener(PrevSlide);
    }

    void ShowSlide(int index)
    {
        tutorialImageDisplay.sprite = tutorialImages[index];
        

        prevButton.interactable = index > 0;
        nextButton.interactable = index < tutorialImages.Length - 1;
    }

    void NextSlide()
    {
        if (currentIndex < tutorialImages.Length - 1)
        {
            currentIndex++;
            ShowSlide(currentIndex);
        }
    }

    void PrevSlide()
    {
        if (currentIndex > 0)
        {
            currentIndex--;
            ShowSlide(currentIndex);
        }
    }
}
