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
    public int Weight {  get => _weight; set => _weight = value; }

    [SerializeField, Tooltip("このブロックに与えられたID")]
    private int _blockId = -1;
    [SerializeField, Tooltip("このブロックが何段目にいるか")]
    private int _height = -1;
    [SerializeField, Tooltip("このブロックがジェンガ全体に及ぼす影響度")]
    private float _stability = -1;
    [SerializeField, Tooltip("チェックシート内における自分の添え字")]
    private int _assignedIndex = -1;
    [SerializeField, Tooltip("このブロックの重さ")]
    private int _weight = -1;
}
