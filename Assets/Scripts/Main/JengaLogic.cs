﻿using System.Collections.Generic;
using UnityEngine;

public class JengaLogic
{
    /// <summary>ジェンガが崩れてもおかしくない状態かを判定する</summary>
    public bool IsUnstable(List<int[]> blockMapping)
    {
        bool isNotCenterExist = true;   // 検索する段に中央のブロックが存在するか
        int blockCounter = 0;           // 検索する段にブロックがいくつ残っているか

        foreach (var listItem in blockMapping)
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
    private float GetCollapseRisk(Dictionary<int, BlockData> blocks, List<int[]> blockMapping)
    {
        float sumAllStability = 0f;

        for (int i = 0; i < blockMapping.Count; i++)
        {
            if (blockMapping[i] == null) continue;

            float cash = 0f;

            for (int k = 0; k < blockMapping[i].Length; k++)
            {
                int target = blockMapping[i][k];
                cash += target switch
                {
                    0 => target,
                    _ => blocks[target].Stability * blocks[target].Weight,
                };
            }
            sumAllStability += cash * (1.0f - 0.01f * i);
        }
        return 1f - (sumAllStability / (blockMapping.Count - 1));
    }

    /// <summary>ジェンガを引き抜いたときに倒れる確率を引いたか判定する</summary>
    public bool IsCollapse(Dictionary<int, BlockData> blocks, List<int[]> blockMapping, float collapseProb)
    {
        float collapseRisk = GetCollapseRisk(blocks, blockMapping);
        Debug.Log($"倒壊率は、{collapseRisk * 100}%！");
        return collapseProb <= collapseRisk;
    }
}
