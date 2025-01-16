using System.Collections.Generic;
using UnityEngine;

public class JengaLogic
{
    /// <summary>IDをキーにしてブロックを保持する</summary>
    public Dictionary<int, BlockData> Blocks => _blocks;

    ///// <summary>IDをキーにしてブロックを保持する</summary>
    private Dictionary<int, BlockData> _blocks = new Dictionary<int, BlockData>();
    /// <summary>ブロックがどの位置にあるかを保持する</summary>
    private List<int[]> _blockMapping = new List<int[]>();

    /// <param name="floorLevel">ジェンガが何段あるか</param>
    /// <param name="itemsPerLevel">1段当たりブロックはいくつか</param>
    public void Initialize(int floorLevel, int itemsPerLevel, in BlockData blockPrefab, Transform blockParent = null)
    {
        _blockMapping.Add(null);    // 「添え字」と「ジェンガの高さ」を揃えるため、０番目にnullを追加する
        int blockId = 1;
        int height = 0;
        int index = 0;

        // blocksとblockMappingの初期化
        for (int i = 0; i < floorLevel * itemsPerLevel; i++, blockId++)
        {
            index = i % itemsPerLevel;

            if (index == 0)
            {
                _blockMapping.Add(new int[itemsPerLevel]);
                height++;
            }
            
            BlockData block = Object.Instantiate(blockPrefab, blockParent);
            _blocks.Add(blockId, block);
            _blockMapping[height][index] = blockId;

            // blockの初期化
            float stability = index switch
            {
                1 => 0.10f,
                _ => 0.45f,
            };
            block.UpdateData(blockId, height, stability, index, 1);
        }

        foreach (var listItem in _blockMapping)
        {
            if (listItem == null) continue;
            Debug.Log(string.Join(' ', listItem));
        }
    }

    /// <summary>ジェンガが崩れてもおかしくない状態かを判定する</summary>
    public bool IsUnstable()
    {
        bool isNotCenterExist = true;   // 検索する段に中央のブロックが存在するか
        int blockCounter = 0;           // 検索する段にブロックがいくつ残っているか

        foreach (var listItem in _blockMapping)
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

            // 「中央のブロックがない」「残りブロックが１つだけ」の段がある時は倒壊する
            if (isNotCenterExist && blockCounter < 2) return true;
        }
        return false;
    }

    /// <summary>ブロックを引き抜いたとき、ジェンガが崩壊する確率を計算する</summary>
    private float GetCollapseRisk()
    {
        float sumAllStability = 0f;

        for (int i = 0; i < _blockMapping.Count; i++)
        {
            if (_blockMapping[i] == null) continue;

            float cash = 0f;

            for (int k = 0; k < _blockMapping[i].Length; k++)
            {
                int target = _blockMapping[i][k];
                cash += target switch
                {
                    0 => target,
                    _ => _blocks[target].Stability * _blocks[target].Weight,
                };
            }
            sumAllStability += cash * (1.0f - 0.01f * i);
        }
        return 1f - (sumAllStability / (_blockMapping.Count - 1));
    }

    /// <summary>ジェンガを引き抜いたときに倒れる確率を引いたか判定する</summary>
    public bool IsCollapse(float collapseProb)
    {
        float collapseRisk = GetCollapseRisk();
        Debug.Log($"倒壊率は、{collapseRisk * 100}%！");
        return collapseProb <= collapseRisk;
    }
}
