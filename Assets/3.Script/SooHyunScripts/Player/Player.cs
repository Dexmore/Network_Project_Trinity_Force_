using UnityEngine;
using Mirror;

public class Player : NetworkBehaviour
{
    [SyncVar] public int playerIndex;

    private TimeManager tm;
    private int lastCycle = -1;

    private void Update()
    {
        if (!isLocalPlayer) return;

        if (tm == null)
            tm = FindObjectOfType<TimeManager>();

        if (tm == null || !tm.IsMyTurn(playerIndex)) return;

        if (tm.currentCycle != lastCycle)
        {
            UIManager.Instance.ShowUIForTurn(tm.currentCycle, tm.GetChainIndex(playerIndex), tm.chains);
            lastCycle = tm.currentCycle;
        }
    }

    public void SubmitText()
    {
        string text = UIManager.Instance.textInput.text;
        if (!string.IsNullOrWhiteSpace(text))
        {
            tm.CmdSubmitText(text, playerIndex);
        }
    }

    public void SubmitGuess()
    {
        string guess = UIManager.Instance.guessInput.text;
        if (!string.IsNullOrWhiteSpace(guess))
        {
            tm.CmdSubmitText(guess, playerIndex);
        }
    }

    public void SubmitDrawing()
    {
        Texture2D drawing = UIManager.Instance.painter.GetTextureCopy();
        if (drawing != null)
        {
            tm.CmdSubmitDrawing(drawing.EncodeToPNG(), playerIndex);
        }
    }
}
