using System;
using System.Collections.Generic;
using UnityEngine;

public class JengaManager : MonoBehaviour
{
    [SerializeField, Tooltip("生成するジェンガ")]
    private BlockData _blockPrefab = null;
    [SerializeField, Tooltip("何段、ジェンガを生成するか")]
    private int _floorLevel = 10;
    [SerializeField, Tooltip("1段当たりのジェンガの個数")]
    private int _itemsPerLevel = 3;
    [SerializeField]
    private MaterialController _mateCtrler = new MaterialController();

    private Vector3 _updatePos = Vector3.zero;
    private Quaternion _updateRot = Quaternion.identity;
    private int _blockIndexCounter = 0;
    private int _placeCount = 0;
    private bool _isGameFinish = false;

    /// <summary>IDをキーにしてジェンガブロックを保持する辞書型</summary>
    private Dictionary<int, BlockData> _blocks = new Dictionary<int, BlockData>();
    /// <summary>ジェンガブロックがどの位置にあるかを確認するチェックシート</summary>
    private List<int[]> _blockExistsChecker = new List<int[]>();

    private void Start()
    {
        if (_blockPrefab == null) throw new NullReferenceException($"Prefab is not found");

        BuildUp();
        InitBlockExistsChecker();
        _mateCtrler.Initialize();
    }

    private void Update()
    {
        if (IsNewBlockSelected(out int targetId))
        {
            BlockData target = _blocks[targetId];
            int tmp = _blockExistsChecker[target.Height][target.AssignedIndex];
            _blockExistsChecker[target.Height][target.AssignedIndex] = 0;
            Place(targetId);
            UpdateBlockExistsChecker(targetId);
            _blockExistsChecker[target.Height][target.AssignedIndex] = tmp;

            if (IsJengaUnstable() || IsJengaCollapse())
            {
                GameFinish();
            }
        }
    }

    /// <summary>ジェンガを指定された階層の分だけ組み立てる</summary>
    private void BuildUp()
    {
        BlockData jenga = null;
        GameObject blockParent = new GameObject("Blocks");

        for (int i = 1; i <= _floorLevel * _itemsPerLevel; i++)
        {
            jenga = Instantiate(_blockPrefab, blockParent.transform);
            jenga.BlockId = i;
            jenga.AssignedIndex = AssignedIndexCounter();
            _blocks.Add(i, jenga);
            Place(jenga.BlockId);
        }
    }

    /// <summary>ジェンガを配置する</summary>
    /// <param name="target">配置する対象のブロック</param>
    private void Place(int targetBlockId)
    {
        BlockData target = _blocks[targetBlockId];

        //ブロックの座標の変更先を更新する
        if (_placeCount == 0) { _updatePos.x -= _itemsPerLevel / 2; }
        else if (_placeCount < _itemsPerLevel) { _updatePos.x++; }
        else if (_placeCount == _itemsPerLevel) { _updatePos.z -= _itemsPerLevel / 2; }
        else if (_placeCount < _itemsPerLevel * 2) { _updatePos.z++; }

        //ブロックを１段ごとに互い違いとなるよう、向きを 90°回転させる
        if (_placeCount % _itemsPerLevel == 0)
            _updateRot = Quaternion.AngleAxis(90.0f * _updatePos.y, Vector3.up);

        //ブロックの座標・回転を更新
        target.transform.position = _updatePos;
        target.transform.rotation = _updateRot;
        _placeCount++;

        if (_placeCount % _itemsPerLevel == 0) _updatePos.Set(0.0f, ++_updatePos.y, 0.0f);
        if (_placeCount % (_itemsPerLevel * 2) == 0) _placeCount = 0;
    }

    private int _oldBlockId = -1;

    /// <summary>プレイ中に選択されたブロックのIDが新しく選択されたものかどうか</summary>
    /// <param name="newBlockId">新しく選択されたブロックだった場合、そのブロックのIDを返す</param>
    /// <returns>新しく選択されたブロックかどうかの真偽値を返す</returns>
    private bool IsNewBlockSelected(out int newBlockId)
    {
        int selectedBlockId = DataContainer.Instance.SelectedBlockId;

        if (_oldBlockId == selectedBlockId) { newBlockId = -1; return false; }

        _oldBlockId = selectedBlockId;
        newBlockId = selectedBlockId;
        return true;
    }

