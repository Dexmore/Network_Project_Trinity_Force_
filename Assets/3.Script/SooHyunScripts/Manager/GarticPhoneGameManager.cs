// GarticPhoneGameManager.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.UI;

public class GarticPhoneGameManager : NetworkBehaviour
{
    public static GarticPhoneGameManager Instance;

    [SerializeField] private GameObject textCanvas;
    [SerializeField] private GameObject drawCanvas;
    [SerializeField] private GameObject guessCanvas;
    [SerializeField] private GameObject waitingCanvas;

    [SerializeField] private TMP_InputField textInput;
    [SerializeField] private TMP_InputField guessInput;

    [SerializeField] private TextMeshProUGUI referenceText;
    [SerializeField] private RawImage referenceImage;

    public TexturePainter painter;

    private const int playerCount = 4;

    [SyncVar] private int textSubmitCount = 0;
    [SyncVar] private int drawSubmitCount = 0;
    [SyncVar] private int guessSubmitCount = 0;

    private List<string> submittedTexts = new List<string>();
    private List<Texture2D> submittedDrawings = new List<Texture2D>();
    private List<string> finalGuesses = new List<string>();

    private Dictionary<int, NetworkConnectionToClient> playerConnections = new();
    private bool gameStarted = false;
    private int nextPlayerIndex = 0;
    private int connectedPlayers = 0;

    public override void OnStartServer()
    {
        Instance = this;
    }

    public override void OnStartClient()
    {
        if (!isLocalPlayer) return;
        ShowCanvas(textCanvas);
    }

    [Server]
    public void RegisterPlayer(int index)
    {
        connectedPlayers++;
        Debug.Log($"[Server] Player {index} joined. Total: {connectedPlayers}");

        if (connectedPlayers == playerCount)
        {
            StartGame();
        }
    }

    [Server]
    private void StartGame()
    {
        gameStarted = true;
        RpcStartTextPhase();
    }

    public int RegisterPlayer(NetworkConnectionToClient conn)
    {
        playerConnections[nextPlayerIndex] = conn;
        int assignedIndex = nextPlayerIndex;
        nextPlayerIndex++;
        RegisterPlayer(assignedIndex);
        return assignedIndex;
    }

    [ClientRpc]
    void RpcStartTextPhase()
    {
        ShowCanvas(textCanvas);
    }

    [Command(requiresAuthority = false)]
    public void CmdSubmitText(string sentence)
    {
        submittedTexts.Add(sentence);
        textSubmitCount++;
        if (textSubmitCount == playerCount)
        {
            RpcStartDrawPhase(GetTextTargets());
        }
    }

    [ClientRpc]
    void RpcStartDrawPhase(List<string> textsToDraw)
    {
        ShowCanvas(drawCanvas);
        int myIndex = GetPlayerIndex();
        referenceText.text = textsToDraw[myIndex];
    }

    public void OnSubmitDrawingButtonPressed()
    {
        Texture2D drawnTexture = painter.GetTextureCopy();
        byte[] imageData = drawnTexture.EncodeToPNG();
        CmdSubmitDrawing(imageData);
    }

    [Command(requiresAuthority = false)]
    public void CmdSubmitDrawing(byte[] imageData)
    {
        Texture2D tex = new Texture2D(512, 512);
        tex.LoadImage(imageData);
        submittedDrawings.Add(tex);
        drawSubmitCount++;
        if (drawSubmitCount == playerCount)
        {
            RpcStartGuessPhase(GetDrawingTargets());
        }
    }

    [ClientRpc]
    void RpcStartGuessPhase(List<Texture2D> drawingsToGuess)
    {
        ShowCanvas(guessCanvas);
        int myIndex = GetPlayerIndex();
        referenceImage.texture = drawingsToGuess[myIndex];
    }

    [Command(requiresAuthority = false)]
    public void CmdSubmitGuess(string guess)
    {
        finalGuesses.Add(guess);
        guessSubmitCount++;
        if (guessSubmitCount == playerCount)
        {
            RpcCheckEndGame();
        }
    }

    [ClientRpc]
    void RpcCheckEndGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("EndScene");
    }

    void ShowCanvas(GameObject go)
    {
        textCanvas.SetActive(false);
        drawCanvas.SetActive(false);
        guessCanvas.SetActive(false);
        waitingCanvas.SetActive(false);
        go.SetActive(true);
    }

    int GetPlayerIndex()
    {
        if (NetworkClient.connection?.identity == null) return 0;

        var player = NetworkClient.connection.identity.GetComponent<PlayerController>();
        return player != null ? player.playerIndex : 0;
    }

    List<string> GetTextTargets()
    {
        var result = new List<string>();
        for (int i = 0; i < playerCount; i++)
        {
            int targetIndex = (i + 1) % playerCount;
            result.Add(submittedTexts[targetIndex]);
        }
        return result;
    }

    List<Texture2D> GetDrawingTargets()
    {
        var result = new List<Texture2D>();
        for (int i = 0; i < playerCount; i++)
        {
            int targetIndex = (i + 2) % playerCount;
            result.Add(submittedDrawings[targetIndex]);
        }
        return result;
    }
}