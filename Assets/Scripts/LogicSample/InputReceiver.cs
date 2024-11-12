using UnityEngine;

/// <summary> プレイヤーの入力を管理するクラスのサンプル </summary>
public class InputReceiver : MonoBehaviour
{
    [SerializeField]
    private bool _isMyTurn = false;

    private GameObject _selectedTarget = default;
    private Vector3 _inputPosition = Vector3.zero;

    private void Update()
    {
        InputHandler();
    }

    private void InputHandler()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                var go = hit.collider.gameObject;
                Debug.Log(go.name);

                //ブロックを選択したかどうかの判定を行う（仮）
                if (go.TryGetComponent(out BlockComponent _))
                {
                    go.SetActive(false);
                }
            }
            else if (_selectedTarget != null)
            {

            }
            else
            {
                //何も衝突対象がいなかった場合
            }
        }
        else if (Input.GetMouseButton(0))
        {
            //スクロール処理、ブロックの移動処理
        }
        else if (Input.GetMouseButtonUp(0))
        {
            //画面から離れた時、何をしていたかによって処理を分ける
            _selectedTarget = null;
        }
#else
#endif
    }

    /// <summary> 画面を見る角度を変える </summary>
    private void CameraScroll()
    {

    }

    /// <summary> 選択したブロックを動かす </summary>
    private void BlockSwipe()
    {
        if (!_isMyTurn || _selectedTarget == null) { return; }
    }
}
