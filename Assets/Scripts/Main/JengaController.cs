using System;
using UnityEngine;

[Serializable]
public class JengaController
{
    [SerializeField, Tooltip("生成するジェンガ")]
    private BlockData _blockPrefab = null;
    [SerializeField, Tooltip("何段、ジェンガを生成するか")]
    private int _floorLevel = 10;
    [SerializeField, Tooltip("1段当たりのジェンガの個数")]
    private int _itemsPerLevel = 3;
    [SerializeField]
    private MaterialController _mateCtrler = new MaterialController();

    private JengaLogic _logic = new();
    private DataContainer _container = null;

    private Vector3 _destination = Vector3.zero;
    private GameObject _blockParent = null;

    public void Initialize(DataContainer container)
    {
        if (_blockPrefab == null) throw new NullReferenceException($"Prefab is not found");

        _blockParent = new GameObject("Block Parent");
        _logic.Initialize(_floorLevel, _itemsPerLevel, in _blockPrefab, _blockParent.transform);
        _container = container;

    }

    public void Update()
    {
        if (_logic.IsUnstable() || _logic.IsCollapse(_container.CollapseProbability))
        {
            GameFinish();
        }
    }

    private void Place(int blockId)
    {
        _container.Blocks[blockId].transform.position = _destination;
    }

    private void GameFinish()
    {
        foreach (var block in _container.Blocks)
        {
            Rigidbody blockRb = block.Value.gameObject.AddComponent<Rigidbody>();
        }
        Debug.Log("BREAK！");
    }
}
