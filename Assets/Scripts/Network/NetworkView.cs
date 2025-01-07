using UnityEngine;
using UnityEngine.UI;

namespace Network
{
    public class NetworkView : MonoBehaviour
    {
        [Header("=== Join Room ===")]
        [SerializeField]
        private InputField _roomIDField = default;
        [SerializeField]
        private RequestButton _joinRequestButton = default;
        [SerializeField]
        private Button _applyButton = default;

        [Header("=== For Developer ===")]
        [SerializeField]
        private GameObject _developerPanel = default;
        [SerializeField]
        private InputField[] _ipAddressFields = default;

        public GameObject DeveloperPanel => _developerPanel;
        public InputField[] IPAddressFields => _ipAddressFields;

        public void Initialize(NetworkPresenter presenter)
        {
            _applyButton.onClick.AddListener(() =>
            {
                presenter.PassingRoomID(_roomIDField.text.Trim());
                presenter.SendPutRequest(_joinRequestButton, _roomIDField.text);
            });
        }

        /// <summary> 表示するテキスト内容の更新 </summary>
        public void OnUpdateText(InputField target, string message) => target.text = message;

        /// <summary> 表示するテキスト内容の更新 </summary>
        public void OnUpdateText(Text target, string message) => target.text = message;

        /// <summary> UIオブジェクトの表示、非表示の更新 </summary>
        public void SetActivate(GameObject target, bool activate) => target.SetActive(activate);
    }
}
