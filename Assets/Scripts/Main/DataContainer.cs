using System;
using System.Collections.Generic;
using VTNConnect;
using Random = UnityEngine.Random;

/// <summary> ゲーム全体に関与するデータを保持するクラス </summary>
public class DataContainer
{
    public int LoopCount => _loopCount;
    public int PlayerPoint => _playerPoint;
    public int ItemsPerLevel { get; private set; } = -1;
    public Dictionary<int, BlockData> Blocks { get; private set; } = new Dictionary<int, BlockData>();
    public List<int[]> BlockMapping { get; private set; } = new List<int[]>();
    public int SelectedBlockId
    {
        get => _selectedBlockID;
        set
        {
            int newBlockId = value;
            _selectedBlockID = Blocks.ContainsKey(newBlockId) switch
            {
                true => (Blocks[newBlockId].Height >= BlockMapping.Count - 2) switch
                {
                    true => 0,
                    false => newBlockId,
                },
                false => newBlockId,
            };
            CollapseProbability = Random.Range(0f, 1f);
        }
    }
    public float CollapseProbability { get; private set; } = 1.0f;

    public int CurrentTurn
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
    private int _currentTurnCount = -1;
    /// <summary> プレイヤーが保持している得点 </summary>
    private int _playerPoint = 0;

    private Action _initialize = null;
    private Action _gameFinish = null;

    public DataContainer(int floorLevel, int itemsPerLevel, BlockData blockPrefab)
    {
        ItemsPerLevel = itemsPerLevel;
        BlockMapping.Add(null);    // 「添え字」と「ブロックの高さ」を揃えるため、０番目にnullを追加する

        for (int i = 0, blockId = 1; i < floorLevel * itemsPerLevel; i++, blockId++)
        {
            if (i % itemsPerLevel == 0) BlockMapping.Add(new int[itemsPerLevel]);

            BlockData block = UnityEngine.Object.Instantiate(blockPrefab);
            Blocks.Add(blockId, block);
        }
    }

    #region Register Invoke Events
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

        //以下引数のbool値は勝ち、負けで分ける
        VantanConnect.GameEnd(true, (VC_StatusCode code) =>
        {
            SceneLoader.FadeLoad(SceneName.Title);
        });
    }
    #endregion

    public void NextTurn() => CurrentTurn++;
}
