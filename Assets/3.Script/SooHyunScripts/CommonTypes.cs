using System.Collections.Generic;
public struct PlayerResultData
{
    public string playerName;
    public string sentence;
    public List<byte> drawing1;
    public string guess;
    public List<byte> drawing2;
}

public class PlayerResult
{
    public string playerName;
    public string sentence;
    public List<byte> drawing1;
    public string guess;
    public List<byte> drawing2;
}

public class GameTurn
{
    public string playerName;
    public string sentence;
    public List<byte> drawing;
    public string guess;
    public string ownerName;
}