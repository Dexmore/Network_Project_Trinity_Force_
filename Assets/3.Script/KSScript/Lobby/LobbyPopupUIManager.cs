using UnityEngine;
using TMPro;
using System.Collections;

public class LobbyPopupUIManager : MonoBehaviour
{
    public static LobbyPopupUIManager Instance;

    [SerializeField] private CanvasGroup popupGroup; // NotificationPanel
    [SerializeField] private TMP_Text popupText;     // NotificationText
    [SerializeField] private float showDuration = 2f;

    private Coroutine currentRoutine;

    void Awake()
    {
        Instance = this;
        popupGroup.alpha = 0f;
    }

    public void ShowPopup(string message)
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(PopupRoutine(message));
    }

    private IEnumerator PopupRoutine(string msg)
    {
        popupText.text = msg;
        popupGroup.alpha = 1f;

        yield return new WaitForSeconds(showDuration);

        popupGroup.alpha = 0f;
    }
}
