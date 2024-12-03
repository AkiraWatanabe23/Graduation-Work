using Network;
using System;
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

    public string PlayerID => _playerID;

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        _networkModel.Initialize(_playersCount);
        SelfRequest(RequestType.GenerateID);

        _networkView.Initialize(this);
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

        SetPlayersIPAddress();
        var requestResult = await _networkModel.SendPostRequest(form, _otherPlayersIPAddress);
        Debug.Log(requestResult);
    }

    public async void SendPutRequest(RequestButton request, params string[] parameters)
    {
        SetPlayersIPAddress();
        var requestResult
            = await _networkModel.SendPutRequest(
                $"{_playerID},{string.Join(",", parameters)}", request.RequestType.ToString(), _otherPlayersIPAddress);
        Debug.Log(requestResult);
    }

    public void DevelopmentPasswordApply()
    {
        if (!_networkModel.UnlockDevelopMode(_networkView.PasswordField.text))
        {
            Debug.Log("password is not correct");
            _networkView.PasswordField.text = "";

            return;
        }

        _networkModel.SetActivate(_networkView.PasswordPanel, false);
        _networkModel.SetActivate(_networkView.DeveloperPanel, true);
    }

    public void PassingRoomID(string id) => _networkModel.ReceiveRoomID(id);
}
