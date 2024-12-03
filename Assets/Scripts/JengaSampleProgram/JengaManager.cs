using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public class JengaManager : MonoBehaviour
{
    [SerializeField, Tooltip("��������W�F���K�̃I�u�W�F�N�g")]
    private BlockData _jenga = null;
    [SerializeField, Tooltip("���i�A�W�F���K�𐶐����邩")]
    private int _floorLevel = 10;
    [SerializeField, Tooltip("1�i������̃W�F���K�̌�")]
    private int _itemsPerLevel = 3;

    private Vector3 _generatePos = Vector3.zero;
    private (float X, float Z) _saveXZ = (0f, 0f);
    private int _blockIndexCounter = 0;
    private int _placeCount = 0;

    private bool _isGameFinish = false;

    /// <summary>ID���L�[�ɂ��ăW�F���K�u���b�N��ێ����鎫���^</summary>
    private Dictionary<int, BlockData> _blocks = new Dictionary<int, BlockData>();
    /// <summary>�W�F���K�̌���������m�F����`�F�b�N�V�[�g</summary>
    private List<int[]> _blockExistsChecker = new List<int[]>();

    private void Start()
    {
        if (_jenga == null) throw new System.NullReferenceException($"jenga is not found");

        _saveXZ = (_generatePos.x, _generatePos.z);

        Build();
        InitBlockExistsChecker();
    }

    private void Update()
    {
        if (IsNewBlockSelected(out int targetId))
        {
            BlockData target = _blocks[targetId];
            int tmp = _blockExistsChecker[target.Height][target.AssignedIndex];
            _blockExistsChecker[target.Height][target.AssignedIndex] = 0;
            Place(targetId);
            UpdateBlockExistsChecker(targetId);
            _blockExistsChecker[target.Height][target.AssignedIndex] = tmp;

            if (IsJengaUnstable() || IsJengaCollapse())
            {
                GameFinish();
            }
        }
    }

    /// <summary>�W�F���K���w�肳�ꂽ�K�w�̕������g�ݗ��Ă�</summary>
    private void Build()
    {
        BlockData jenga = null;
        GameObject blockParent = new GameObject("Blocks");

        for (int i = 1; i <= _floorLevel * _itemsPerLevel; i++)
        {
            jenga = Instantiate(_jenga, blockParent.transform);
            jenga.BlockId = i;
            jenga.AssignedIndex = IndexCounter();
            _blocks.Add(i, jenga);
            Place(jenga.BlockId);
        }
    }

    /// <summary>�W�F���K��z�u����</summary>
    /// <param name="target">�z�u����Ώۂ̃u���b�N</param>
    private void Place(int targetBlockId)
    {
        BlockData target = _blocks[targetBlockId];

        if (_placeCount == 0) _generatePos.x--;
        else if (_placeCount < _itemsPerLevel) _generatePos.x++;
        else if (_placeCount == _itemsPerLevel) _generatePos.z--;
        else if (_placeCount < _itemsPerLevel * 2) _generatePos.z++;

        if (_placeCount < _itemsPerLevel) 
            target.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        else if (_itemsPerLevel <= _placeCount 
            && _placeCount < _itemsPerLevel * 2 
            && target.transform.rotation.y != 90f) 
            target.transform.rotation = Quaternion.Euler(0f, 90f, 0f);

        if(_placeCount == 0
            || _placeCount == _itemsPerLevel - 1
            || _placeCount == _itemsPerLevel
            || _placeCount == _itemsPerLevel * 2 - 1) target.Stability = 0.45f;
        else target.Stability = 0.1f;

        target.transform.position = _generatePos;
        target.Height = (int)_generatePos.y + 1;
        _placeCount++;

        if (_placeCount % _itemsPerLevel == 0) _generatePos.Set(_saveXZ.X, ++_generatePos.y, _saveXZ.Z);
        if (_placeCount % (_itemsPerLevel * 2) == 0) _placeCount = 0;
    }

    private int _oldBlockId = -1;

    /// <summary>�v���C���ɑI�����ꂽ�u���b�N��ID���V�����I�����ꂽ���̂��ǂ���</summary>
    /// <param name="newBlockId">�V�����I�����ꂽ�u���b�N�������ꍇ�A���̃u���b�N��ID��Ԃ�</param>
    /// <returns>�V�����I�����ꂽ�u���b�N���ǂ����̐^�U�l��Ԃ�</returns>
    private bool IsNewBlockSelected(out int newBlockId)
    {
        int selectedBlockId = DataContainer.Instance.SelectedBlockId;

        if (_oldBlockId == selectedBlockId) { newBlockId = -1; return false; }

        _oldBlockId = selectedBlockId;
        newBlockId = selectedBlockId;
        return true;
    }

    /// <summary>�W�F���K�u���b�N���ǂ��ɂ��邩���L�^����`�F�b�N�V�[�g�̏�����</summary>
    private void InitBlockExistsChecker()
    {
        for (int i = 0, blockId = 1; i <= _floorLevel; i++)
        {
            int[] blockCheckItem = null;

            if (0 < i)
            {
                blockCheckItem = new int[_itemsPerLevel];

                for (int k = 0; k < _itemsPerLevel; k++)
                {
                    blockCheckItem[k] = blockId++;
                }
            }
            _blockExistsChecker.Add(blockCheckItem);
        }
    }

    /// <summary>�W�F���K�u���b�N���ǂ��ɂ��邩���L�^����`�F�b�N�V�[�g�̍X�V</summary>
    /// <param name="targetBlockId">�Ώۂ̃W�F���K�u���b�N��ID</param>
    private void UpdateBlockExistsChecker(int targetBlockId)
    {
        if (_blockExistsChecker.Count <= _blocks[targetBlockId].Height)
        {
            int[] blockCheckItem = new int[_itemsPerLevel];
            Array.Fill(blockCheckItem, 0);
            _blockExistsChecker.Add(blockCheckItem);
        }
        _blocks[targetBlockId].AssignedIndex = IndexCounter();
    }

    private void DebugBlockExistChecker()
    {
        StringBuilder debugMessage = new StringBuilder();

        foreach (var listItem in _blockExistsChecker)
        {
            if (listItem == null) continue;

            for (int k = 0; k < listItem.Length; k++)
            {
                debugMessage.Append(listItem[k]);
            }
            debugMessage.Append('\n');
        }
        Debug.Log(debugMessage);
    }

    /// <summary></summary>
    /// <returns></returns>
    private int IndexCounter()
    {
        if (_blockIndexCounter % _itemsPerLevel == 0)
        {
            _blockIndexCounter = 0;
        }
        return _blockIndexCounter++;
    }

    private bool IsJengaUnstable()
    {
        bool isNotCenterExist = true;
        int zeroCounter = 0;

        foreach (var listItem in _blockExistsChecker)
        {
            if(listItem == null) continue;
            zeroCounter = 0;

            for (int i = 0; i < listItem.Length; i++)
            {
                if (listItem[i] == 0) zeroCounter++;
                if (0 < i && i < listItem.Length - 1 && listItem[i] != 0) isNotCenterExist = false;
            }

            if (isNotCenterExist && zeroCounter >= 2) return true;
        }
        return false;
    }

    /// <summary>�u���b�N�������������Ƃ��A�W�F���K�����󂷂�m�����v�Z����</summary>
    /// <returns></returns>
    private float GetCollapseRisk()
    {
        float sumAllStability = 0f;

        foreach (var listItem in _blockExistsChecker)
        {
            if(listItem == null) continue;

            for (int k = 0; k < listItem.Length; k++)
            {
                sumAllStability += listItem[k] switch
                {
                    0 => listItem[k],
                    _ => _blocks[listItem[k]].Stability,
                };
            }
        }
        return 1f - (sumAllStability / (_blockExistsChecker.Count - 1));
    }

    private bool IsJengaCollapse()
    {
        float collapseRisk = GetCollapseRisk();
        Debug.Log($"�|�󗦂́A{collapseRisk * 100}%�I");
        return DataContainer.Instance.CollapseProbability <= collapseRisk;
    }

    private void GameFinish()
    {
        DataContainer.Instance.IsGameFinish = true;

        foreach (var block in _blocks)
        {
            block.Value.gameObject.AddComponent<Rigidbody>();
        }
        Debug.Log("BREAK�I");
    }
}
