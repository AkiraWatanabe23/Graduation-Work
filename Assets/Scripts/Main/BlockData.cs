using UnityEngine;

public class BlockData : MonoBehaviour
{
    /// <summary>���̃u���b�N�ɗ^����ꂽID�i1�n�܂�j</summary>
    public int BlockId { get => _blockId; set => _blockId = value; }

    /// <summary>���̃u���b�N�����i�ڂɂ��邩�i1�n�܂�j</summary>
    public int Height { get => _height; set => _height = value; }

    /// <summary>���̃u���b�N�������������Ƃ��A�W�F���K�^���[�ɋy�ڂ��e���x�i����x�j</summary>
    public float Stability { get => _stability; set => _stability = value; }

    /// <summary>�`�F�b�N�V�[�g���ɂ����鎩���̓Y�����i0�n�܂�j</summary>
    public int AssignedIndex { get => _assignedIndex; set => _assignedIndex = value; }

    /// <summary>���̃u���b�N�̏d��</summary>
    public int Weight {  get => _weight; set => _weight = value; }

    [SerializeField, Tooltip("�����ɗ^����ꂽID")]
    private int _blockId = -1;
    [SerializeField, Tooltip("���������i�ڂɂ��邩")]
    private int _height = -1;
    [SerializeField, Tooltip("�������W�F���K�S�̂ɋy�ڂ��e���x")]
    private float _stability = -1;
    [SerializeField, Tooltip("�`�F�b�N�V�[�g���ɂ����鎩���̓Y����")]
    private int _assignedIndex = -1;
    [SerializeField, Tooltip("�����̏d��")]
    private int _weight = -1;
    [SerializeField, Tooltip("�����̍ގ��ɑΉ�����}�e���A��")]
    private Material _myMaterial = null;
}
