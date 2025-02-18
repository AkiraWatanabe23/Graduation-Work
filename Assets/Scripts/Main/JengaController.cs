using Cysharp.Threading.Tasks;
using DG.Tweening;
using Extention;
using Network;
using System;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class JengaController
{
    public BlockData BlockPrefab => _blockPrefab;

    [SerializeField, Tooltip("生成するジェンガ")]
    private BlockData _blockPrefab = null;
    [SerializeField, Tooltip("ジェンガの底面かつ中心の位置となる座標")]
    private Transform _generateBottomPos = null;
    [SerializeField, Tooltip("ジェンガが倒壊するときにかける力")]
    private float _corrapsePower = 1.0f;

    private JengaLogic _logic = new();
    private DataContainer _container = null;

    private Vector3 _destination = Vector3.zero;
    private Quaternion _rotation = Quaternion.identity;
    private Vector3 _moveDir = Vector3.zero;
    private Vector3 _blockSize = Vector3.zero;
    private GameObject _blockParent = null;
    private int _alreadySelectedId = 0;

    private BlockData[] _selectBoxes = null;
    private Vector3[] _raycastPositions = null;

    private Action<int, int> _onPlace = null;

    private Action _onGameFinish = null;

    public void Initialize(DataContainer container, NetworkPresenter presenter)
    {
        _blockSize = _blockPrefab.transform.localScale;
        _blockParent = new GameObject("Block Parent");
        _container = container;
        _logic.Initialize(container);
        InitSelectBoxes();
        BuildUp();

        _raycastPositions = new Vector3[]
        {
            Vector3.forward * _blockSize.x * (_container.ItemsPerLevel / 2),
            Vector3.back    * _blockSize.x * (_container.ItemsPerLevel / 2),
            Vector3.left    * _blockSize.x * (_container.ItemsPerLevel / 2),
            Vector3.right   * _blockSize.x * (_container.ItemsPerLevel / 2),
        };

        presenter.Model.RegisterEvent(RequestType.SelectBlock, BlockSelected);
        presenter.Model.RegisterEvent(RequestType.PlaceBlock, PlaceBlock);
        _onPlace = async (id, next) =>
        {
            await presenter.SendPutRequest(RequestType.PlaceBlock, id.ToString(), next.ToString());

            if (GameLogicSupervisor.Instance.IsGameFinish) { return; }

            if (GameLogicSupervisor.Instance.IsPlayableTurn)
            {
                presenter.Model.RequestEvents[RequestType.ChangeTurn.ToString()]?.Invoke("");
                _ = await presenter.SendPutRequest(RequestType.ChangeTurn);
            }
        };

        _onGameFinish = async () =>
        {
            Debug.Log(GameLogicSupervisor.Instance.IsPlayableTurn);
            presenter.Model.RequestEvents[RequestType.GameFinish.ToString()]?.Invoke(container.CurrentTurn.ToString());

            await presenter.SendPutRequest(RequestType.GameFinish, container.CurrentTurn.ToString());

            GameFinish();
            await Task.Delay(3000);
            container.GameFinishInvoke();
        };
    }

    public async void Update()
    {
        if (_container.Blocks.ContainsKey(_container.SelectedBlockId)   // ブロックのIDが辞書に登録されているものか
            && _alreadySelectedId != _container.SelectedBlockId)        // すでに移動させたブロックか
        {
            _alreadySelectedId = _container.SelectedBlockId;

            await PlaceSelector();

            // ジェンガが崩れるか && ゲームがある程度進行したか
            if ((_logic.IsUnstable() || _logic.IsCollapse(_container.CollapseProbability)) && GameLogicSupervisor.Instance.LoopCount >= 2)
            {
                _onGameFinish?.Invoke();
            }
            ShopSystemController.Instance.UpdateFragmentCount(_container.Blocks[_alreadySelectedId].Fragment);

            var se = _container.Blocks[_alreadySelectedId].Weight switch
            {
                0 => SEType.MovePlastic,
                1 => SEType.MoveWood,
                3 => SEType.MoveMetal,
            };
            AudioManager.Instance.PlaySE(se);
            _onPlace?.Invoke(_alreadySelectedId, _container.SelectedBlockId);
        }
    }

    /// <summary>引き抜いたブロックをジェンガの最上段に置く際、置く場所を選択させるときに使う仮想ブロックの初期化</summary>
    private void InitSelectBoxes()
    {
        _selectBoxes = new BlockData[_container.ItemsPerLevel];

        for (int i = 0, id = -1; i < _selectBoxes.Length; i++, id--)
        {
            _selectBoxes[i] = UnityEngine.Object.Instantiate(_blockPrefab);
            _selectBoxes[i].BlockId = id;
            _selectBoxes[i].AssignedIndex = i;
            _selectBoxes[i].GetComponent<Renderer>().material.DOFade(0.5f, 0.0f);
            _selectBoxes[i].gameObject.SetActive(false);
        }
    }

    /// <summary>ジェンガを組み立てる</summary>
    private void BuildUp()
    {
        for (int i = 0; i < _container.Blocks.Count; i++)
        {
            if (i % _container.ItemsPerLevel == 0)
            {
                _destination.Set(0.0f, _destination.y, 0.0f);
                _moveDir = Vector3.zero;

                if (i == 0 && _generateBottomPos != null)
                {
                    _destination = _generateBottomPos.position;
                }
                else
                {
                    _destination.y += _blockSize.y;
                    _rotation *= Quaternion.AngleAxis(90.0f, Vector3.up);
                }

                if (_container.Blocks[i + 1].Height % 2 == 1)   // 奇数段目のとき
                {
                    _destination.x -= _blockSize.x * (_container.ItemsPerLevel / 2);
                    _moveDir.x = _blockSize.x;
                }
                else    // 偶数段目のとき
                {
                    _destination.z -= _blockSize.x * (_container.ItemsPerLevel / 2);
                    _moveDir.z = _blockSize.x;
                }
            }
            Place(_container.Blocks[i + 1].gameObject, _destination, _rotation);
            _container.Blocks[i + 1].transform.SetParent(_blockParent.transform, false);
            _destination += _moveDir;
        }
    }

    /// <summary>ブロックの座標と回転を更新する</summary>
    private void Place(int blockId, Vector3 destination, Quaternion rotation)
    {
        if (_container.Blocks.ContainsKey(blockId).Invert()) return;

        Place(_container.Blocks[blockId].gameObject, destination, rotation);
    }

    /// <summary>ゲームオブジェクトの座標と回転を更新する</summary>
    private void Place(GameObject target, Vector3 destination, Quaternion rotation)
    {
        target.transform.position = destination;
        target.transform.rotation *= rotation;
    }

    /// <summary>ジェンガの最上段のどこにブロックを置くかを選択させ、ブロックを移動させる</summary>
    private async UniTask PlaceSelector()
    {
        if (IsPlaceable().Invert()) // チェックシートの最上段が埋まっていて、ブロックを置く場所がないとき
        {
            _container.BlockMapping.Add(new int[_container.ItemsPerLevel]); // チェックシートの最上段を新たに追加
            _destination.Set(0.0f, _destination.y + _blockSize.y, 0.0f);   // ブロックを配置する座標の更新
            _moveDir = Vector3.zero;

            if (_container.BlockMapping.Count % 2 == 1) // 最上段が奇数段目のとき
            {
                _destination.z = -_blockSize.x * (_container.ItemsPerLevel / 2);
                _moveDir.z = _blockSize.x;
                _rotation = Quaternion.AngleAxis(90.0f, Vector3.up);
            }
            else // 最上段が偶数段目のとき
            {
                _destination.x = -_blockSize.x * (_container.ItemsPerLevel / 2);
                _moveDir.x = _blockSize.x;
                _rotation = Quaternion.AngleAxis(0.0f, Vector3.up);
            }

            // ブロックを引き抜いた後、置く場所を選択させるときに使う仮想ブロックの初期化
            foreach (var selectbox in _selectBoxes)
            {
                selectbox.transform.position = _destination;
                _destination += _moveDir;
                selectbox.transform.rotation = _rotation;
                selectbox.Height = _container.BlockMapping.Count - 1;
                selectbox.Stability = selectbox.AssignedIndex switch
                {
                    1 => 0.10f,
                    _ => 0.45f,
                };
            }
        }

        // ジェンガの最上段にブロックを置く場所があるかを検索する
        var highestFloor = _container.BlockMapping[_container.BlockMapping.Count - 1];

        for (int i = 0; i < highestFloor.Length; i++)
        {
            if (highestFloor[i] == 0) _selectBoxes[i].gameObject.SetActive(true);
        }

        await UniTask.WaitUntil(() => _container.SelectedBlockId < 0);  // 仮想ブロックが選択されるまで待機

        foreach (var box in _selectBoxes)   // 選択された仮想ブロックの位置に引き抜いたブロックを移動させる
        {
            if (box.BlockId == _container.SelectedBlockId)
            {
                _container.Blocks[_alreadySelectedId].transform.position = box.transform.position;
                _container.Blocks[_alreadySelectedId].transform.rotation = box.transform.rotation;
                _logic.UpdateBlockInfo(_container.Blocks[_alreadySelectedId], box); // ブロックのデータも更新する
            }

            if (box.gameObject.activeSelf) box.gameObject.SetActive(false);
        }
    }

    /// <summary> 他プレイヤーがブロックの選択を行った </summary>
    private async Task<string> BlockSelected(string requestData)
    {
        await MainThreadDispatcher.RunAsync(async () =>
        {
            var splitData = requestData.Split(',');
            var id = int.Parse(splitData[0]);
            var prob = float.Parse(splitData[1]);

            _container.SelectedBlockId = id;
            _container.CollapseProbability = prob;
            await Task.Yield();
            return "Selected";
        });
        return "Selected";
    }

    /// <summary>ジェンガの最上部にブロックを置く場所があるか</summary>
    private bool IsPlaceable()
    {
        int placeableCount = 0;

        // チェックシートの最上段を検索する
        foreach (var item in _container.BlockMapping[_container.BlockMapping.Count - 1])
        {
            if (item == 0) placeableCount++;
        }
        return placeableCount > 0;  // 最上段にブロックを置く空きがあるか
    }

    /// <summary>ゲーム終了時に行う処理</summary>
    private void GameFinish()
    {
        int blockCount = 0;
        int smallestBlockCount = int.MaxValue;
        Vector3 corrapseDir = Vector3.zero;
        RaycastHit[] hitResults = null;

        foreach (var pos in _raycastPositions)
        {
            blockCount = Physics.RaycastNonAlloc(pos, Vector3.up, hitResults, /* MaxDistance */100);

            if (smallestBlockCount > blockCount)
            {
                smallestBlockCount = blockCount;
                corrapseDir = pos;
            }
            else if (smallestBlockCount == blockCount)
            {
                corrapseDir += pos;
            }
        }

        foreach (var block in _container.Blocks)
        {
            Rigidbody blockRb = block.Value.gameObject.AddComponent<Rigidbody>();
            blockRb.AddForce(corrapseDir.normalized * _corrapsePower, ForceMode.Impulse);
        }
        Debug.Log("BREAK！");
    }

    private async Task<string> PlaceBlock(string requestData)
    {
        await MainThreadDispatcher.RunAsync(async () =>
        {
            var splitData = requestData.Split(',');
            var id = int.Parse(splitData[0]);
            var next = int.Parse(splitData[1]);

            Debug.Log($"place {id}");
            BlockData nextBlock = null;

            foreach (var target in _selectBoxes)
            {
                if (target.BlockId == next)
                {
                    nextBlock = target;
                    break;
                }
            }

            Place(id, nextBlock.transform.position, nextBlock.transform.rotation);
            await Task.Yield();
            return "Place Success";
        });
        return "Place Success";
    }
}