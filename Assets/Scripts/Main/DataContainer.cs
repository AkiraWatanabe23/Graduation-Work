using System;
using Random = UnityEngine.Random;

/// <summary> ゲーム全体に関与するデータを保持するクラス </summary>
public class DataContainer
{
    public int LoopCount => _loopCount;
    public int PlayerPoint => _playerPoint;

    public int SelectedBlockId
    {
        get => _selectedBlockID;
        set
        {
            _selectedBlockID = value;
            CollapseProbability = Random.Range(0f, 1f);
        }
    }
    public float CollapseProbability { get; private set; } = 1.0f;

    protected int CurrentTurn
    {
        get => _currentTurnCount;
        private set
        {
            _currentTurnCount = value;
            if (_currentTurnCount % 3 == 0) { _loopCount++; }
        }
    }

    private int _selectedBlockID = -1;
    /// <summary> 各プレイヤーの手番が何周したか </summary>
    private int _loopCount = 0;
    /// <summary> 現在の合計ターン数 </summary>
    private int _currentTurnCount = 0;
    /// <summary> プレイヤーが保持している得点 </summary>
    private int _playerPoint = 0;

    private Action _initialize = null;
    private Action _gameFinish = null;

    public void InitializeRegister(params Action[] actions)
    {
        foreach (var action in actions)
        {
            if (action == null) continue;
            _initialize += action;
        }
    }

    public void InitializeInvoke()
    {
        _initialize.Invoke();
        _initialize = null;
    }

    public void GameFinishRegister(params Action[] actions)
    {
        foreach (var action in actions)
        {
            if (action == null) continue;
            _gameFinish += action;
        }
    }

    public void GameFinishInvoke()
    {
        _gameFinish.Invoke();
        _gameFinish = null;
    }
}
