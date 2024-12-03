using UnityEngine;

public class ObjectSelector : MonoBehaviour
{
    [SerializeField] private LayerMask _layerMask = 0;
    [SerializeField] private Color _gizmosColor = Color.white;

    private Camera _mainCamera = null;
    private Ray _ray = new();
    private RaycastHit _hitResult = new();
    private const int MAX_RAYCAST_DISTANCE = 100;

    private void Start()
    {
        _mainCamera = Camera.main;
    }

    private void Update()
    {
        _ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

        if (DataContainer.Instance.IsGameFinish) return;
        if (!Input.GetMouseButtonDown(0)) return;
        if (!Physics.Raycast(_ray, out _hitResult, MAX_RAYCAST_DISTANCE, _layerMask)) return;
        if (!_hitResult.collider.TryGetComponent(out BlockData data)) return;

        DataContainer.Instance.SelectedBlockId = data.BlockId;
        DataContainer.Instance.CollapseProbability = Random.Range(0f, 1f);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = _gizmosColor;
        Gizmos.DrawRay(_ray.origin, _ray.direction * MAX_RAYCAST_DISTANCE);
    }
}
