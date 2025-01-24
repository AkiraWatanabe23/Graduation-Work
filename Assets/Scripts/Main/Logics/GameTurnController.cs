using Network;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[Serializable]
public class GameTurnController
{
    [SerializeField]
    private Text _gameTurnText = default;
    [SerializeField]
    private UnityEvent _events = default;

    private int _playTurnIndex = 0;
    private bool _isPlayableTurn = false;

    [SerializeField]
    private bool _isGameStart = false;

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
                _events?.Invoke();
            }
            _gameTurnText.text = _isPlayableTurn ? "Play Turn" : "Other's Turn";
            Debug.Log(_isGameStart);

            await Task.Yield();
            return "Request Success";
        });

        return "Request Success";
    }
}
