using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using UnityEngine;
using UnityEngine.Networking;
using Debug = Constants.ConsoleLogs;

namespace Network
{
    [Serializable]
    public class NetworkModel
    {
        #region Variables
        [Tooltip("リクエストに失敗した時に再接続を行う回数")]
        [Range(0, 10)]
        [SerializeField]
        private int _rerunCount = 1;
        [Tooltip("1回あたりのリクエストの実行時間（sec）")]
        [Range(1f, 10f)]
        [SerializeField]
        private float _executionTime = 1f;
        [ReadOnly]
        [Tooltip("再起処理の実行回数")]
        [SerializeField]
        private int _runCount = 0;
        [ReadOnly]
        [SerializeField]
        private string _hostURL = "";

        private Stream _responseOutput = default;
        private HttpListener _listener = default;

        private CancellationTokenSource _cancellationTokenSource = default;
        /// <summary> 処理の実行時間を調べる </summary>
        private Stopwatch _stopWatch = default;

        private Dictionary<string, Func<string, Task<string>>> _requestEvents = default;

        private string _roomID = "";
        #endregion

        public void RegisterEvent(RequestType request, Func<string, Task<string>> func)
        {
            _requestEvents ??= new();
            var message = request.ToString();
            if (_requestEvents.ContainsKey(message)) { return; }

            _requestEvents.Add(message, func);
        }

        public void Initialize()
        {
            _cancellationTokenSource = new();
            _stopWatch = new();

            MainThreadDispatcher.SetMainThreadContext();
        }

        /// <summary> 文字列がURLとして成立しているか </summary>
        private bool IsValidURL(string candidateURL)
        {
            if (Uri.TryCreate(candidateURL, UriKind.Absolute, out Uri result))
            {
                return (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
            }
            return false;
        }

        /// <summary> 接続先のURLを作成する </summary>
        /// <param name="address"> 接続対象のIPAddress </param>
        /// <param name="port"> ポート番号 </param>
        public string CreateConnectionURL(string address, string port) => $"http://{address}:{port}/";

        #region Data Send
        /// <summary> Postリクエストを送信する </summary>
        /// <returns> 実行結果の文字列 </returns>
        public async Task<string> SendPostRequest(WWWForm form, string[] addresses, CancellationToken token = default)
        {
            int loopCount = _hostURL == "" ? addresses.Length : 1;

            for (int i = 0; i < loopCount; i++)
            {
                if (addresses[i] == "") { continue; }
                _hostURL = CreateConnectionURL(addresses[i], _roomID);
                if (!IsValidURL(_hostURL)) { Debug.Log($"URL未成立：{_hostURL}"); continue; }

                if (token == default) { token = _cancellationTokenSource.Token; }
                _stopWatch.Start();

                using UnityWebRequest request = UnityWebRequest.Post(_hostURL, form);
                var send = request.SendWebRequest();
                while (!send.isDone)
                {
                    if (token.IsCancellationRequested || _runCount >= _rerunCount) { break; }
                    if (_stopWatch.ElapsedMilliseconds >= _executionTime * 1000f)
                    {
                        if (_runCount < _rerunCount)
                        {
                            _runCount++;
                            _stopWatch.Reset();

                            return await SendPostRequest(form, addresses, _cancellationTokenSource.Token);
                        }
                        else { continue; }
                    }
                    await Task.Delay(1, token);
                }

                //送信結果の確認
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError(request.error);
                    continue;
                }
                else
                {
                    var result = request.downloadHandler.text;
                    Debug.Log($"Request Result : {result}");

                    _runCount = 0;
                    _stopWatch.Reset();
                    return result;
                }
            }
            //各URLに対してそれぞれリクエスト処理を行い、どこにも合致しなかったらリクエスト失敗
            return "None";
        }

        /// <summary> Putリクエストを送信する </summary>
        /// <returns> 実行結果の文字列 </returns>
        public async Task<string> SendPutRequest(string json, string requestMessage, string[] addresses, CancellationToken token = default)
        {
            int loopCount = _hostURL == "" ? addresses.Length : 1;
            if (int.TryParse(json, out int _)) { _roomID = json; }

            for (int i = 0; i < loopCount; i++)
            {
                if (addresses[i] == "") { continue; }
                _hostURL = CreateConnectionURL(addresses[i], _roomID);
                if (!IsValidURL(_hostURL)) { Debug.Log($"URL未成立：{_hostURL}"); continue; }

                if (token == default) { token = _cancellationTokenSource.Token; }
                _stopWatch.Start();

                Debug.Log(_hostURL);
                using UnityWebRequest request = UnityWebRequest.Put(_hostURL, Encoding.UTF8.GetBytes(json + $"^{requestMessage}"));
                var send = request.SendWebRequest();

                while (!send.isDone)
                {
                    Debug.Log("sending...");
                    if (token.IsCancellationRequested || _runCount >= _rerunCount) { break; }
                    if (_stopWatch.ElapsedMilliseconds >= _executionTime * 1000f)
                    {
                        if (_runCount < _rerunCount)
                        {
                            _runCount++;
                            _stopWatch.Reset();

                            return await SendPutRequest(json, requestMessage, addresses, _cancellationTokenSource.Token);
                        }
                        else { continue; }
                    }
                    await Task.Delay(1, token);
                }

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError(request.error);
                    continue;
                }
                else
                {
                    var result = request.downloadHandler.text;
                    Debug.Log($"Request Result : {result}");

                    _runCount = 0;
                    _stopWatch.Reset();
                    return result;
                }
            }
            //各URLに対してそれぞれリクエスト処理を行い、どこにも合致しなかったらリクエスト失敗
            return "None";
        }
        #endregion

