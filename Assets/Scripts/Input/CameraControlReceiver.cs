using Cinemachine;
using Extention;
using UnityEngine;

/// <summary>カメラコントローラーからの入力を受け取って、カメラを動かす</summary>
public class CameraControlReceiver : MonoBehaviour
{
    public bool IsCameraMove { get; set; } = false;
    public Vector3 MoveDirection { get; set; } = Vector3.zero;

    [SerializeField, Tooltip("操作するカメラ本体")]
    private CinemachineVirtualCamera _vCam = null;
    [SerializeField, Range(0.0f, 10.0f)]
    private float _cameraMoveSpeed = 1.0f;
    [SerializeField, Range(0.0f, 10.0f)]
    private float _cameraZoomSpeed = 1.0f;
    [SerializeField, Range(1.0f, 90.0f)]
    private float _cameraFovMin = 0.0f;
    [SerializeField, Range(90.0f, 179.0f)]
    private float _cameraFovMax = 0.0f;

    private Vector3 _center = Vector3.zero;
    private Vector3 _axis = Vector3.zero;
    private float _moveValue = float.Epsilon;

    private void Start()
    {
        ReferenceCheck();
    }

    private void FixedUpdate()
    {
        if (IsCameraMove)
        {
            RotateCamera();
        }
        ZoomCamera();
    }

    /// <summary>初期化時に参照漏れがないかを確認する処理を記述する</summary>
    private void ReferenceCheck()
    {
        if (_vCam == null) throw new System.NullReferenceException("VirtualCamera is not found");

        Camera mainCamera = FindObjectOfType<Camera>();
        if (mainCamera != null && mainCamera.TryGetComponent(out CinemachineBrain brain).Invert())
        {
            mainCamera.gameObject.AddComponent<CinemachineBrain>();
        }
    }

    private void RotateCamera()
    {
        if (MoveDirection.x == 0)
        {
            Vector3 vec = _vCam.transform.position;
            vec.y += MoveDirection.y * (1.0f / 50.0f);
            _vCam.transform.position = vec;
        }
        else if (MoveDirection.y == 0)
        {
            _vCam.transform.RotateAround(_center, Vector3.up, MoveDirection.x * _cameraMoveSpeed);
        }
    }

    private void ZoomCamera()
    {
        _vCam.m_Lens.FieldOfView = Mathf.Clamp(
            _vCam.m_Lens.FieldOfView - Input.mouseScrollDelta.y * _cameraZoomSpeed,
            _cameraFovMin,
            _cameraFovMax);
    }
}
