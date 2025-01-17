using Network;
using System;
using UnityEngine;

public class ObjectSelector : MonoBehaviour
{
    /// <summary> 自分が動かすブロックが選択されたときに実行される </summary>
    protected Action<BlockData> OnSelectBlock { get; private set; }

    [SerializeField] private LayerMask _layerMask = 0;
    [SerializeField] private Color _gizmosColor = Color.white;

    private Camera _mainCamera = null;
    private Ray _ray = new();
    private RaycastHit _hitResult = new();
    private const int MAX_RAYCAST_DISTANCE = 100;
    private bool _isGameFinish = false;

    public void Initialize(DataContainer container, NetworkPresenter presenter)
    {
        container.GameFinishRegister(GameFinish);
        OnSelectBlock = async (data) =>
        {
            container.SelectedBlockId = data.BlockId;
            //todo : どのブロックが選択されたかを他ユーザーに伝える処理
            await presenter.SendPutRequest(RequestType.SelectBlock, data.BlockId.ToString());
        };

        _mainCamera = Camera.main;
    }

    private void Update()
    {
        _ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

        if (_isGameFinish) return;
        if (!Input.GetMouseButtonDown(0)) return;
        if (!Physics.Raycast(_ray, out _hitResult, MAX_RAYCAST_DISTANCE, _layerMask)) return;
        if (!_hitResult.collider.TryGetComponent(out BlockData data)) return;

        OnSelectBlock?.Invoke(data);
    }

    private void GameFinish()
    {
        _isGameFinish = true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = _gizmosColor;
        Gizmos.DrawRay(_ray.origin, _ray.direction * MAX_RAYCAST_DISTANCE);
    }

    private void OnDestroy() => OnSelectBlock = null;
}
