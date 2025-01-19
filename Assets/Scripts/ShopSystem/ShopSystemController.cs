using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Debug = Constants.ConsoleLogs;

/// <summary> 材質周りのショップ処理を管理するクラス </summary>
public class ShopSystemController : SingletonMonoBehaviour<ShopSystemController>
{
    [Serializable]
    public class MaterialHolder
    {
        [Tooltip("対象の材質")]
        [SerializeField]
        private MaterialType _materialType = MaterialType.None;
        [Tooltip("購入に必要な欠片の数")]
        [SerializeField]
        private int _requiredFragmentCount = 0;
        [ReadOnly]
        [Tooltip("現在保持している対象の材質の数")]
        [SerializeField]
        private int _holdCount = 0;
        [SerializeField]
        private Text _holdCountText = default;

        [field: SerializeField]
        public GameObject BackImage { get; private set; }
        [field: SerializeField]
        public Button PurchaseButton { get; private set; }
        [field: SerializeField]
        public Button UseButton { get; private set; }

        public MaterialType MaterialType => _materialType;
        public int RequiredFragmentCount => _requiredFragmentCount;
        public int HoldCount
        {
            get => _holdCount;
            set
            {
                _holdCount = value;
                _holdCountText.text = $"x {_holdCount:00}";
            }
        }
    }

    [SerializeField]
    private MaterialInputHandler _inputHandler = default;
    [Tooltip("プレイヤーが保持しているブロックの欠片の数")]
    [SerializeField]
    private int _blockFragmentCount = 0;

    [Header("=== UI ===")]
    [SerializeField]
    private Button _shopPageButton = default;
    [SerializeField]
    private Image _arrowImage = default;
    [SerializeField]
    private Sprite[] _arrows = new Sprite[2];
    [SerializeField]
    private Image _rootImage = default;
    [SerializeField]
    private Text _currentFragmentCountText = default;
    [SerializeField]
    private MaterialHolder[] _holders = default;

    private bool _isOpenShop = false;
    private Dictionary<MaterialType, MaterialHolder> _materialPurchaseDict = default;

    protected int BlockFragmentCount
    {
        get => _blockFragmentCount;
        private set
        {
            _blockFragmentCount = value;
            _currentFragmentCountText.text = $"x {_blockFragmentCount:00}";
        }
    }

    protected override bool DontDestroyOnLoad => false;

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (_inputHandler == null) { _inputHandler = FindObjectOfType<MaterialInputHandler>(); }

        _materialPurchaseDict = new();

        _shopPageButton.onClick.AddListener(() =>
        {
            if (!_isOpenShop)
            {
                _isOpenShop = true;
                _rootImage.rectTransform.DOLocalMoveX(-640f, 1f).OnComplete(() => _arrowImage.sprite = _arrows[1]);
            }
            else
            {
                _isOpenShop = false;
                _rootImage.rectTransform.DOLocalMoveX(-1280f, 1f).OnComplete(() => _arrowImage.sprite = _arrows[0]);
            }
        });

        foreach (var holder in _holders)
        {
            _materialPurchaseDict.Add(holder.MaterialType, holder);

            holder.PurchaseButton.onClick.AddListener(() => Shopping(holder.MaterialType));
            holder.UseButton.onClick.AddListener(() =>
            {
                if (!GameLogicSupervisor.Instance.IsPlayableTurn) { return; }

                if (holder.HoldCount <= 0)
                {
                    Debug.Log("対象のカードがありません");
                    return;
                }
                _inputHandler.TargetSetting(holder.MaterialType);
                holder.BackImage.SetActive(true);
            });

            holder.HoldCount = 0;
            holder.BackImage.SetActive(false);
        }
        _currentFragmentCountText.text = $"x {_blockFragmentCount:00}";

        _inputHandler.OnChangeMaterial = (material) =>
        {
            var target = _materialPurchaseDict[material];

            target.HoldCount--;
            target.BackImage.SetActive(false);
        };
        _inputHandler.OnCancelSelect = (material) =>
        {
            _materialPurchaseDict[material].BackImage.SetActive(false);
        };
    }

    /// <summary> 購入、欠片の減算処理 </summary>
    /// <param name="target"> 購入対象の材質 </param>
    private void Shopping(MaterialType target)
    {
        if (_blockFragmentCount < _materialPurchaseDict[target].RequiredFragmentCount)
        {
            Debug.Log("欠片の数が必要数に達していません");
            return;
        }

        BlockFragmentCount -= _materialPurchaseDict[target].RequiredFragmentCount;
        _materialPurchaseDict[target].HoldCount++;

    }

    /// <summary> 引き抜いたブロックに応じて欠片の数を加算する </summary>
    public void UpdateFragmentCount(int count)
    {
        BlockFragmentCount += count;
    }

    private void OnDestroy()
    {
        _inputHandler.OnChangeMaterial = null;
        _inputHandler.OnCancelSelect = null;
    }
}
