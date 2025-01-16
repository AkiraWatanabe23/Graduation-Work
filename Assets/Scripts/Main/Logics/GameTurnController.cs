using System;

public class GameTurnController
{
    private GameState _gameState = GameState.None;

    protected Action<int> OnNextTurn { get; private set; }

    public GameState GameState => _gameState;
    /// <summary> 自分の番かどうか </summary>
    public bool IsPlayableTurn => _gameState == GameState.MyTurn;

    /// <summary> 初期化処理 </summary>
    public void Initialize(DataContainer container)
    {
        OnNextTurn = (count) => container.NextTurn(count);
    }

    public void ChangeState(GameState next)
    {
        if (_gameState == next) { return; }

        _gameState = next;
    }
}

public enum GameState
{
    None,
    Title,
    MyTurn,
    OthersTurn
}
