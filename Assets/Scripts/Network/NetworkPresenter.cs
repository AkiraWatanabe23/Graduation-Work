﻿using Network;
using System.Threading.Tasks;
using UnityEngine;
using Debug = Constants.ConsoleLogs;
using NetworkView = Network.NetworkView;

public class NetworkPresenter : MonoBehaviour
{
    [SerializeField]
    private NetworkView _networkView = default;
    [SerializeField]
    private NetworkModel _networkModel = new();

    /// <summary> リクエスト処理を実行中かどうか </summary>
    private bool _isRequesting = false;

    private static string[] _otherPlayersIPAddress = default;

    public NetworkModel Model => _networkModel;

    private void Update()
    {
        if (Input.GetKey(KeyCode.Escape) && Input.GetKeyDown(KeyCode.Return))
        {
            _networkView.SetActivate(_networkView.DeveloperPanel, !_networkView.DeveloperPanel.activeSelf);
        }
    }

    public void Initialize()
    {
        _networkModel.Initialize();

        _networkView.Initialize(this);
    }

    /// <summary> 各プレイヤーのIPAddressを取得する </summary>
    private void SetPlayersIPAddress()
    {
        if (_otherPlayersIPAddress != null) { Debug.Log("既にアドレスが設定済です"); return; }

        _otherPlayersIPAddress = new string[3];
        for (int i = 0; i < _otherPlayersIPAddress.Length; i++)
        {
            _otherPlayersIPAddress[i] = _networkView.AddressTexts[i].text.Trim();
            Debug.Log(_otherPlayersIPAddress[i]);
        }
    }

    public async void SelfRequest(RequestType requestType)
    {
        _ = await _networkModel.ReceiveSelfRequest(requestType.ToString());
    }

    public async Task<string> SendPostRequest(RequestType requestType)
    {
        if (_isRequesting) { return "他のリクエストを実行中です"; }

        _isRequesting = true;
        //「誰が」「何をしたいか」を送信する
        var form = new WWWForm();
        form.AddField("RequestMessage", requestType.ToString());

        SetPlayersIPAddress();
        var requestResult = await _networkModel.SendPostRequest(form, _otherPlayersIPAddress);
        Debug.Log(requestResult);

        _isRequesting = false;
        return requestResult;
    }

    public async Task<string> SendPutRequest(RequestType requestType, params string[] parameters)
    {
        if (_isRequesting) { return "他のリクエストを実行中です"; }

        _isRequesting = true;
        SetPlayersIPAddress();

        var requestResult
            = await _networkModel.SendPutRequest(
                $"{string.Join(",", parameters)}", requestType.ToString(), _otherPlayersIPAddress);
        Debug.Log(requestResult);

        _isRequesting = false;
        return requestResult;
    }

    public void DevelopmentPasswordApply()
    {
        _networkView.SetActivate(_networkView.DeveloperPanel, true);
    }

    public void AccessWaiting() => _networkModel.AccessWaiting();

    public async void GameStart()
    {
        _networkModel.RequestEvents[RequestType.ChangeTurn.ToString()]?.Invoke("");
        var request = await SendPutRequest(RequestType.ChangeTurn);

        Debug.Log(request);
    }
}
