using System;
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
    private GameObject _blockParent = null;

    public void Initialize(DataContainer container)
    {
        _blockParent = new GameObject("Block Parent");
        _logic.Initialize(container, _blockParent.transform);
        _container = container;
    }

    public void Update()
    {
        if (_logic.IsUnstable() || _logic.IsCollapse(_container.Blocks, _container.CollapseProbability))
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
