using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary> 材質周りのショップ処理を管理するクラス </summary>
public class ShopSystemController : SingletonMonoBehaviour<ShopSystemController>
{
    [Serializable]
    public class MaterialHolder
    {
        [field: SerializeField]
        public Button BuyButton { get; private set; }
        [field: SerializeField]
        public Button UseButton { get; private set; }
        [field: SerializeField]
        public Text HoldCountText { get; private set; }
    }

    [ReadOnly]
    [Tooltip("プレイヤーが保持しているブロックの欠片の数")]
    [SerializeField]
    private int _blockFragmentCount = 0;

    [Header("=== UI ===")]
    [SerializeField]
    private MaterialHolder[] _holders = default;

    protected override bool DontDestroyOnLoad => false;

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {


        foreach (var holder in _holders)
        {
            holder.BuyButton.onClick.AddListener(() =>
            {
                //todo : ここに購入、減算処理を記述する
            });
        }

        _blockFragmentCount = 0;
    }

    /// <summary> 引き抜いたブロックに応じて欠片の数を加算する </summary>
    public void UpdateFragmentCount(int count)
    {
        _blockFragmentCount += count;
    }
}
