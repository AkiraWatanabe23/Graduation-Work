using UnityEngine;
using VTNConnect;

/// <summary> インゲームの各場面でのロジックを統括するクラス </summary>
public class GameLogicSupervisor : SingletonMonoBehaviour<GameLogicSupervisor>, IVantanConnectEventReceiver
{
    [SerializeField, Tooltip("何段、ジェンガを生成するか")]
    private int _floorLevel = 10;
    [SerializeField, Tooltip("1段当たりのジェンガの個数")]
    private int _itemsPerLevel = 3;

    [SerializeField]
    private NetworkPresenter _networkPresenter = default;

    [SerializeField]
    private JengaController _jengaCtrl = new();
    [SerializeField]
    private GameTurnController _turnCtrl = new();
    [SerializeField]
    private MaterialController _matCtrl = new();
    [SerializeField]
    private RoomController _roomCtrl = new();

    private DataContainer _dataContainer = default;

    public NetworkPresenter NetworkPresenter => _networkPresenter;

    public JengaController JengaCtrl => _jengaCtrl;
    public GameTurnController TurnCtrl => _turnCtrl;
    public MaterialController MatCtrl => _matCtrl;

    public DataContainer DataContainer => _dataContainer;

    public bool IsPlayableTurn => _turnCtrl.IsPlayableTurn;
    public bool IsGameStart => _turnCtrl.IsGameStart;

    public bool IsActive => true;

    protected override bool DontDestroyOnLoad => false;

    private void Start()
    {
        VantanConnect.RegisterEventReceiver(this);
        _networkPresenter?.Initialize();

        Initialize();
        //Fade.Instance.StartFadeIn().OnComplete(() => AudioManager.Instance.PlayBGM(BGMType.Title));
    }

    private void Initialize()
    {
        _dataContainer = new(_floorLevel, _itemsPerLevel, _jengaCtrl.BlockPrefab);

        var input = FindObjectOfType<ObjectSelector>();
        if (input != null) { input.Initialize(_dataContainer, _networkPresenter); }

        _jengaCtrl.Initialize(_dataContainer, _networkPresenter);
        _turnCtrl.Initialize(_dataContainer, _networkPresenter?.Model);
        _matCtrl.Initialize(_dataContainer, _networkPresenter?.Model);
        _roomCtrl.Initialize(_networkPresenter?.Model);

#if !UNITY_EDITOR
        VantanConnect.SystemReset();
        VantanConnect.GameStart((VC_StatusCode code) =>
        {
            Debug.Log($"GameStart {code}");
        });
#endif
    }

    private void Update()
    {
        _jengaCtrl.Update();
    }

    public void PlayTurnIndexSetting(int index) => _turnCtrl.PlayTurnIndexSetting(index);

    public void OnEventCall(EventData data)
    {
        switch (data.EventCode)
        {
            case EventDefine.Cheer: break;
        }
    }
}
