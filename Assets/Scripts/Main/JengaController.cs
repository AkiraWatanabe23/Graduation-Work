using Cysharp.Threading.Tasks;
using Extention;
using Network;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class JengaController
{
    public BlockData BlockPrefab => _blockPrefab;

    [SerializeField, Tooltip("生成するジェンガ")]
    private BlockData _blockPrefab = null;

    private JengaLogic _logic = new();
    private DataContainer _container = null;

    private Vector3 _destination = Vector3.zero;
    private Quaternion _rotation = Quaternion.identity;
    private Vector3 _moveDir = Vector3.zero;
    private Vector3 _blockScale = Vector3.zero;
    private GameObject _blockParent = null;
    private int _alreadySelectedId = 0;

    private BlockData[] _selectBoxes = null;
    private int _selectBoxId = -1;

    private Action<int, Vector3, Quaternion> _onPlace = null;

    public void Initialize(DataContainer container, NetworkPresenter presenter)
    {
        _blockScale = _blockPrefab.transform.localScale;
        _blockParent = new GameObject("Block Parent");
        _container = container;
        _logic.Initialize(container);
        InitSelectBoxes();
        BuildUp();

        presenter.Model.RegisterEvent(RequestType.PlaceBlock, PlaceBlock);
        _onPlace = async (id, vector, quaternion) =>
        {
            await presenter.SendPutRequest(RequestType.PlaceBlock, id.ToString(), vector.ToString(), quaternion.ToString());
        };
    }

    public async void Update()
    {
        if (_container.Blocks.ContainsKey(_container.SelectedBlockId)
            && _alreadySelectedId != _container.SelectedBlockId)
        {
            _alreadySelectedId = _container.SelectedBlockId;

            await PlaceSelector();

            if (_logic.IsUnstable() || _logic.IsCollapse(_container.CollapseProbability))
            {
                GameFinish();
            }
            ShopSystemController.Instance.UpdateFragmentCount(_container.Blocks[_alreadySelectedId].Fragment);
            _onPlace?.Invoke(_container.SelectedBlockId, _destination, _rotation);
        }
    }

    private void InitSelectBoxes()
    {
        _selectBoxes = new BlockData[_container.ItemsPerLevel];

        for (int i = 0, id = -1; i < _selectBoxes.Length; i++, id--)
        {
            _selectBoxes[i] = GameObject.CreatePrimitive(PrimitiveType.Cube).AddComponent<BlockData>();
            _selectBoxes[i].BlockId = id;
            _selectBoxes[i].AssignedIndex = i;
            _selectBoxes[i].transform.localScale = _blockScale;
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

                if (i != 0)
                {
                    _destination.y += _blockScale.y;
                    _rotation *= Quaternion.AngleAxis(90.0f, Vector3.up);
                }

                if (_container.Blocks[i + 1].Height % 2 == 1)   // 奇数段目のとき
                {
                    _destination.x -= _blockScale.x * (_container.ItemsPerLevel / 2);
                    _moveDir.x = _blockScale.x;
                }
                else    // 偶数段目のとき
                {
                    _destination.z -= _blockScale.x * (_container.ItemsPerLevel / 2);
                    _moveDir.z = _blockScale.x;
                }
            }
            Place(_container.Blocks[i + 1].gameObject, _destination, _rotation);
            _container.Blocks[i + 1].transform.SetParent(_blockParent.transform, false);
            _destination += _moveDir;
        }
    }

    private void Place(int blockId, Vector3 destination, Quaternion rotation)
    {
        if (_container.Blocks.ContainsKey(blockId).Invert()) return;

        Place(_container.Blocks[blockId].gameObject, destination, rotation);
    }

    /// <summary>ブロックの座標と回転を更新する</summary>
    private void Place(GameObject target, Vector3 destination, Quaternion rotation)
    {
        target.transform.position = destination;
        target.transform.rotation *= rotation;
    }

    private async UniTask PlaceSelector()
    {
        if (IsPlaceable().Invert())
        {
            _container.BlockMapping.Add(new int[_container.ItemsPerLevel]);
            _destination.Set(0.0f, _destination.y + _blockScale.y, 0.0f);
            _moveDir = Vector3.zero;

            if (_container.BlockMapping.Count % 2 == 0)
            {
                _destination.z = -_blockScale.x * (_container.ItemsPerLevel / 2);
                _moveDir.z = _blockScale.x;
                _rotation = Quaternion.AngleAxis(90.0f, Vector3.up);
            }
            else
            {
                _destination.x = -_blockScale.x * (_container.ItemsPerLevel / 2);
                _moveDir.x = _blockScale.x;
                _rotation = Quaternion.AngleAxis(0.0f, Vector3.up);
            }

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

        var highestFloor = _container.BlockMapping[_container.BlockMapping.Count - 1];

        for (int i = 0; i < highestFloor.Length; i++)
        {
            if (highestFloor[i] == 0) _selectBoxes[i].gameObject.SetActive(true);
        }

        await UniTask.WaitUntil(() => _container.SelectedBlockId < 0);

        foreach (var box in _selectBoxes)
        {
            if (box.BlockId == _container.SelectedBlockId)
            {
                _container.Blocks[_alreadySelectedId].transform.position = box.transform.position;
                _container.Blocks[_alreadySelectedId].transform.rotation = box.transform.rotation;
                _logic.UpdateBlockInfo(_container.Blocks[_alreadySelectedId], box);
            }

            if(box.gameObject.activeSelf) box.gameObject.SetActive(false);
        }
    }

    /// <summary>ジェンガの最上部にブロックを置く場所があるか</summary>
    private bool IsPlaceable()
    {
        int placeableCount = 0;

        foreach (var item in _container.BlockMapping[_container.BlockMapping.Count - 1])
        {
            if (item == 0) placeableCount++;
        }
        return placeableCount > 0;
    }

    private void GameFinish()
    {
        foreach (var block in _container.Blocks)
        {
            Rigidbody blockRb = block.Value.gameObject.AddComponent<Rigidbody>();
        }
        Debug.Log("BREAK！");
    }

    private async Task<string> PlaceBlock(string requestData)
    {
        var splitData = requestData.Split(',');
        var id = int.Parse(splitData[0]);

        //Position
        var posX = float.Parse(splitData[1].Trim('('));
        var posY = float.Parse(splitData[2]);
        var posZ = float.Parse(splitData[3].Trim(')'));

        //Quaternion
        var quatX = float.Parse(splitData[4].Trim('('));
        var quatY = float.Parse(splitData[5]);
        var quatZ = float.Parse(splitData[6]);
        var quatW = float.Parse(splitData[7].Trim(')'));

        Place(id, new(posX, posY, posZ), new Quaternion(quatX, quatY, quatZ, quatW));
        await Task.Yield();
        return "Place Success";
    }
}