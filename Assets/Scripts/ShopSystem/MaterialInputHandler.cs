using UnityEngine;
using Debug = Constants.ConsoleLogs;

/// <summary> ショップで購入したデータをインゲームに反映させるためのインプットを管理するクラス </summary>
public class MaterialInputHandler : MonoBehaviour
{
    [ReadOnly]
    [SerializeField]
    private MaterialType _currentTarget = MaterialType.None;

    public void TargetSetting(MaterialType target)
    {
        if (_currentTarget != MaterialType.None)
        {
            Debug.Log("他の材質を選択中です");
            return;
        }
        _currentTarget = target;
    }

    private void MaterialApply()
    {
        _currentTarget = MaterialType.None;
    }
}
