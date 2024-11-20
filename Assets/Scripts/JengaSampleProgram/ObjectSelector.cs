using UnityEngine;

public class ObjectSelector : MonoBehaviour
{
    [SerializeField] private LayerMask _layerMask = 0;
    [SerializeField] private Color _gizmosColor = Color.white;

    private Camera _mainCamera = null;
    private Ray _ray = new();
    private RaycastHit _hitResult = new();
    private const int MAX_RAYCAST_DISTANCE = 100;
    private RaycastHit _saveHitResult = new();

    private JengaManager _manager = null;

    private void Start()
    {
        _mainCamera = Camera.main;
        _manager = FindFirstObjectByType<JengaManager>();
    }

    private void Update()
    {
        _ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Input.GetMouseButtonDown(0)
            && Physics.Raycast(_ray, out _hitResult, MAX_RAYCAST_DISTANCE, _layerMask))
        {
            if(_hitResult.collider.TryGetComponent(out Jenga.BlockData data))
            {
                var jenga = _manager.Blocks[data.BlockID];
                _manager.Place(ref jenga);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = _gizmosColor;
        Gizmos.DrawRay(_ray.origin, _ray.direction * MAX_RAYCAST_DISTANCE);
    }
}
