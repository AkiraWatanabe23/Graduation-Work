using UnityEngine;

namespace LogicSample
{
    /// <summary> 各ブロックが保持するコンポーネント </summary>
    public class BlockComponent : MonoBehaviour
    {
        [SerializeField]
        private MaterialType _materialType = MaterialType.Wood;
        [SerializeField]
        private Direciton[] _direcitons = default;

        private BlockData _blockData = default;

        public MaterialType MaterialType => _materialType;
        public Direciton[] Direcitons => _direcitons;
        public BlockData BlockData => _blockData;

        public void Initiaize(BlockData initialBlock)
        {
            _blockData = initialBlock;

            if (!gameObject.TryGetComponent(out Collider _))
            {
                _ = gameObject.AddComponent<BoxCollider>();

                //ここでブロックのサイズに合わせた値の設定をしたい
            }
        }

        public void UpdateMaterial(BlockData next)
        {
            _materialType = next.MaterialType;
            _blockData = next;
        }
    }

    public enum Direciton
    {
        Up,
        Down,
        Left,
        Right,
    }
}