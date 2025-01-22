using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class JengaLogic
{
    private DataContainer _container = null;

    public void Initialize(DataContainer container)
    {
        _container = container;
        int blockId = 0;
        int height = 0;
        int index = 0;

        // blocksとblockMappingの初期化
        foreach (var block in container.Blocks)
        {
            index = blockId % container.ItemsPerLevel;

            if (index == 0) height++;

            container.BlockMapping[height][index] = ++blockId;

            // blockの初期化
            block.Value.BlockId = blockId;
            block.Value.Height = height;
            block.Value.Stability = index switch
            {
                1 => 0.10f,
                _ => 0.45f,
            };
            block.Value.AssignedIndex = index;
            block.Value.Weight = 1.0f;
            block.Value.Fragment = 3;
        }
        DebugBlockMapping();
    }

    private void DebugBlockMapping()
    {
        StringBuilder builder = new StringBuilder();

        foreach (var listItem in _container.BlockMapping)
        {
            if (listItem == null) continue;

            builder.Append($"{string.Join(' ', listItem)}\n");
        }
        Debug.Log(builder);
    }

    public void UpdateBlockInfo(BlockData from, BlockData to)
    {
        var tmp = _container.BlockMapping[from.Height][from.AssignedIndex];
        _container.BlockMapping[from.Height][from.AssignedIndex] = 0;
        _container.BlockMapping[to.Height][to.AssignedIndex] = tmp;

        _container.Blocks[from.BlockId].Height = to.Height;
        _container.Blocks[from.BlockId].Stability = to.Stability;
        _container.Blocks[from.BlockId].AssignedIndex = to.AssignedIndex;
    }

    /// <summary>ジェンガが崩れてもおかしくない状態かを判定する</summary>
    public bool IsUnstable()
    {
        bool isNotCenterExist = true;   // 検索する段に中央のブロックが存在するか
        int blockCounter = 0;           // 検索する段にブロックがいくつ残っているか

        for (int i = 1; i < _container.BlockMapping.Count - 2; i++)
        {
            // 変数を使いまわすため初期化する
            isNotCenterExist = true;
            blockCounter = 0;

            // １段ずつブロックの有無を確認する
            for (int k = 0; k < _container.BlockMapping[i].Length; k++)
            {
                if (_container.BlockMapping[i][k] != 0)
                {
                    blockCounter++;

                    if (0 < k && k < _container.BlockMapping[i].Length - 1)
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

        for (int i = 0; i < _container.BlockMapping.Count; i++)
        {
            if (_container.BlockMapping[i] == null) continue;

            float cash = 0f;

            for (int k = 0; k < _container.BlockMapping[i].Length; k++)
            {
                int target = _container.BlockMapping[i][k];
                cash += target switch
                {
                    0 => target,
                    _ => _container.Blocks[target].Stability * _container.Blocks[target].Weight,
                };
            }
            sumAllStability += cash * (1.0f - 0.01f * i);
        }
        return 1f - (sumAllStability / (_container.BlockMapping.Count - 1));
    }

    /// <summary>ジェンガを引き抜いたときに倒れる確率を引いたか判定する</summary>
    public bool IsCollapse(float collapseProb)
    {
        float collapseRisk = GetCollapseRisk();
        Debug.Log($"倒壊率は、{collapseRisk * 100}%！");
        return collapseProb <= collapseRisk;
    }
}
