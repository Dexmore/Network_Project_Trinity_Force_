using UnityEngine;
using UnityEngine.UI;
using Mirror;

public enum SubmitType { Text, Guess, Drawing }

public class SubmitButton : MonoBehaviour
{
    public SubmitType submitType;

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnSubmitClicked);
    }

    private void OnSubmitClicked()
    {
        if (!NetworkClient.ready || NetworkClient.connection == null)
        {
            Debug.LogWarning("[SubmitButton] Network client not ready or connection null.");
            return;
        }

        Player player = NetworkClient.connection.identity.GetComponent<Player>();
        if (player == null)
        {
            Debug.LogError("[SubmitButton] Player component not found on client connection.");
            return;
        }

        switch (submitType)
        {
            case SubmitType.Text:
                player.CmdSubmitTextToServer(UIManager.Instance.textInput.text);
                break;
            case SubmitType.Guess:
                player.CmdSubmitTextToServer(UIManager.Instance.guessInput.text);
                break;
            case SubmitType.Drawing:
                player.CmdSubmitDrawingToServer(UIManager.Instance.painter.GetPNG());
                break;
        }

        UIManager.Instance.ShowWaitingCanvas();
    }
}
