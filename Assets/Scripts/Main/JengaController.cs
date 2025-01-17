using Cysharp.Threading.Tasks;
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

    private GameObject _blockParent = null;

    public void Initialize(DataContainer container)
    {
        _blockParent = new GameObject("Block Parent");
        _logic.Initialize(container, _blockParent.transform);
        _container = container;
        BuildUp();
    }

    public void Update()
    {
        if (_container.Blocks.ContainsKey(_container.SelectedBlockId))
        {
            if (_logic.IsUnstable(_container.BlockMapping) ||
            _logic.IsCollapse(_container.Blocks, _container.BlockMapping, _container.CollapseProbability))
            {
                GameFinish();
            }

            Place(_container.SelectedBlockId, Vector3.zero, Quaternion.identity);
        }
    }

    /// <summary>ジェンガを組み立てる</summary>
    private void BuildUp()
    {
        Vector3 dest = Vector3.zero;    // ブロックの座標変更先
        Vector3 moveDir = Vector3.zero; // ブロックの座標をずらす方向
        Vector3 blockScale = _blockPrefab.transform.localScale; // ブロックの大きさ
        Quaternion angle = Quaternion.identity; // ブロックの回転方向
        bool placeDirectionSwitch = true;   // 奇数段目と偶数段目でブロックの向きを変えるので、そのスイッチ

        for (int i = 0; i < _container.Blocks.Count; i++)
        {
            if (i % _container.ItemsPerLevel == 0)
            {
                dest.Set(0.0f, dest.y, 0.0f);
                moveDir = Vector3.zero;

                if(i != 0) dest.y += blockScale.y;

                if (placeDirectionSwitch)   // 奇数段目のとき
                {
                    dest.x -= blockScale.x * (_container.ItemsPerLevel / 2);
                    moveDir.x = blockScale.x;
                    angle = Quaternion.AngleAxis(0, Vector3.up);
                }
                else    // 偶数段目のとき
                {
                    dest.z -= blockScale.x * (_container.ItemsPerLevel / 2);
                    moveDir.z = blockScale.x;
                    angle = Quaternion.AngleAxis(90, Vector3.up);
                }
                placeDirectionSwitch = !placeDirectionSwitch;
            }
            Place(i + 1, dest, angle);
            dest += moveDir;
        }
    }

    /// <summary>ブロックの座標と回転を更新する</summary>
    private void Place(int blockId, Vector3 dest, Quaternion angle)
    {
        _container.Blocks[blockId].transform.position = dest;
        _container.Blocks[blockId].transform.rotation *= angle;
    }

    private async UniTask PlaceSelector()
    {
        
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
