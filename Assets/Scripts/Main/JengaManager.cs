using System;
using System.Collections.Generic;
using UnityEngine;

public class JengaManager : MonoBehaviour
{


    //[SerializeField, Tooltip("生成するジェンガ")]
    //private BlockData _blockPrefab = null;
    //[SerializeField, Tooltip("何段、ジェンガを生成するか")]
    //private int _floorLevel = 10;
    //[SerializeField, Tooltip("1段当たりのジェンガの個数")]
    //private int _itemsPerLevel = 3;
    //[SerializeField]
    //private MaterialController _mateCtrler = new MaterialController();

    //private JengaLogic _logic = new();

    //private Vector3 _updatePos = Vector3.zero;
    //private Quaternion _updateRot = Quaternion.identity;
    //private int _blockIndexCounter = 0;
    //private int _placeCount = 0;

    ///// <summary>IDをキーにしてジェンガブロックを保持する辞書型</summary>
    //private Dictionary<int, BlockData> _blocks = new Dictionary<int, BlockData>();
    ///// <summary>ジェンガブロックがどの位置にあるかを確認するチェックシート</summary>
    //private List<int[]> _blockMapping = new List<int[]>();

    //private void OnEnable()
    //{
    //    DataContainer.Instance.GameFinishRegister(GameFinish);
    //}

    //private void Start()
    //{
    //    if (_blockPrefab == null) throw new NullReferenceException($"Prefab is not found");

    //    BuildUp();
    //    InitBlockExistsChecker();
    //    _mateCtrler.Initialize();
    //}

    //private void Update()
    //{
    //    if (_logic.IsNewBlockSelected(out int targetId))
    //    {
    //        Build(targetId);
    //        ExpandBlockExistsChecker(targetId);

    //        if (_logic.IsUnstable(_blockMapping) 
    //            || _logic.IsCollapse(_blocks, _blockMapping, 0f))
    //        {
    //            DataContainer.Instance.GameFinishInvoke();
    //        }
    //    }
    //}

    //private void BlockData(int targetId)
    //{
    //    int oldHeight = _blocks[targetId].Height;
    //    (int Height, int AssignedIndex) newDest = default /*変更予定*/;

    //    int tmp = _blockMapping[oldHeight][_blocks[targetId].AssignedIndex];
    //    _blockMapping[oldHeight][_blocks[targetId].AssignedIndex] = 0;
    //    _blockMapping[newDest.Height][newDest.AssignedIndex] = tmp;

    //    _blocks[targetId].Height = newDest.Height;
    //    _blocks[targetId].AssignedIndex = newDest.AssignedIndex;
    //}

    ///// <summary>ジェンガを指定された階層の分だけ組み立てる</summary>
    //private void BuildUp()
    //{
    //    BlockData block = null;
    //    GameObject blockParent = new GameObject("Blocks");

    //    for (int i = 1; i <= _floorLevel * _itemsPerLevel; i++)
    //    {
    //        block = Instantiate(_blockPrefab, blockParent.transform);
    //        block.BlockId = i;
    //        block.AssignedIndex = AssignedIndexCounter();
    //        _blocks.Add(i, block);
    //        Build(i);
    //    }
    //}

    ///// <summary>ジェンガを配置する</summary>
    ///// <param name="target">配置する対象のブロック</param>
    //private void Build(int targetId)
    //{
    //    //ブロックの座標の変更先を更新する
    //    if (_placeCount == 0) _updatePos.x -= _itemsPerLevel / 2;
    //    else if (_placeCount < _itemsPerLevel) _updatePos.x++;
    //    else if (_placeCount == _itemsPerLevel) _updatePos.z -= _itemsPerLevel / 2;
    //    else if (_placeCount < _itemsPerLevel * 2) _updatePos.z++;

    //    //ブロックを１段ごとに互い違いとなるよう、向きを 90°回転させる
    //    if (_placeCount % _itemsPerLevel == 0)
    //        _updateRot = Quaternion.AngleAxis(90.0f * _updatePos.y, Vector3.up);

    //    //ブロックの座標・回転を更新
    //    _blocks[targetId].transform.position = _updatePos;
    //    _blocks[targetId].transform.rotation = _updateRot;
    //    _placeCount++;

    //    if (_placeCount % _itemsPerLevel == 0) _updatePos.Set(0.0f, ++_updatePos.y, 0.0f);
    //    if (_placeCount % (_itemsPerLevel * 2) == 0) _placeCount = 0;
    //}

    ///// <summary>ジェンガブロックがどこにあるかを記録するチェックシートの初期化</summary>
    //private void InitBlockExistsChecker()
    //{
    //    for (int i = 0, blockId = 0; i <= _floorLevel; i++)
    //    {
    //        int[] blockCheckItem = null;

    //        if (0 < i)
    //        {
    //            blockCheckItem = new int[_itemsPerLevel];

    //            for (int k = 0; k < _itemsPerLevel; k++)
    //            {
    //                blockCheckItem[k] = ++blockId;
    //            }
    //        }
    //        _blockMapping.Add(blockCheckItem);
    //    }
    //}

    ///// <summary></summary>
    ///// <param name="targetId">対象のジェンガブロックのID</param>
    //private void ExpandBlockExistsChecker(int targetId)
    //{
    //    if (_blockMapping.Count > _blocks[targetId].Height) return;

    //    int[] blockCheckItem = new int[_itemsPerLevel];
    //    Array.Fill(blockCheckItem, 0);
    //    _blockMapping.Add(blockCheckItem);
    //}

    ///// <summary>ブロックに与える添え字をカウントする</summary>
    //private int AssignedIndexCounter()
    //{
    //    if (_blockIndexCounter % _itemsPerLevel == 0)
    //    {
    //        _blockIndexCounter = 0;
    //    }
    //    return _blockIndexCounter++;
    //}

    //private void GameFinish()
    //{
    //    foreach (var block in _blocks)
    //    {
    //        Rigidbody blockRb = block.Value.gameObject.AddComponent<Rigidbody>();
    //    }
    //    Debug.Log("BREAK！");

    //    DataContainer.Instance.GameFinishUnregister(GameFinish);
    //}
}
