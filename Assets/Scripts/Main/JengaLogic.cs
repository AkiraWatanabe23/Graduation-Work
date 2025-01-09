using System.Collections.Generic;
using UnityEngine;

public class JengaLogic
{
    /// <summary>IDをキーにしてブロックを保持する辞書型</summary>
    public Dictionary<int, BlockData> Blocks => _blocks;
    /// <summary>ブロックが「何段目」の「何番目」にいるかを記録する</summary>
    public List<int[]> BlockMapping => _blockMapping;

    /// <summary>IDをキーにしてジェンガブロックを保持する辞書型</summary>
    private Dictionary<int, BlockData> _blocks = new Dictionary<int, BlockData>();
    /// <summary>ジェンガブロックがどの位置にあるかを確認するチェックシート</summary>
    private List<int[]> _blockMapping = new List<int[]>();

    private int _oldBlockId = -1;   // 直前に入力されたブロックのIDを保持する

    /// <summary>プレイ中に選択されたブロックのIDが新しく選択されたものか判定する</summary>
    /// <param name="newBlockId">新しく選択されたブロックだった場合、そのブロックのIDを返す</param>
    public bool IsNewBlockSelected(out int newBlockId)
    {
        int selectedBlockId = DataContainer.Instance.SelectedBlockId;

        if (_oldBlockId == selectedBlockId) { newBlockId = -1; return false; }

        _oldBlockId = selectedBlockId;
        newBlockId = selectedBlockId;
        return true;
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
    public bool IsCollapse()
    {
        float collapseRisk = GetCollapseRisk();
        Debug.Log($"倒壊率は、{collapseRisk * 100}%！");
        return DataContainer.Instance.CollapseProbability <= collapseRisk;
    }
}
