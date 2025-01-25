using Network;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

[Serializable]
public class RoomController
{
    [Min(2)]
    [Tooltip("同時プレイ可能人数")]
    [SerializeField]
    private int _maxConnectableCount = 2;
    [ReadOnly]
    [Tooltip("自分含めたルーム内のユーザー数")]
    [SerializeField]
    private int _currentPlayersCount = 0;

    [SerializeField]
    private Text _connectionCountText = default;
    [SerializeField]
    private Text _roomIDText = default;

    /// <summary> ルーム作成時に発行されるID </summary>
    private string _roomID = "";
    /// <summary> 自分がルームを建てた人かどうか </summary>
    private bool _isHost = false;

    private Random _random = default;

    public void Initialize(NetworkModel model)
    {
        model.RegisterEvent(RequestType.CreateRoom, CreateRoom);
        model.RegisterEvent(RequestType.ExitRoom, ExitRoom);
        model.RegisterEvent(RequestType.JoinRoom, JoinRoom);

        _currentPlayersCount = 0;
        _isHost = false;
    }

    /// <summary> ルームの新規作成 </summary>
    /// <returns> 新規作成されたルームのID（他プレイヤーはこのIDをキーにしてルームを検索する） </returns>
    private async Task<string> CreateRoom(string _)
    {
        _isHost = true;
        //ルームを作成したユーザーが1人目に該当するため、直に代入
        _currentPlayersCount = 1;
        _connectionCountText.text = $"Count : {_currentPlayersCount}";

        //ルームIDを新規発行する
        _random ??= new();
        _roomID = _random.Next(0, 10000).ToString("0000");
        await Task.Yield();

        _roomIDText.text = $"RoomID : {_roomID}";

        return _roomID;
    }

    /// <summary> ルームから退出する </summary>
    /// <returns> 退出処理が正常に行われたらSuccess </returns>
    private async Task<string> ExitRoom(string _)
    {
        if (!_isHost) { return "I'm not room host"; }
        _currentPlayersCount--;

        await Task.Yield();
        return "Exit Success";
    }

    /// <summary> 作成済のルームに対する参加リクエスト </summary>
    /// <param name="requestData"> ルーム参加に必要なデータ（PlayerID, RoomID） </param>
    /// <returns> ルーム参加が正常に行われたら自分が何番目のユーザーかを返す </returns>
    private async Task<string> JoinRoom(string requestData)
    {
        var splitData = requestData.Split(',');
        var roomID = splitData[0];

        //ルームIDが異なる or ルームが満員 → ルーム参加失敗
        if (roomID != _roomID) { Debug.Log($"RoomID is not correct. {roomID}"); return $"RoomID is not correct. {roomID}"; }
        else if (_currentPlayersCount + 1 > _maxConnectableCount) { Debug.Log("Room is full."); return "Room is full."; }

        await MainThreadDispatcher.RunAsync(async () =>
        {
            _currentPlayersCount++;
            _connectionCountText.text = $"Count : {_currentPlayersCount}";
            await Task.Yield();
            return "Count up Finish";
        });

        Debug.Log($"ルームへの参加を承認しました : 現在{_currentPlayersCount}人です");
        return (_currentPlayersCount - 1).ToString();
    }
}
