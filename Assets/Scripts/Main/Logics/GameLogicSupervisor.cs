using UnityEngine;
using VTNConnect;

/// <summary> インゲームの各場面でのロジックを統括するクラス </summary>
public class GameLogicSupervisor : MonoBehaviour, IVantanConnectEventReceiver
{
    [SerializeField]
    private NetworkPresenter _networkPresenter = default;

    [SerializeField]
    private GameTurnController _turnCtrl = new();
    [SerializeField]
    private MaterialController _matCtrl = new();

    private DataContainer _dataContainer = default;

    public bool IsPlayableTurn => _turnCtrl.IsPlayableTurn;

    public bool IsActive => true;

    private void Start()
    {
        VantanConnect.RegisterEventReceiver(this);
        Initialize();
    }

    private void Initialize()
    {
        _dataContainer = new();

        var input = FindObjectOfType<ObjectSelector>();
        if (input != null) { input.Initialize(_dataContainer); }

        _turnCtrl.Initialize(_dataContainer);
        _matCtrl.Initialize(_dataContainer);
    }

    public void OnEventCall(EventData data)
    {
        switch (data.EventCode)
        {
            case EventDefine.Cheer: break;
        }
    }
}
