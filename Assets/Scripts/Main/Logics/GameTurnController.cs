using Network;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VTNConnect;

[Serializable]
public class GameTurnController
{
    [SerializeField]
    private Text _gameTurnText = default;
    [SerializeField]
    private UnityEvent _gameStartEvent = default;

    [Header("Result")]
    [SerializeField]
    private GameObject _resultObj = default;
    [SerializeField]
    private Text _resultText = default;

    [ReadOnly]
    [SerializeField]
    private bool _isGameStart = false;

    private int _playTurnIndex = 0;
    private bool _isPlayableTurn = false;

    protected Action OnNextTurn { get; private set; }

    /// <summary> 自分の番かどうか </summary>
    public bool IsPlayableTurn => _isPlayableTurn;
    public bool IsGameStart => _isGameStart;

    /// <summary> 初期化処理 </summary>
    public void Initialize(DataContainer container, NetworkModel model)
    {
        OnNextTurn = () =>
        {
            container.NextTurn();
            //自分の番が回ってきたかの判定
            _isPlayableTurn = container.CurrentTurn % 3 == _playTurnIndex;
        };

        model.RegisterEvent(RequestType.ChangeTurn, ChangeTurn);
        model.RegisterEvent(RequestType.GameFinish, GameFinish);
    }

    public void PlayTurnIndexSetting(int index) => _playTurnIndex = index;

    private async Task<string> ChangeTurn(string _)
    {
        await MainThreadDispatcher.RunAsync(async () =>
        {
            OnNextTurn?.Invoke();

            Debug.Log(_isGameStart);
            if (!_isGameStart)
            {
                _isGameStart = true;
                _gameStartEvent?.Invoke();

                AudioManager.Instance.PlayBGM(BGMType.InGame);
            }
            _gameTurnText.text = _isPlayableTurn ? "Play Turn" : "Other's Turn";

            await Task.Yield();
            return "Request Success";
        });

        return "Request Success";
    }

    /// <summary> ゲーム終了時に送信される(ゲームに負けたプレイヤーが送信する) </summary>
    /// <param name="requestData"></param>
    /// <returns></returns>
    private async Task<string> GameFinish(string _)
    {
        await MainThreadDispatcher.RunAsync(async () =>
        {
            //ゲーム終了のメッセージがきたとき、自分のターンかどうか調べる

#if !UNITY_EDITOR
            //VantanConnect対応 ==========================================
            EventData data = new(EventDefine.JengaInfo);
            data.DataPack("GameFinish", _isPlayableTurn);
            VantanConnect.SendEvent(data);
            // ===========================================================
#endif

            //todo : 以下勝敗による演出等々
            _resultObj.SetActive(true);
            if (!_isPlayableTurn)
            {
                AudioManager.Instance.PlayBGM(BGMType.ResultWin);
                _resultText.text = "You Win!!!";
            }
            else
            {
                AudioManager.Instance.PlayBGM(BGMType.ResultLose);
                _resultText.text = "You Lose...";
            }

            await Task.Yield();
            return "Request Success";
        });

        return "Request Success";
    }
}
