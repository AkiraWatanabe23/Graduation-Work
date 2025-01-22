using UnityEngine;

public class BlockData : MonoBehaviour
{
    /// <summary>このブロックに与えられたID（1始まり）</summary>
    public int BlockId { get => _blockId; set => _blockId = value; }

    /// <summary>このブロックが何段目にいるか（1始まり）</summary>
    public int Height { get => _height; set => _height = value; }

    /// <summary>このブロックを引き抜いたとき、ジェンガタワーに及ぼす影響度（安定度）</summary>
    public float Stability { get => _stability; set => _stability = value; }

    /// <summary>チェックシート内における自分の添え字（0始まり）</summary>
    public int AssignedIndex { get => _assignedIndex; set => _assignedIndex = value; }

    /// <summary>このブロックの重さ</summary>
    public float Weight { get => _weight; set => _weight = value; }

    public int Fragment { get => _fragment; set => _fragment = value; }

    [SerializeField, Tooltip("自分に与えられたID")]
    private int _blockId = 0;
    [SerializeField, Tooltip("自分が何段目にいるか")]
    private int _height = 0;
    [SerializeField, Tooltip("自分がジェンガ全体に及ぼす影響度")]
    private float _stability = 0;
    [SerializeField, Tooltip("チェックシート内における自分の添え字")]
    private int _assignedIndex = -1;
    [SerializeField, Tooltip("自分の重さ")]
    private float _weight = -1;
    [SerializeField, Tooltip("自分を引き抜いたときに取得できる欠片の数")]
    private int _fragment = -1;

    private MeshRenderer _renderer = default;

    /// <summary> ブロックの材質変化を行う </summary>
    public void ChangeMaterial((float Weight, Material Material) target)
    {
        if (_renderer == null) { _renderer = GetComponent<MeshRenderer>(); }

        _weight = target.Weight;
        _renderer.material = target.Material;
    }
}
