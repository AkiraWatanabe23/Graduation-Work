using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using UnityEngine;
using UnityEngine.Networking;

using Debug = Constants.ConsoleLogs;
using Random = System.Random;

namespace Network
{
    [Serializable]
    public class NetworkModel
    {
        #region Variables
        [Tooltip("開発者ページをアンロックするためのパスワード")]
        [SerializeField]
        private string _developerPassword = "";
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

        /// <summary> 同時プレイ可能人数 </summary>
        private int _maxConnectableCount = 0;
        private string _roomID = "";
        private List<string> _roomPlayers = default;

        private Random _random = default;

        private const int UserIDLength = 8;
        /// <summary> UserIDに使用される文字 </summary>
        private const string CharLine = "abcdefghijklmnopqrstuvwxyz0123456789";

        /// <summary> 自分を含めた現在の接続数 </summary>
        protected int CurrentConnectionCount => _roomPlayers == null ? 0 : _roomPlayers.Count;

        public string SelfIPAddress { get; private set; }
        #endregion

        public void Initialize(int maxConnectableCount)
        {
            _maxConnectableCount = maxConnectableCount;
            SelfIPAddress = GetSelfIPAddress();

            _cancellationTokenSource = new();
            _stopWatch = new();
        }

        /// <summary> 自分のマシンのIPアドレスを取得する </summary>
        private string GetSelfIPAddress()
        {
            string hostname = Dns.GetHostName();
            return Dns.GetHostAddresses(hostname)[1].ToString();
        }

        public void ReceiveRoomID(string id) => _roomID = id;

        /// <summary> パスワードのマッチ確認 </summary>
        /// <param name="enteredWord"> 入力された文字列 </param>
        /// <returns> 入力がパスワードとマッチしているかどうか </returns>
        public bool UnlockDevelopMode(string enteredWord) => _developerPassword == enteredWord;

        /// <summary> 接続先のURLを作成する </summary>
        /// <param name="address"> 接続対象のIPAddress </param>
        /// <param name="port"> ポート番号 </param>
        public string CreateConnectionURL(string address, string port) => $"http://{address}:{port}/";

        public void SetActivate(GameObject target, bool activate) => target.SetActive(activate);

        /// <summary> リクエストに対する一連の処理が正常に流れた時に返す文字列 </summary>
        private const string Success = "Request Success";

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

                if (token == default) { token = _cancellationTokenSource.Token; }
                _stopWatch.Start();

                using UnityWebRequest request = UnityWebRequest.Post(_hostURL, form);
                var send = request.SendWebRequest();
                while (!send.isDone)
                {
                    if (token.IsCancellationRequested) { break; }
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

            for (int i = 0; i < loopCount; i++)
            {
                if (addresses[i] == "") { continue; }
                _hostURL = CreateConnectionURL(addresses[i], _roomID);

                if (token == default) { token = _cancellationTokenSource.Token; }
                _stopWatch.Start();

                using UnityWebRequest request = UnityWebRequest.Put(_hostURL, Encoding.UTF8.GetBytes(json + $"^{requestMessage}"));
                var send = request.SendWebRequest();

                while (!send.isDone)
                {
                    if (token.IsCancellationRequested) { break; }
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
        /// <param name="id"> 自分のID </param>
        /// <param name="requestMessage"> リクエスト内容 </param>
        /// <returns> リクエストに応じた処理結果 </returns>
        public async Task<string> ReceiveSelfRequest(string id, string requestMessage)
        {
            Debug.Log(requestMessage);
            return requestMessage switch
            {
                "CreateRoom" => CreateRoom(id),
                "GenerateID" => await GenerateID(),
                _ => ""
            };
        }

        /// <summary> IDのみをキーにして何か処理を行う場合 </summary>
        public async Task<string> ReceivePostRequest(string id, string requestMessage)
        {
            Debug.Log($"{id} {requestMessage}");
            return requestMessage switch
            {
                "ExitRoom" => await ExitRoom(id),
                _ => ""
            };
        }

        /// <summary> ID以外のパラメータもキーにして何か処理を行う場合 </summary>
        public async Task<string> ReceivePutRequest(string data)
        {
            var parse = data.Split('^');
            var requestData = parse[0];
            var message = parse[1];

            Debug.Log($"Request : {message}");
            return message switch
            {
                "JoinRoom" => JoinRoom(requestData),
                "PlayAudio" => await PlayAudio(),
                _ => ""
            };
        }
        #endregion

        #region Self Request
        /// <summary> ルームの新規作成 </summary>
        /// <param name="hostID"> ルームのホストを担うプレイヤーのID </param>
        /// <returns> 新規作成されたルームのID（他プレイヤーはこのIDをキーにしてルームを検索する） </returns>
        private string CreateRoom(string hostID)
        {
            if (CurrentConnectionCount > 0) { return "Room is already exist."; }

            _roomPlayers ??= new();

            _roomPlayers.Add(hostID);
            //ルームIDを新規発行する
            _random ??= new();
            _roomID = _random.Next(0, 10000).ToString("F4");
            _hostURL = CreateConnectionURL(SelfIPAddress, _roomID);

            //ここでルームへの入室待機処理の開始を行う
            string redirectURL = $"http://*:{_roomID}/";

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
            for (int i = 0; i < _maxConnectableCount - 1; i++) { AccessWaiting(); }

            return _roomID;
        }

        private void AccessWaiting()
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

        /// <summary> IDの新規生成 </summary>
        private async Task<string> GenerateID()
        {
            _random ??= new();
            string newID = "";
            await Task.Run(() =>
            {
                newID = new string(Enumerable.Repeat(CharLine, UserIDLength)
                                    .Select(selected => selected[_random.Next(selected.Length)])
                                    .ToArray());
            });
            return newID;
        }
        #endregion

        #region Post Request
        /// <summary> ルームから退出する </summary>
        /// <param name="id"> 退出するプレイヤーのID </param>
        /// <returns> 退出処理が正常に行われたらSuccess </returns>
        private async Task<string> ExitRoom(string id)
        {
            //渡されたIDのプレイヤーがルームに存在しない場合、失敗
            if (!_roomPlayers.Contains(id)) { return $"You`re not exist in this room : {_roomID}"; }

            _roomPlayers.Remove(id);

            await Task.Yield();
            return Success;
        }
        #endregion

        #region Put Request
        /// <summary> 作成済のルームに対する参加リクエスト </summary>
        /// <param name="requestData"> ルーム参加に必要なデータ（PlayerID, RoomID） </param>
        /// <returns> ルーム参加が正常に行われたらSuccess </returns>
        private string JoinRoom(string requestData)
        {
            var splitData = requestData.Split(',');
            var playerID = splitData[0];
            var roomID = splitData[1];

            //現在プレイ中 or ルームIDが異なる or ルームが満員→ルーム参加拒否
            if (roomID != _roomID) { return "RoomID is not correct."; }
            else if (_roomPlayers.Count + 1 > _maxConnectableCount) { return "Room is full."; }

            _roomPlayers?.Add(playerID);
            return Success;
        }

        /// <summary> プレイヤーのアクションに応じた音の再生を行う </summary>
        /// <returns> 再生が正常に行われたらSuccess </returns>
        private async Task<string> PlayAudio()
        {
            await Task.Yield();
            return Success;
        }
        #endregion
    }

    public enum RequestType
    {
        None,
        //Self
        CreateRoom,
        GenerateID,
        //Post
        ExitRoom,
        //Put
        JoinRoom,
        PlayAudio,
    }
}
