using UnityEngine;
using UnityEngine.UI;

namespace Network
{
    public class NetworkView : MonoBehaviour
    {
        [Header("For Developer")]
        [SerializeField]
        private GameObject _developerPanel = default;
        [SerializeField]
        private InputField _passwordField = default;
        [SerializeField]
        private InputField[] _ipAddressFields = default;

        [field: SerializeField]
        public GameObject PasswordPanel { get; private set; }

        [Header("Join Room")]
        [SerializeField]
        private InputField _roomIDField = default;
        [SerializeField]
        private RequestButton _joinRequestButton = default;
        [SerializeField]
        private Button _applyButton = default;

        public GameObject DeveloperPanel => _developerPanel;
        public InputField PasswordField => _passwordField;
        public InputField[] IPAddressFields => _ipAddressFields;

        public void Initialize(NetworkPresenter presenter)
        {
            _applyButton?.onClick.AddListener(() =>
            {
                presenter.PassingRoomID(_roomIDField.text.Trim());
                presenter.SendPutRequest(_joinRequestButton, _roomIDField.text);
            });
        }
    }
}
