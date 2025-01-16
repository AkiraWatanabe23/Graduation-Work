using System;
using System.Collections.Generic;
using UnityEngine;

public class JengaController
{
    [SerializeField, Tooltip("��������W�F���K")]
    private BlockData _blockPrefab = null;
    [SerializeField, Tooltip("���i�A�W�F���K�𐶐����邩")]
    private int _floorLevel = 10;
    [SerializeField, Tooltip("1�i������̃W�F���K�̌�")]
    private int _itemsPerLevel = 3;
    [SerializeField]
    private MaterialController _mateCtrler = new MaterialController();

    /// <summary>ID���L�[�ɂ��ăW�F���K�u���b�N��ێ����鎫���^</summary>
    private Dictionary<int, BlockData> _blocks = new Dictionary<int, BlockData>();
    /// <summary>�W�F���K�u���b�N���ǂ̈ʒu�ɂ��邩���m�F����`�F�b�N�V�[�g</summary>
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
        Debug.Log("BREAK�I");
    }
}
