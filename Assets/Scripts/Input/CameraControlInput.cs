using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>マウスからの入力を検知して、カメラの動きを制御する</summary>
public class CameraControlInput : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [SerializeField, Tooltip("カメラの回転方向")]
    private Vector2 _rotateDirection = Vector2.zero;
    [SerializeField, Tooltip("このオブジェクトとマウスポインターが重なったときの色")]
    private Color _selectColor = Color.white;
    [SerializeField, Tooltip("このオブジェクトが押されたときの色")]
    private Color _pushColor = Color.white;

    private CameraControlReceiver _receiver = null;
    private Image _myImage = null;

    private void Start()
    {
        if (transform.parent && transform.parent.TryGetComponent(out _receiver)) ;
        else throw new System.NullReferenceException($"{_receiver.GetType().Name} is not found");

        if (TryGetComponent(out _myImage)) ;
        else throw new System.NullReferenceException();
    }

    public void OnPointerDown(PointerEventData eventData)   // マウスが押されているとき
    {
        _receiver.IsCameraMove = true;
        _receiver.RotateDirection = _rotateDirection;
    }

    public void OnPointerUp(PointerEventData eventData)     // マウスが離されたとき
    {
        _receiver.IsCameraMove = false;
    }

    public void OnPointerExit(PointerEventData eventData)   // マウスが検知範囲外に出たとき
    {
        _receiver.IsCameraMove = false;
    }
}
