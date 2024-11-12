using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/PieceData", fileName = "New PieceData")]
public class BlockData : ScriptableObject
{
    [Tooltip("材質")]
    [SerializeField]
    private MaterialType _materialType = MaterialType.Wood;
    [Tooltip("質量")]
    [Range(0.1f, 5f)]
    [SerializeField]
    private float _mass = 1f;
    [Tooltip("摩擦の大きさ（大きいほど滑りにくい）")]
    [Range(0.1f, 5f)]
    [SerializeField]
    private float _friction = 1f;

    public MaterialType MaterialType => _materialType;
    public float Mass => _mass;
    public float Friction => _friction;

    public BlockData(MaterialType materialType)
    {
        _materialType = materialType;
        _mass = 1f;
        _friction = 1f;
    }
}

/// <summary> 材質の種類 </summary>
public enum MaterialType
{
    Wood,
    Metal,
    /// <summary> 発泡スチロール </summary>
    Styrofoam,
}
