
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

// 생략: TurnChain 클래스는 동일합니다

public class TimeManager : NetworkBehaviour
{
    public float turnTime = 30f;
    public int totalPlayers = 4;

    [SyncVar] public int currentCycle = 0;

    private class TurnState
    {
        public float timer = 0f;
        public bool isSubmitted = false;
    }

    private readonly Dictionary<int, TurnState> playerStates = new();
    public List<TurnChain> chains = new();

    private void Start()
    {
        if (isServer)
        {
            for (int i = 0; i < totalPlayers; i++)
            {
                chains.Add(new TurnChain(i));
                playerStates[i] = new TurnState();
            }
        }
    }

    private void Update()
    {
        if (isServer)
        {
            HandleServerTimer();
        }

        // 클라이언트에서 UI 상태 업데이트 반복 호출
        if (isClient)
        {
            Player player = NetworkClient.connection.identity.GetComponent<Player>();
            if (player != null && IsMyTurn(player.playerIndex))
            {
                int chainIdx = GetChainIndex(player.playerIndex);
                UIManager.Instance.ShowUIForTurn(currentCycle, chainIdx, chains);
            }
        }
    }

    private void HandleServerTimer()
    {
        bool allSubmitted = true;

        foreach (var kvp in playerStates)
        {
            var playerId = kvp.Key;
            var state = kvp.Value;

            if (state.isSubmitted) continue;

            state.timer += Time.deltaTime;

            if (state.timer >= turnTime)
            {
                Debug.Log($"⏱️ [SERVER] Player {playerId} 시간 초과 - 자동 제출");
                AutoSubmit(playerId);
                state.isSubmitted = true;
            }

            allSubmitted = false;
        }

        if (allSubmitted)
        {
            AdvanceCycle();
        }
    }

    [Server]
    private void AdvanceCycle()
    {
        currentCycle++;

        if (currentCycle >= totalPlayers)
        {
            Debug.Log("🎉 [SERVER] 모든 턴 완료 → 결과 씬으로 이동");

            if (Application.CanStreamedLevelBeLoaded("ResultScene"))
            {
                SceneManager.LoadScene("ResultScene");
            }
            else
            {
                Debug.LogWarning("⚠️ ResultScene 씬이 Build Settings에 등록되지 않았습니다.");
            }
            return;
        }

        foreach (var state in playerStates.Values)
        {
            state.timer = 0f;
            state.isSubmitted = false;
        }
    }

    public bool IsMyTurn(int playerIndex)
    {
        return playerStates.ContainsKey(playerIndex) && !playerStates[playerIndex].isSubmitted;
    }

    [Command]
    public void CmdSubmitText(string text, int playerIndex)
    {
        int chainIndex = GetChainIndex(playerIndex);
        if (chainIndex < chains.Count)
        {
            chains[chainIndex].texts.Add(text);
        }
        MarkSubmitted(playerIndex);
    }

    [Command]
    public void CmdSubmitDrawing(byte[] drawingData, int playerIndex)
    {
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(drawingData);

        int chainIndex = GetChainIndex(playerIndex);
        if (chainIndex < chains.Count)
        {
            chains[chainIndex].drawings.Add(tex);
        }
        MarkSubmitted(playerIndex);
    }

    private void MarkSubmitted(int playerIndex)
    {
        if (playerStates.ContainsKey(playerIndex))
        {
            playerStates[playerIndex].isSubmitted = true;
        }
    }

    private void AutoSubmit(int playerIndex)
    {
        if (currentCycle == 0)
        {
            CmdSubmitText("(자동 문장)", playerIndex);
        }
        else if (currentCycle % 2 == 1)
        {
            Texture2D empty = new Texture2D(2, 2);
            CmdSubmitDrawing(empty.EncodeToPNG(), playerIndex);
        }
        else
        {
            CmdSubmitText("(자동 유추)", playerIndex);
        }
    }

    public int GetChainIndex(int playerIndex)
    {
        return (playerIndex - currentCycle + totalPlayers) % totalPlayers;
    }
}

