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

    private void Start()
    {
        _main = Camera.main;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) { OnSelectBlock(); }
    }

    private void OnSelectBlock()
    {
        //変更対象がない場合、処理を実行しない
        if (_currentTarget == MaterialType.None) { return; }

        //クリック時にブロックを検知したかどうか
        if (!Physics.Raycast(_main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
        {
            Debug.Log("衝突対象が検出されませんでした");
            return;
        }

        //以下クリック対象が存在した場合の処理
        if (!hit.collider.gameObject.TryGetComponent(out BlockData block)) { return; }
        if (_supervisor == null) { return; }

        _supervisor.MatCtrl.ChangeMaterial(block, _currentTarget);
        MaterialApply();

        _supervisor.NetworkPresenter.SendPutRequest(Network.RequestType.ChangeMaterial);
    }

    private void MaterialApply()
    {
        _currentTarget = MaterialType.None;
    }

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
