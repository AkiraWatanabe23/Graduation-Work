using Network;
using UnityEngine;

using Debug = Constants.ConsoleLogs;
using NetworkView = Network.NetworkView;

public class NetworkPresenter : MonoBehaviour
{
    [SerializeField]
    private NetworkView _networkView = default;

    [ReadOnly]
    [SerializeField]
    private string _playerID = "";
    [SerializeField]
    private NetworkModel _networkModel = new();

    [Header("Model`s Parameter")]
    [Range(2, 10)]
    [Tooltip("同時プレイ可能人数")]
    [SerializeField]
    private int _playersCount = 2;

    private string[] _otherPlayersIPAddress = default;


    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        _networkModel.Initialize(_playersCount);
        _networkView.Initialize();

        SelfRequest(RequestType.GenerateID);
    }

    /// <summary> 各プレイヤーのIPAddressを取得する </summary>
    private void SetPlayersIPAddress()
    {
        _otherPlayersIPAddress = new string[_playersCount - 1];
        for (int i = 0; i < _playersCount; i++)
        {
            _otherPlayersIPAddress[i] = _networkView.IPAddressFields[i].text.Trim();
        }
    }

    public void DevelopmentPasswordApply()
    {
        if (!_networkModel.UnlockDevelopMode(_networkView.PasswordField.text))
        {
            Debug.Log("password is not correct");
            _networkView.PasswordField.text = "";

            return;
        }

        _networkModel.SetActivate(_networkView.DeveloperPanel, true);
    }

    private async void SelfRequest(RequestType requestType)
    {
        var requestResult = await _networkModel.ReceiveSelfRequest(_playerID, requestType.ToString());
        Debug.Log(requestResult);
        if (_playerID == "")
        {
            _playerID = requestResult;
            Debug.Log($"PlayerIDが発行されました : {_playerID}");
        }
    }

    public async void SelfRequest(RequestButton request)
    {
        var requestResult = await _networkModel.ReceiveSelfRequest(_playerID, request.RequestType.ToString());
        if (_playerID == "")
        {
            _playerID = requestResult;
            Debug.Log($"PlayerIDが発行されました : {_playerID}");
        }
    }

    public async void SendPostRequest(RequestButton request)
    {
        //「誰が」「何をしたいか」を送信する
        var form = new WWWForm();
        form.AddField("UserID", _playerID);
        form.AddField("RequestMessage", request.RequestType.ToString());

        var requestResult = await _networkModel.SendPostRequest(form);
        Debug.Log(requestResult);
    }

    public async void SendPutRequest(RequestButton request, params string[] parameters)
    {
        var requestResult = await _networkModel.SendPutRequest($"{_playerID},{string.Join(",", parameters)}", request.RequestType.ToString());
        Debug.Log(requestResult);
    }
}
