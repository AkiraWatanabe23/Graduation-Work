﻿using Network;
using System;
using System.Threading.Tasks;

public class GameTurnController
{
    private int _playTurnIndex = 0;
    private bool _isPlayableTurn = false;

    protected Action OnNextTurn { get; private set; }

    /// <summary> 自分の番かどうか </summary>
    public bool IsPlayableTurn => _isPlayableTurn;

    /// <summary> 初期化処理 </summary>
    public void Initialize(DataContainer container, NetworkModel model)
    {
        OnNextTurn = () =>
        {
            container.NextTurn();
            //自分の番が回ってきたかの判定
            _isPlayableTurn = container.CurrentTurn % 3 == _playTurnIndex;
        };

        model.RegisterEvent(RequestType.ChangeTurn, ChangeTurn);
    }

    private async Task<string> ChangeTurn(string _)
    {
        OnNextTurn?.Invoke();

        await Task.Yield();
        return "Request Success";
    }
}
