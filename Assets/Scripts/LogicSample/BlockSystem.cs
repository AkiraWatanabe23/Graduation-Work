using System;
using System.Collections.Generic;
using UnityEngine;

namespace LogicSample
{
    /// <summary> 積まれているブロックのロジックを記述するクラス </summary>
    [Serializable]
    public class BlockSystem
    {
        [SerializeField]
        private BlockData[] _blockDatas = default;
        [SerializeField]
        private BlockComponent[] _components = default;

        private Dictionary<MaterialType, BlockData> _blocks = default;

        protected int Height
        {
            get
            {
                if (_components == null) { return -1; }

                return _components.Length / 3;
            }
        }

        public void Initialize()
        {
            if (_blockDatas != null && _blockDatas.Length > 0)
            {
                _blocks = new();
                Array.ForEach(_blockDatas, blockData => _blocks.Add(blockData.MaterialType, blockData));
            }
            //各ブロックの初期化処理
            Array.ForEach(_components, component => component.Initiaize(_blocks[component.MaterialType]));
        }

        public BlockData GetBlockData(MaterialType materialType) => _blocks[materialType];

        public void UpdateMaterial(BlockComponent target, MaterialType materialType)
        {
            target.UpdateMaterial(_blocks[materialType]);
        }

        /// <summary> 全体の重みを計算する </summary>
        public int[][] FloorCalculation()
        {

            int[][] result = new int[3][];
            for (int i = 0; i < result.Length; i++) { result[i] = new int[] { Height, Height, Height }; }
            foreach (var block in _components)
            {
                if (block.gameObject.activeSelf) { continue; }

                for (int i = 0; i < block.Direcitons.Length; i++)
                {
                    var (row, column) = GetIndex(block.Direcitons[i]);
                    result[row][column]--;
                }
            }

            return result;
        }

        /// <summary> ジャグ配列のインデックスを取得する </summary>
        /// <returns> インデックスの値（tuple） </returns>
        private (int row, int column) GetIndex(Direciton direciton)
            => direciton switch
            {
                Direciton.Up => (0, 1),
                Direciton.Down => (2, 1),
                Direciton.Left => (1, 0),
                Direciton.Right => (1, 2),
                _ => (0, 0),
            };
    }
}