using Network;
using System.Threading.Tasks;
using UnityEngine;

public class InGameLogic
{
    private InGameData _inGameData = default;

    public string RoomID => _inGameData.RoomID;
    public int MaxConnectableCount => _inGameData.MaxConnectableCount;

    /// <summary> リクエストに対する一連の処理が正常に流れた時に返す文字列 </summary>
    private const string Success = "Request Success";

    public void Initialize(NetworkModel model, int maxConnectableCount)
    {
        _inGameData = new(maxConnectableCount);

        //各場面の処理の登録を行う
        model.RegisterEvent(RequestType.ExitRoom, ExitRoom);

        model.RegisterEvent(RequestType.JoinRoom, JoinRoom);
        model.RegisterEvent(RequestType.ChangeTurn, ChangeTurn);
        model.RegisterEvent(RequestType.ChangeMaterial, ChangeMaterial);
        model.RegisterEvent(RequestType.SelectBlock, SelectBlock);
        model.RegisterEvent(RequestType.PlaceBlock, PlaceBlock);
        model.RegisterEvent(RequestType.RecalculateTowerState, RecalculateTowerState);
    }

    #region Passing NetworkModel
    public void ReceiveRoomID(string id) => _inGameData.ReceiveRoomID(id);

    public void CreateRoom(string hostID)
    {
        _inGameData.RoomPlayers.Clear();
        _inGameData.AddPlayer(hostID);
    }
    #endregion

    #region Post Request
    /// <summary> ルームから退出する </summary>
    /// <param name="id"> 退出するプレイヤーのID </param>
    /// <returns> 退出処理が正常に行われたらSuccess </returns>
    private async Task<string> ExitRoom(string id)
    {
        //渡されたIDのプレイヤーがルームに存在しない場合、失敗
        if (!_inGameData.RoomPlayers.Contains(id)) { return $"You`re not exist in this room : {_inGameData.RoomID}"; }

        _inGameData.RemovePlayer(id);

        await Task.Yield();
        return Success;
    }
    #endregion

    #region Put Request
    /// <summary> 作成済のルームに対する参加リクエスト </summary>
    /// <param name="requestData"> ルーム参加に必要なデータ（PlayerID, RoomID） </param>
    /// <returns> ルーム参加が正常に行われたらSuccess </returns>
    private async Task<string> JoinRoom(string requestData)
    {
        var splitData = requestData.Split(',');
        var playerID = splitData[0];
        var roomID = splitData[1];

        //ルームIDが異なる or ルームが満員 or 現在プレイ中 → ルーム参加失敗
        if (roomID != _inGameData.RoomID) { return $"RoomID is not correct. {roomID}"; }
        else if (_inGameData.RoomPlayers.Count + 1 > _inGameData.MaxConnectableCount) { return "Room is full."; }
        else if (_inGameData.IsGamePlaying) { return "Game Playing"; }

        await Task.Yield();
        _inGameData.AddPlayer(playerID);
        Debug.Log($"PlayersCount : {_inGameData.RoomPlayers.Count}");
        return Success;
    }

    private async Task<string> ChangeTurn(string requestData)
    {
        await Task.Yield();
        return Success;
    }

    private async Task<string> ChangeMaterial(string requestData)
    {
        var splitData = requestData.Split(',');
        var playerID = splitData[0];
        var id = int.Parse(splitData[1]);
        var material = splitData[2];

        //===============================================================================
        //todo : ここで更新処理を行う
        //
        //_blocks[id].material = material; のようなイメージ
        //===============================================================================

        await Task.Yield();
        return Success;
    }

    private async Task<string> SelectBlock(string requestData)
    {
        var splitData = requestData.Split(',');
        var playerID = splitData[0];
        var id = int.Parse(splitData[1]);
        var material = splitData[2];

        //===============================================================================
        //todo : ここで更新処理を行う
        //
        //_currentTarget = _blocks[id]; のようなイメージ
        //===============================================================================

        await Task.Yield();
        return Success;
    }

    private async Task<string> PlaceBlock(string requestData)
    {
        var splitData = requestData.Split(',');
        var playerID = splitData[0];
        var id = int.Parse(splitData[1]);
        var material = splitData[2];

        //===============================================================================
        //todo : ここで更新処理を行う
        //===============================================================================

        await Task.Yield();
        return Success;
    }

    /// <summary> ブロックの更新、それに伴う倒壊率の再計算 </summary>
    /// <param name="requestData"> 更新するデータ群 </param>
    /// <returns> 処理が正常に行われたらSuccess </returns>
    private async Task<string> RecalculateTowerState(string requestData)
    {
        var splitData = requestData.Split(',');

        //===============================================================================
        //todo : ここで更新処理、計算処理を行う
        //===============================================================================

        await Task.Yield();
        return Success;
    }
    #endregion
}