        #region Data Recipient
        /// <summary> 通信を介さない自己完結のリクエスト処理 </summary>
        /// <param name="requestMessage"> リクエスト内容 </param>
        /// <returns> リクエストに応じた処理結果 </returns>
        public async Task<string> ReceiveSelfRequest(string requestMessage)
        {
            Debug.Log(requestMessage);

            var result = await _requestEvents[requestMessage]?.Invoke("");
            switch (requestMessage)
            {
                case "CreateRoom":
                    //ここでルームへの入室待機処理の開始を行う
                    _roomID = result;

                    string redirectURL = $"http://*:{_roomID}/";
                    Debug.Log(redirectURL);

                    _listener = new();
                    try
                    {
                        _listener.Prefixes.Add(redirectURL);
                        _listener.Start();
                        Debug.Log("Server started");
                    }
                    catch (Exception exception)
                    {
                        Debug.LogError(exception.Message);
                        return exception.Message;
                    }
                    //同時アクセスに対応できるように複数スレッドで実行する
                    for (int i = 0; i < 2; i++) { AccessWaiting(); }
                    break;
            }

            return result;
        }

        /// <summary> IDのみをキーにして何か処理を行う場合 </summary>
        public async Task<string> ReceivePostRequest(string id, string requestMessage)
        {
            Debug.Log($"{id} {requestMessage}");

            return await _requestEvents[requestMessage]?.Invoke(id);
        }

        /// <summary> ID以外のパラメータもキーにして何か処理を行う場合 </summary>
        public async Task<string> ReceivePutRequest(string data)
        {
            var parse = data.Split('^');
            var requestData = parse[0];
            var message = parse[1];

            Debug.Log($"Request : {message}");

            return await _requestEvents[message]?.Invoke(requestData);
        }
        #endregion

        public void AccessWaiting()
        {
            Task.Run(async () =>
            {
                try
                {
                    if (!_listener.IsListening) { _listener.Start(); }

                    var context = await _listener.GetContextAsync();
                    var response = context.Response;

                    // 受け取ったリダイレクトURLをログに出力する
                    Debug.Log($"redirectURI: {context.Request.Url}");

                    // 受け取ったリダイレクトURLのクエリパラメータからcodeを取得する
                    var query = context.Request.Url.Query;
                    var code = HttpUtility.ParseQueryString(query).Get("code");

                    string responseString = "";
                    //クライアントからのリクエストを判定
                    if (context.Request.HttpMethod == "POST")
                    {
                        //送信されてきたデータ配列
                        var reader = new StreamReader(context.Request.InputStream).ReadToEnd().Split(',');
                        var requestData = reader[0].Split('&');
                        var id = requestData[0].Split('=')[1];
                        var requestMessage = requestData[1].Split('=')[1];

                        //受けたリクエストに対して処理を実行する
                        responseString = await ReceivePostRequest(id, requestMessage);
                    }
                    else if (context.Request.HttpMethod == "PUT")
                    {
                        responseString = await ReceivePutRequest(new StreamReader(context.Request.InputStream).ReadToEnd());
                    }

                    var buffer = Encoding.UTF8.GetBytes(responseString);
                    response.ContentLength64 = buffer.Length;
                    _responseOutput = response.OutputStream;
                    await _responseOutput.WriteAsync(buffer, 0, buffer.Length);

                    //複数端末からの処理を待機するために再起実行
                    AccessWaiting();
                }
                catch (Exception exception) { Debug.LogError(exception.Message); }
            });
        }

        /// <summary> 実行中のリクエスト処理を中断する </summary>
        private string RequestSuspended()
        {
            Debug.Log("送信中のリクエストを中断します");
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource?.Dispose();

            _cancellationTokenSource = new();

            return "Suspend Success";
        }
    }

    public enum RequestType
    {
        None,
        //Self
        CreateRoom,
        RequestSuspended,
        //Post
        ExitRoom,
        //Put
        JoinRoom,
        ChangeTurn,
        ChangeMaterial,
        SelectBlock,
        PlaceBlock,
    }
}
