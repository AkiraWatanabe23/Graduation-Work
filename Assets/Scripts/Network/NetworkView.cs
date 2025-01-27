using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Network
{
    public class NetworkView : MonoBehaviour
    {
        [SerializeField]
        private Button _applyButton = default;
        [SerializeField]
        private Button _createRoomButton = default;

        [SerializeField]
        private Text _roomIDText = default;
        [ReadOnly]
        [SerializeField]
        private Text _targetAddressText = default;

        [Header("=== For Developer ===")]
        [SerializeField]
        private GameObject _developerPanel = default;
        [SerializeField]
        private Text[] _addressTexts = default;

        public GameObject DeveloperPanel => _developerPanel;
        public Text[] AddressTexts => _addressTexts;

        public void Initialize(NetworkPresenter presenter)
        {
            _applyButton.onClick.AddListener(async () =>
            {
                if (_roomIDText.text.Length != 4 || !int.TryParse(_roomIDText.text, out int _))
                {
                    Debug.Log("入力されたIDが正常ではありません");
                    return;
                }

                var result = await presenter.SendPutRequest(RequestType.JoinRoom, _roomIDText.text.Trim());
                if (int.TryParse(result, out int value))
                {
                    GameLogicSupervisor.Instance.PlayTurnIndexSetting(value);

                    await Task.Yield();
                    presenter.AccessWaiting();
                }
            });

            _createRoomButton.onClick.AddListener(() =>
            {
                presenter.SelfRequest(RequestType.CreateRoom);
            });
        }

        #region UI Setting
        public void AddressTextSetting(Text text) => _targetAddressText = text;

        public void InputAddressPad(string text)
        {
            if (_targetAddressText == null) { return; }

            if (!int.TryParse(text, out int _) && text.Length > 1)
            {
                _targetAddressText.text = "";
                return;
            }

            _targetAddressText.text += text;
        }

        public void InputNumberPad(string text)
        {
            if (_roomIDText == null) { return; }

            if (!int.TryParse(text, out int _))
            {
                _roomIDText.text = "";
                return;
            }
            if (_roomIDText.text.Length == 4) { return; }

            _roomIDText.text += text;
        }
        #endregion

        /// <summary> 表示するテキスト内容の更新 </summary>
        public void OnUpdateText(InputField target, string message) => target.text = message;

        /// <summary> 表示するテキスト内容の更新 </summary>
        public void OnUpdateText(Text target, string message) => target.text = message;

        /// <summary> UIオブジェクトの表示、非表示の更新 </summary>
        public void SetActivate(GameObject target, bool activate) => target.SetActive(activate);
    }
}
