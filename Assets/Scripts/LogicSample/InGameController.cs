using UnityEngine;
using UnityEngine.UI;

namespace LogicSample
{
    public class InGameController : MonoBehaviour
    {
        [SerializeField]
        private BlockSystem _blockSystem = new();
        [SerializeField]
        private Button _calcButton = default;

        private void Start()
        {
            _blockSystem.Initialize();
            if (_calcButton != null)
            {
                _calcButton.onClick.AddListener(() =>
                {
                    var result = _blockSystem.FloorCalculation();
                    for (int i = 0; i < result.Length; i++) { Debug.Log(string.Join(" ", result[i])); }
                });
            }
        }
    }
}