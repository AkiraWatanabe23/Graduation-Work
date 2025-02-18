using Network;
using System;
using UnityEngine;
using VTNConnect;
using Random = UnityEngine.Random;

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

    private DataContainer _dataContainer = default;

    public void Initialize(DataContainer container, NetworkPresenter presenter)
    {
        container.GameFinishRegister(() => GameLogicSupervisor.Instance.IsGameFinish = true);

        _dataContainer = container;

        OnSelectBlock = (async (data) =>
        {
            _dataContainer.CollapseProbability = Random.Range(0f, 1f);
            await presenter.SendPutRequest(RequestType.SelectBlock, data.BlockId.ToString(), _dataContainer.CollapseProbability.ToString());
            _dataContainer.SelectedBlockId = data.BlockId;

            var clip = (int)data.Weight switch
            {
                0 => SEType.SelectPlastic,
                1 => SEType.SelectWood,
                3 => SEType.SelectMetal,
            };
            AudioManager.Instance.PlaySE(clip);

            //VantanConnect対応 ==========================================
            EventData eventData = new(EventDefine.JengaInfo);
            eventData.DataPack("SelectBlock", (int)data.Weight);
            VantanConnect.SendEvent(eventData);
            // ===========================================================
        });

        _mainCamera = Camera.main;
    }

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0)) { return; }
        if (GameLogicSupervisor.Instance.IsGameFinish) { Debug.Log("Game Finished"); return; }
        if (!GameLogicSupervisor.Instance.IsGameStart) { Debug.Log("not game start yet"); return; }
        if (!GameLogicSupervisor.Instance.IsPlayableTurn) { Debug.Log("not my turn"); return; }

        _ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(_ray, out _hitResult, MAX_RAYCAST_DISTANCE)) { Debug.Log("not hit"); return; }
        if (!_hitResult.collider.TryGetComponent(out BlockData data)) { Debug.Log("not get block"); return; }

        Debug.Log($"hit ID : {data.BlockId} {_dataContainer.SelectedBlockId}");
        OnSelectBlock?.Invoke(data);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = _gizmosColor;
        Gizmos.DrawRay(_ray.origin, _ray.direction * MAX_RAYCAST_DISTANCE);
    }

    private void OnDestroy() => OnSelectBlock = null;
}
