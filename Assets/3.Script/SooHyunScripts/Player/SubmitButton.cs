using UnityEngine;

public class SubmitButton : MonoBehaviour
{
    public enum SubmitType { Text, Guess, Drawing }
    public SubmitType submitType;

    public void OnClick()
    {
        Player player = FindObjectOfType<Player>();
        if (player == null) return;

        switch (submitType)
        {
            case SubmitType.Text:
                player.SubmitText();
                break;
            case SubmitType.Guess:
                player.SubmitGuess();
                break;
            case SubmitType.Drawing:
                player.SubmitDrawing();
                break;
        }
    }
}
