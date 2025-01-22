using Network;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class ObjectSelector : MonoBehaviour
{
    /// <summary> 自分が動かすブロックが選択されたときに実行される </summary>
    protected Action<BlockData> OnSelectBlock { get; private set; }

    [SerializeField] private bool _debugMode = true;

    [SerializeField] private LayerMask _layerMask = 0;
    [SerializeField] private Color _gizmosColor = Color.white;

    private Camera _mainCamera = null;
    private Ray _ray = new();
    private RaycastHit _hitResult = new();
    private const int MAX_RAYCAST_DISTANCE = 100;
    private bool _isGameFinish = false;

    private DataContainer _dataContainer = default;

    public void Initialize(DataContainer container, NetworkPresenter presenter)
    {
        container.GameFinishRegister(GameFinish);

        presenter.Model.RegisterEvent(RequestType.SelectBlock, SelectBlock);

        _dataContainer = container;
        OnSelectBlock = _debugMode switch
        {
            true => (data) => _dataContainer.SelectedBlockId = data.BlockId,
            false => async (data) =>
            {
                await presenter.SendPutRequest(RequestType.SelectBlock, data.BlockId.ToString());
            }
        };

        _mainCamera = Camera.main;
    }

    private void Update()
    {
        if (_isGameFinish) return;
        if (!Input.GetMouseButtonDown(0)) return;
        if (!GameLogicSupervisor.Instance.IsPlayableTurn && !_debugMode) return;

        _ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(_ray, out _hitResult, MAX_RAYCAST_DISTANCE)) return;
        if (!_hitResult.collider.TryGetComponent(out BlockData data)) return;

        Debug.Log($"hit ID : {data.BlockId} {_dataContainer.SelectedBlockId}");
        OnSelectBlock?.Invoke(data);
    }

    private async Task<string> SelectBlock(string requestData)
    {
        var splitData = requestData.Split(',');
        var id = int.Parse(splitData[0]);

        _dataContainer.SelectedBlockId = id;
        await Task.Yield();
        return "Block Selected";
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
