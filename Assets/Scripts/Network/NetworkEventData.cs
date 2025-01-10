using System.Collections.Generic;

public class NetworkEventData
{
    private GameState _gameState = GameState.None;

    /// <summary> ルーム作成時に発行されるID </summary>
    public string RoomID { get; private set; } = "";
    /// <summary> 同時プレイ可能人数 </summary>
    public int MaxConnectableCount { get; private set; } = 0;
    /// <summary> 自分を含めたプレイヤーのList </summary>
    public List<string> RoomPlayers { get; private set; } = default;
    /// <summary> ゲームプレイ中かどうか（プレイ中の外からの介入を防ぐ） </summary>
    public bool IsGamePlaying { get; private set; } = false;
    public GameState GameState => _gameState;
    /// <summary> 自分の番かどうか </summary>
    public bool IsPlayableTurn => _gameState == GameState.MyTurn;

    public NetworkEventData(int maxConnectableCount)
    {
        MaxConnectableCount = maxConnectableCount;
        RoomPlayers = new();
    }

    public void ChangeState(GameState next)
    {
        if (_gameState == next) { return; }

        _gameState = next;
    }

    public void ReceiveRoomID(string id) => RoomID = id;

    public void AddPlayer(string playerID)
    {
        RoomPlayers.Add(playerID);
    }

    public void RemovePlayer(string playerID)
    {
        if (RoomPlayers == null || RoomPlayers.IndexOf(playerID) < 0) { return; }
        RoomPlayers.Remove(playerID);
    }
}

public enum GameState
{
    None,
    Title,
    MyTurn,
    OthersTurn
}