    /// <summary>ジェンガブロックがどこにあるかを記録するチェックシートの初期化</summary>
    private void InitBlockExistsChecker()
    {
        for (int i = 0, blockId = 1; i <= _floorLevel; i++)
        {
            int[] blockCheckItem = null;

            if (0 < i)
            {
                blockCheckItem = new int[_itemsPerLevel];

                for (int k = 0; k < _itemsPerLevel; k++)
                {
                    blockCheckItem[k] = blockId++;
                }
            }
            _blockExistsChecker.Add(blockCheckItem);
        }
    }

    /// <summary>ジェンガブロックがどこにあるかを記録するチェックシートの更新</summary>
    /// <param name="targetBlockId">対象のジェンガブロックのID</param>
    private void UpdateBlockExistsChecker(int targetBlockId)
    {
        if (_blockExistsChecker.Count <= _blocks[targetBlockId].Height)
        {
            int[] blockCheckItem = new int[_itemsPerLevel];
            Array.Fill(blockCheckItem, 0);
            _blockExistsChecker.Add(blockCheckItem);
        }
        _blocks[targetBlockId].AssignedIndex = AssignedIndexCounter();
    }

    //private void DebugBlockExistChecker()
    //{
    //    StringBuilder debugMessage = new StringBuilder();

    //    foreach (var listItem in _blockExistsChecker)
    //    {
    //        if (listItem == null) continue;

    //        for (int k = 0; k < listItem.Length; k++)
    //        {
    //            debugMessage.Append(listItem[k]);
    //        }
    //        debugMessage.Append('\n');
    //    }
    //    Debug.Log(debugMessage);
    //}

    /// <summary>ブロックに与える添え字をカウントする</summary>
    private int AssignedIndexCounter()
    {
        if (_blockIndexCounter % _itemsPerLevel == 0)
        {
            _blockIndexCounter = 0;
        }
        return _blockIndexCounter++;
    }

    /// <summary>ジェンガが崩れてもおかしくない状態かを判定する</summary>
    private bool IsJengaUnstable()
    {
        bool isNotCenterExist = true;   // 検索する段に中央のブロックが存在するか
        int blockCounter = 0;           // 検索する段にブロックがいくつ残っているか

        foreach (var listItem in _blockExistsChecker)
        {
            if (listItem == null) continue;

            // 変数を使いまわすため初期化する
            isNotCenterExist = true;
            blockCounter = 0;

            // １段ずつブロックの有無を確認する
            for (int k = 0; k < listItem.Length; k++)
            {
                if (listItem[k] != 0)
                {
                    blockCounter++;

                    if (0 < k && k < listItem.Length - 1)
                        isNotCenterExist = false;
                }
            }

            // １段に「中央のブロックがない」&&「残りブロックが１つだけ」の時は倒壊する
            if (isNotCenterExist && blockCounter < 2) return true;
        }
        return false;
    }

    /// <summary>ブロックを引き抜いたとき、ジェンガが崩壊する確率を計算する</summary>
    private float GetCollapseRisk()
    {
        float sumAllStability = 0f;

        for (int i = 0; i < _blockExistsChecker.Count; i++)
        {
            if (_blockExistsChecker[i] == null) continue;

            float cash = 0f;

            for (int k = 0; k < _blockExistsChecker[i].Length; k++)
            {
                int target = _blockExistsChecker[i][k];
                cash += target switch
                {
                    0 => target,
                    _ => _blocks[target].Stability * _blocks[target].Weight,
                };
            }
            sumAllStability += cash * (1.0f - 0.01f * i);
        }
        return 1f - (sumAllStability / (_blockExistsChecker.Count - 1));
    }

    /// <summary>ジェンガを引き抜いたときに倒れる確率を引いたか判定する</summary>
    private bool IsJengaCollapse()
    {
        float collapseRisk = GetCollapseRisk();
        Debug.Log($"倒壊率は、{collapseRisk * 100}%！");
        return DataContainer.Instance.CollapseProbability <= collapseRisk;
    }

    private void GameFinish()
    {
        DataContainer.Instance.IsGameFinish = true;

        foreach (var block in _blocks)
        {
            block.Value.gameObject.AddComponent<Rigidbody>();
        }
        Debug.Log("BREAK！");
    }
}
