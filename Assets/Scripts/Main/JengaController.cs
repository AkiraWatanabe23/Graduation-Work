using Cysharp.Threading.Tasks;
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

    private JengaLogic _logic = new();
    private DataContainer _container = null;

    private Vector3 _destination = Vector3.zero;
    private Quaternion _rotation = Quaternion.identity;
    private Vector3 _blockScale = Vector3.zero;
    private GameObject _blockParent = null;
    private int _alreadySelectedId = -1;

    private BlockData[] _selectBoxes = null;

    private Action<int, Vector3, Quaternion> _onPlace = null;

    public void Initialize(DataContainer container, NetworkPresenter presenter)
    {
        _blockScale = _blockPrefab.transform.localScale;
        _blockParent = new GameObject("Block Parent");
        _logic.Initialize(container, _blockParent.transform);
        _container = container;
        InitSelectBoxes(container);
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

            if (_logic.IsUnstable(_container.BlockMapping) ||
            _logic.IsCollapse(_container.Blocks, _container.BlockMapping, _container.CollapseProbability))
            {
                GameFinish();
            }

            //await PlaceSelector();
            _onPlace?.Invoke(_container.SelectedBlockId, _destination, _rotation);
        }
    }

    public void NextDestinationReceiver(Vector3 dest, Quaternion rotation)
    {
        _destination = dest;
        _rotation = rotation;
    }

    private void InitSelectBoxes(DataContainer container)
    {
        _selectBoxes = new BlockData[container.ItemsPerLevel];

        for (int i = 0; i < _selectBoxes.Length; i++)
        {
            _selectBoxes[i] = UnityEngine.Object.Instantiate(_blockPrefab);
            _selectBoxes[i].transform.localScale = _blockScale;
            _selectBoxes[i].gameObject.AddComponent<SelectBox>();
            _selectBoxes[i].gameObject.SetActive(false);
        }
    }

    /// <summary>ジェンガを組み立てる</summary>
    private void BuildUp()
    {
        Vector3 moveDir = Vector3.zero; // ブロックの座標をずらす方向

        for (int i = 0; i < _container.Blocks.Count; i++)
        {
            if (i % _container.ItemsPerLevel == 0)
            {
                _destination.Set(0.0f, _destination.y, 0.0f);
                moveDir = Vector3.zero;

                if (i != 0)
                {
                    _destination.y += _blockScale.y;
                    _rotation *= Quaternion.AngleAxis(90.0f, Vector3.up);
                }

                if (_container.Blocks[i + 1].Height % 2 == 1)   // 奇数段目のとき
                {
                    _destination.x -= _blockScale.x * (_container.ItemsPerLevel / 2);
                    moveDir.x = _blockScale.x;
                }
                else    // 偶数段目のとき
                {
                    _destination.z -= _blockScale.x * (_container.ItemsPerLevel / 2);
                    moveDir.z = _blockScale.x;
                }
            }
            Place(_container.Blocks[i + 1].gameObject, _destination, _rotation);
            _destination += moveDir;
        }
    }

    private void Place(int blockId, Vector3 destination, Quaternion rotation)
    {
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
            _destination.y += _blockScale.y;

            if (_container.BlockMapping.Count % 2 == 0)
            {
                _destination.z = _blockScale.x * (_container.ItemsPerLevel / 2);
                _rotation = Quaternion.AngleAxis(90.0f, Vector3.up);
            }
            else
            {
                _destination.x = _blockScale.x * (_container.ItemsPerLevel / 2);
                _rotation = Quaternion.AngleAxis(0.0f, Vector3.up);
            }
        }

        var highestFloor = _container.BlockMapping[_container.BlockMapping.Count - 1];

        for (int i = 0; i < highestFloor.Length; i++)
        {
            if (highestFloor[i] == 0)
            {
                _selectBoxes[i].gameObject.SetActive(true);
            }
        }

        await UniTask.WaitUntil(() => true);
    }

    /// <summary>ジェンガの最上部にブロックを置く場所があるか</summary>
    private bool IsPlaceable()
    {
        int placeableCount = 0;

        foreach (var item in _container.BlockMapping[_container.BlockMapping.Count])
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