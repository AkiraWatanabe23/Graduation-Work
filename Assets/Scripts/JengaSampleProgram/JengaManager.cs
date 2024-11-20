using System.Collections.Generic;
using UnityEngine;

public class JengaManager : MonoBehaviour
{
    public Dictionary<int, GameObject> Blocks => _blocks;

    [SerializeField, Tooltip("生成するジェンガのオブジェクト")]
    private GameObject _jenga = null;
    [SerializeField, Tooltip("何段、ジェンガを生成するか")]
    private int _floorLevel = 10;
    [SerializeField, Tooltip("1段当たりのジェンガの個数")]
    private int _itemsPerLevel = 3;

    private Vector3 _generatePos = Vector3.zero;
    private (float X, float Z) _saveXZ = (0f, 0f);
    private int _placeCount = 0;

    private Dictionary<int, GameObject> _blocks = new Dictionary<int, GameObject>();

    private void Start()
    {
        if (_jenga == null) throw new System.NullReferenceException($"jenga is not found");

        _saveXZ = (_generatePos.x, _generatePos.z);

        Build();
    }

    /// <summary>ジェンガを組み立てる</summary>
    private void Build()
    {
        GameObject jenga = null;
        GameObject blockParent = new GameObject("Blocks");

        for (int i = 1; i <= _floorLevel * 3; i++)
        {
            jenga = Instantiate(_jenga, blockParent.transform);

            if (jenga.TryGetComponent(out Jenga.BlockData data)) data.Init(i);
            _blocks.Add(i, jenga);
            Place(ref jenga);
        }
    }

    public void Place(ref GameObject target)
    {
        if (_placeCount == 0) _generatePos.x--;
        else if (_placeCount < 3) _generatePos.x++;
        else if (_placeCount == 3) _generatePos.z--;
        else if (_placeCount < 6) _generatePos.z++;

        if (_placeCount < 3) target.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        else if (3 <= _placeCount && _placeCount < 6 && target.transform.rotation.y != 90f)
            target.transform.rotation = Quaternion.Euler(0f, 90f, 0f);

        target.transform.position = _generatePos;
        _placeCount++;

        if (_placeCount % 3 == 0) _generatePos.Set(_saveXZ.X, ++_generatePos.y, _saveXZ.Z);
        if (_placeCount % 6 == 0) _placeCount = 0;
    }
}
