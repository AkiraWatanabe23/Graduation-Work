using DG.Tweening;
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
    private Image _gameTurnImage = default;
    [SerializeField]
    private Text _gameTurnText = default;
    [SerializeField]
    private UnityEvent _gameStartEvent = default;

    [Header("Result")]
    [SerializeField]
    private GameObject _resultObj = default;
    [SerializeField]
    private Text _resultText = default;
    [SerializeField]
    private Button _returnTitleButton = default;

    [ReadOnly]
    [SerializeField]
    private bool _isGameStart = false;

    private int _playTurnIndex = 0;
    private bool _isPlayableTurn = false;

    /// <summary> リクエスト重複を防ぐためのフラグ </summary>
    private bool _isGameFinish = false;

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
            _isPlayableTurn = container.CurrentTurn % GameLogicSupervisor.Instance.MaxConnectableCount == _playTurnIndex;
            if (_isPlayableTurn) { AudioManager.Instance.PlaySE(SEType.MyTurn); }
        };

        container.GameFinishRegister(() =>
        {
            _resultObj.SetActive(true);
            AudioManager.Instance.StopBGM();

            _resultText.text = "";
            if (!GameLogicSupervisor.Instance.IsWinning)
            {
                Debug.Log("You Win!!!");
                _resultText.DOText("You Win!!!", 1.5f).OnComplete(() => AudioManager.Instance.PlayBGM(BGMType.ResultWin));
            }
            else
            {
                Debug.Log("You Lose...");
                _resultText.DOText("You Lose...", 1.5f).OnComplete(() => AudioManager.Instance.PlayBGM(BGMType.ResultLose));
            }
        });

        model.RegisterEvent(RequestType.ChangeTurn, ChangeTurn);
        model.RegisterEvent(RequestType.GameFinish, GameFinish);
    }

    public void PlayTurnIndexSetting(int index) => _playTurnIndex = index;

    private async Task<string> ChangeTurn(string _)
    {
        await MainThreadDispatcher.RunAsync(async () =>
        {
            OnNextTurn?.Invoke();

            if (!_isGameStart)
            {
                AudioManager.Instance.PlaySE(SEType.GameStart);
                _isGameStart = true;
                _gameStartEvent?.Invoke();

                AudioManager.Instance.PlayBGM(BGMType.InGame);
            }
            _gameTurnText.text = _isPlayableTurn ? "あなたの番です" : "他のプレイヤーの番です";

            var sequence = DOTween.Sequence();
            sequence.
                Append(_gameTurnImage.transform.DOScale(1.2f, 1f)).
                AppendInterval(0.25f).
                AppendCallback(() =>
                {
                    _gameTurnImage.transform.DOScale(1f, 1f);
                });


            await Task.Yield();
            return "Request Success";
        });

        return "Request Success";
    }

    /// <summary> ゲーム終了時に送信される(ゲームに負けたプレイヤーが送信する) </summary>
    private async Task<string> GameFinish(string turn)
    {
        await MainThreadDispatcher.RunAsync(async () =>
        {
            if (_isGameFinish) { return ""; }

            _isGameFinish = true;

            //ゲーム終了のメッセージがきたとき、自分のターンかどうか調べる
            //VantanConnect対応 ==========================================
            EventData data = new(EventDefine.BadJengaInfo);
            data.DataPack("JengaFinish", int.Parse(turn));
            VantanConnect.SendEvent(data);
            // ===========================================================

            _returnTitleButton.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlaySE(SEType.ClickButton);
                //以下引数のbool値は勝ち、負けで分ける
                VantanConnect.GameEnd(GameLogicSupervisor.Instance.IsWinning, (VC_StatusCode code) =>
                {
                    SceneLoader.FadeLoad(SceneName.InGame);
                });
            });

            await Task.Yield();
            return "Request Success";
        });

        return "Request Success";
    }
}
