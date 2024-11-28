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

        public GameObject DeveloperPanel => _developerPanel;
        public InputField PasswordField => _passwordField;
        public InputField[] IPAddressFields => _ipAddressFields;

        public void Initialize()
        {

        }
    }
}
