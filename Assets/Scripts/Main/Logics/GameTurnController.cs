﻿using Network;
using System;
using System.Threading.Tasks;

public class GameTurnController
{
    private GameState _gameState = GameState.None;

    protected Action<int> OnNextTurn { get; private set; }

    public GameState GameState => _gameState;
    /// <summary> 自分の番かどうか </summary>
    public bool IsPlayableTurn => _gameState == GameState.MyTurn;

    /// <summary> 初期化処理 </summary>
    public void Initialize(DataContainer container, NetworkModel model)
    {
        OnNextTurn = (count) => container.NextTurn(count);

        model.RegisterEvent(RequestType.ChangeTurn, ChangeTurn);
    }

    public void ChangeState(GameState next)
    {
        if (_gameState == next) { return; }

        _gameState = next;
    }

    private async Task<string> ChangeTurn(string key)
    {
        await Task.Yield();
        return "";
    }
}

public enum GameState
{
    None,
    Title,
    MyTurn,
    OthersTurn
}
