using Mirror;
using System.Collections.Generic;

public struct GameStartMsg : NetworkMessage { }
public struct ProceedToNextPhaseMsg : NetworkMessage { }
public struct GameResultMsg : NetworkMessage
{
    public List<PlayerResultData> results;
}
public struct PlayerResultData
{
    public string playerName;
    public string sentence;
    public byte[] drawing1;
    public string guess;
    public byte[] drawing2;
}
