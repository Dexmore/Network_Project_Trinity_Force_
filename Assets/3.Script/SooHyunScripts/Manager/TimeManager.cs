using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

[System.Serializable]
public class TurnChain
{
    public int ownerPlayerIndex;
    public List<string> texts = new();
    public List<Texture2D> drawings = new();

    public TurnChain(int ownerIndex) => ownerPlayerIndex = ownerIndex;
}

public class TimeManager : NetworkBehaviour
{
    public static TimeManager Instance;

    public float turnTime = 30f;
    public int totalPlayers = 4;

    [SyncVar] public int currentCycle = 0;

    private class TurnState { public float timer = 0f; public bool isSubmitted = false; }

    private Dictionary<int, TurnState> playerStates = new();
    public List<TurnChain> chains = new();

    private int connectedPlayers = 0;
    private bool gameStarted = false;

    public override void OnStartServer()
    {
        Instance = this;
    }

    [Server]
    public void RegisterPlayer(int index)
    {
        connectedPlayers++;
        Debug.Log($"[Server] Player {index} joined. Total: {connectedPlayers}");

        if (connectedPlayers == totalPlayers)
        {
            StartGame();
        }
    }

    [Server]
    private void StartGame()
    {
        Debug.Log("✅ All players connected. Starting game.");
        gameStarted = true;

        for (int i = 0; i < totalPlayers; i++)
        {
            chains.Add(new TurnChain(i));
            playerStates[i] = new TurnState();
        }

        RpcUpdateAllClients();
    }

    private void Update()
    {
        if (!isServer || !gameStarted) return;

        foreach (var kvp in playerStates)
        {
            if (!kvp.Value.isSubmitted)
            {
                kvp.Value.timer += Time.deltaTime;
                if (kvp.Value.timer >= turnTime)
                {
                    AutoSubmit(kvp.Key);
                    kvp.Value.isSubmitted = true;
                }
            }
        }

        if (AllSubmitted())
            AdvanceCycle();
    }

    private bool AllSubmitted()
    {
        foreach (var s in playerStates.Values)
            if (!s.isSubmitted) return false;
        return true;
    }

    [Server]
    private void AdvanceCycle()
    {
        currentCycle++;

        if (currentCycle >= 1 + (totalPlayers - 1) * 2)
        {
            SceneManager.LoadScene("ResultScene");
            return;
        }

        foreach (var state in playerStates.Values)
        {
            state.timer = 0f;
            state.isSubmitted = false;
        }

        RpcUpdateAllClients();
    }

    [ClientRpc]
    private void RpcUpdateAllClients()
    {
        if (!NetworkClient.ready || NetworkClient.connection == null) return;

        Player player;
        if (!NetworkClient.connection.identity.TryGetComponent(out player)) return;

        int index = player.playerIndex;

        if (!gameStarted)
        {
            UIManager.Instance?.ShowWaitingCanvas();
            return;
        }

        int chainIndex = GetTargetChainIndex(index);

        if (currentCycle == 0)
        {
            // 시작 문장 입력 단계
            UIManager.Instance?.ShowTextCanvas();
        }
        else if (currentCycle % 2 == 1)
        {
            // 그림 그리기 단계
            UIManager.Instance?.ShowUIForTurn(currentCycle, chainIndex, chains);
        }
        else
        {
            // 추측 문장 단계
            UIManager.Instance?.ShowUIForTurn(currentCycle, chainIndex, chains);
        }
    }

    [Command]
    public void CmdSubmitText(string text, int playerIndex)
    {
        int targetChain = GetTargetChainIndex(playerIndex);
        chains[targetChain].texts.Add(text);
        MarkSubmitted(playerIndex);
    }

    [Command]
    public void CmdSubmitDrawing(byte[] data, int playerIndex)
    {
        int targetChain = GetTargetChainIndex(playerIndex);
        Texture2D tex = new Texture2D(2, 2); tex.LoadImage(data);
        chains[targetChain].drawings.Add(tex);
        MarkSubmitted(playerIndex);
    }

    private void AutoSubmit(int playerIndex)
    {
        if (currentCycle == 0 || currentCycle % 2 == 0)
            CmdSubmitText("(Auto)", playerIndex);
        else
            CmdSubmitDrawing(new Texture2D(2, 2).EncodeToPNG(), playerIndex);
    }

    private void MarkSubmitted(int idx)
    {
        if (playerStates.ContainsKey(idx)) playerStates[idx].isSubmitted = true;
    }

    public int GetTargetChainIndex(int playerIndex)
    {
        return (playerIndex + currentCycle) % totalPlayers;
    }
}
