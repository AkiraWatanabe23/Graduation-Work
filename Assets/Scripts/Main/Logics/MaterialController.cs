using Network;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Debug = Constants.ConsoleLogs;

/// <summary> 各ブロックの材質の管理を行うクラス </summary>
[Serializable]
public class MaterialController
{
    [Serializable]
    public class MaterialData
    {
        [field: SerializeField]
        public MaterialType MaterialType { get; private set; } = MaterialType.None;
        [field: SerializeField]
        public int Weight { get; private set; } = 1;
        [field: SerializeField]
        public Material Material { get; private set; } = default;
    }

    [SerializeField]
    private MaterialData[] _materialDatas = default;

    private Dictionary<int, BlockData> _blockDict = default;
    private Dictionary<MaterialType, (int Weight, Material Material)> _materialDatasDict = default;

    /// <summary> 初期化処理 </summary>
    public void Initialize(DataContainer container, NetworkModel model)
    {
        _blockDict = container.Blocks;

        _materialDatasDict = new();
        foreach (var item in _materialDatas)
        {
            _materialDatasDict.Add(item.MaterialType, (item.Weight, item.Material));
        }

        model.RegisterEvent(RequestType.ChangeMaterial, ChangeMaterial);
    }

    /// <summary> ブロックの材質変化を行う </summary>
    /// <param name="target"> 対象となるブロック </param>
    public void ChangeMaterial(BlockData target, MaterialType next)
    {
        //対象の材質のデータが存在しない場合、変更できない
        if (!_materialDatasDict.ContainsKey(next)) { Debug.Log($"対象の材質のデータが存在しませんでした : {next}"); return; }

        Debug.Log($"Target Weight : {target.Weight} → {_materialDatasDict[next].Weight}");
        target.ChangeMaterial(_materialDatasDict[next]);
    }

    private async Task<string> ChangeMaterial(string requestData)
    {
        var splitData = requestData.Split(',');
        _ = splitData[0];
        var id = int.Parse(splitData[1]);
        var material = splitData[2];

        ChangeMaterial(_blockDict[id], (MaterialType)Enum.Parse(typeof(MaterialType), material));

        await Task.Yield();
        return "Request Success";
    }
}

public enum MaterialType
{
    None,
    Wood,
    Metal,
    Plastic
}