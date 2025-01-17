using System;
using UnityEngine;
using Debug = Constants.ConsoleLogs;

/// <summary> ショップで購入したデータをインゲームに反映させるためのインプットを管理するクラス </summary>
public class MaterialInputHandler : MonoBehaviour
{
    [SerializeField]
    private GameLogicSupervisor _supervisor = default;

    [ReadOnly]
    [Tooltip("現在対象になっている材質")]
    [SerializeField]
    private MaterialType _currentTarget = MaterialType.None;

    private Camera _main = default;

    public Action<MaterialType> OnChangeMaterial { get; set; }
    public Action<MaterialType> OnCancelSelect { get; set; }

    private void Start()
    {
        _main = Camera.main;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) { OnSelectBlock(); }
    }

    private async void OnSelectBlock()
    {
        //変更対象がない場合、処理を実行しない
        if (_currentTarget == MaterialType.None) { return; }

        //クリック時にブロックを検知したかどうか
        if (!Physics.Raycast(_main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
        {
            Debug.Log("衝突対象が検出されませんでした");
            OnCancelSelect?.Invoke(_currentTarget);
            MaterialApply();
            return;
        }

        //以下クリック対象が存在した場合の処理
        if (_supervisor == null) { return; }
        if (!hit.collider.gameObject.TryGetComponent(out BlockData block)) { return; }

        //ローカル環境で材質の変更が成された場合のみ他ユーザーに情報を送る
        if (_supervisor.MatCtrl.ChangeMaterial(block, _currentTarget))
        {
            OnChangeMaterial?.Invoke(_currentTarget);
            await _supervisor.NetworkPresenter.SendPutRequest(Network.RequestType.ChangeMaterial, block.BlockId.ToString(), _currentTarget.ToString());
        }
        MaterialApply();
    }

    /// <summary> 材質変更後に実行する </summary>
    private void MaterialApply()
    {
        _currentTarget = MaterialType.None;
    }

    /// <summary> 変更対象の材質を設定する </summary>
    public void TargetSetting(MaterialType target)
    {
        if (_currentTarget != MaterialType.None)
        {
            Debug.Log("他の材質を選択中です");
            return;
        }
        _currentTarget = target;
    }
}
