using UnityEngine;

public class BlockData : MonoBehaviour
{
    /// <summary>このブロックに与えられたID（1始まり）</summary>
    public int BlockId => _blockId;

    /// <summary>このブロックが何段目にいるか（1始まり）</summary>
    public int Height => _height;

    /// <summary>このブロックを引き抜いたとき、ジェンガタワーに及ぼす影響度（安定度）</summary>
    public float Stability => _stability;

    /// <summary>チェックシート内における自分の添え字（0始まり）</summary>
    public int AssignedIndex => _assignedIndex;

    /// <summary>このブロックの重さ</summary>
    public int Weight => _weight;

    [SerializeField, Tooltip("自分に与えられたID")]
    private int _blockId = -1;
    [SerializeField, Tooltip("自分が何段目にいるか")]
    private int _height = -1;
    [SerializeField, Tooltip("自分がジェンガ全体に及ぼす影響度")]
    private float _stability = -1;
    [SerializeField, Tooltip("チェックシート内における自分の添え字")]
    private int _assignedIndex = -1;
    [SerializeField, Tooltip("自分の重さ")]
    private int _weight = -1;

    private MeshRenderer _renderer = default;

    /// <summary>ブロックの内部データを更新する</summary>
    /// <param name="id">ブロックID</param>
    /// <param name="height">このブロックがジェンガタワーの何段目にいるか</param>
    /// <param name="stab">このブロックを引き抜いたときにジェンガ全体に与える影響度</param>
    /// <param name="index">このブロックが置かれている段の何番目にいるか</param>
    /// <param name="weight">このブロックの重さ</param>
    public void UpdateData(int id,int height, float stab, int index, int weight)
    {
        _blockId = id;
        _height = height;
        _stability = stab;
        _assignedIndex = index;
        _weight = weight;
    }

    /// <summary> ブロックの材質変化を行う </summary>
    public void ChangeMaterial((int Weight, Material Material) target)
    {
        if (_renderer == null) { _renderer = GetComponent<MeshRenderer>(); }

        _weight = target.Weight;
        _renderer.material = target.Material;
    }

    // やること
    // ブロックのかけらの個数を持つ変数を作成する
    // ブロックのかけらは材質変化カードの交換に使う
    // これの初期化手段を用意する
}
