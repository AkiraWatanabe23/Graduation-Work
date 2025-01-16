using System;
using UnityEngine;

[Serializable]
public class JengaController
{
    public BlockData BlockPrefab => _blockPrefab;

    [SerializeField, Tooltip("ê∂ê¨Ç∑ÇÈÉWÉFÉìÉK")]
    private BlockData _blockPrefab = null;

    private JengaLogic _logic = new();
    private DataContainer _container = null;

    private Vector3 _destination = Vector3.zero;
    private GameObject _blockParent = null;

    public void Initialize(DataContainer container)
    {
        if (_blockPrefab == null) throw new NullReferenceException($"Prefab is not found");

        _blockParent = new GameObject("Block Parent");
        //_logic.Initialize(_floorLevel, _itemsPerLevel, in _blockPrefab, _blockParent.transform);
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
        Debug.Log("BREAKÅI");
    }
}
