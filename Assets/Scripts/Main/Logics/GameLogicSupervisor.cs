using UnityEngine;

/// <summary> インゲームの各場面でのロジックを統括するクラス </summary>
public class GameLogicSupervisor : MonoBehaviour
{
    [SerializeField]
    private MaterialController _matCtrl = new();

    private DataContainer _dataContainer = default;

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        _dataContainer = new();

        var input = FindObjectOfType<ObjectSelector>();
        if (input != null) { input.Initialize(_dataContainer); }

        _matCtrl.Initialize(_dataContainer);
    }
}
