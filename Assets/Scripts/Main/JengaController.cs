using System;
using System.Collections.Generic;
using UnityEngine;

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

    /// <summary>IDをキーにしてジェンガブロックを保持する辞書型</summary>
    private Dictionary<int, BlockData> _blocks = new Dictionary<int, BlockData>();
    /// <summary>ジェンガブロックがどの位置にあるかを確認するチェックシート</summary>
    private List<int[]> _blockMapping = new List<int[]>();

    private JengaLogic _logic = new();

    private Vector3 _generatePos = Vector3.zero;
    private Quaternion _generateRot = Quaternion.identity;

    private void Start()
    {
        if (_blockPrefab == null) throw new NullReferenceException($"Prefab is not found");
    }

    private void Update()
    {
        
    }

    private void BuildUp()
    {
        int placeCount = 0;

        Build(0, ref placeCount);
    }

    private void Build(int blockId, ref int placeCount)
    {
        
    }

    private void GameFinish()
    {
        foreach (var block in _blocks)
        {
            Rigidbody blockRb = block.Value.gameObject.AddComponent<Rigidbody>();
        }
        Debug.Log("BREAK！");
    }
}
