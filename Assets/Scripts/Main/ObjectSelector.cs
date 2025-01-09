using UnityEngine;

public class ObjectSelector : MonoBehaviour
{
    [SerializeField] private LayerMask _layerMask = 0;
    [SerializeField] private Color _gizmosColor = Color.white;

    private Camera _mainCamera = null;
    private Ray _ray = new();
    private RaycastHit _hitResult = new();
    private const int MAX_RAYCAST_DISTANCE = 100;
    private bool _isGameFinish = false;

    private void OnEnable()
    {
        DataContainer.Instance.GameFinishRegister(GameFinish);
    }

    private void Start()
    {
        _mainCamera = Camera.main;
    }

    private void Update()
    {
        _ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

        if (_isGameFinish) return;
        if (!Input.GetMouseButtonDown(0)) return;
        if (!Physics.Raycast(_ray, out _hitResult, MAX_RAYCAST_DISTANCE, _layerMask)) return;
        if (!_hitResult.collider.TryGetComponent(out BlockData data)) return;

        DataContainer.Instance.SelectedBlockId = data.BlockId;
        DataContainer.Instance.CollapseProbability = Random.Range(0f, 1f);
    }

    private void GameFinish()
    {
        _isGameFinish = true;
        DataContainer.Instance.GameFinishUnregister(GameFinish);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = _gizmosColor;
        Gizmos.DrawRay(_ray.origin, _ray.direction * MAX_RAYCAST_DISTANCE);
    }
}
