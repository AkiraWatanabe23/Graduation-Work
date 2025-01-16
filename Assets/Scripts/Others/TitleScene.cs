using UnityEngine;
using UnityEngine.UI;
using VTNConnect;

public class TitleScene : MonoBehaviour
{
    [SerializeField]
    private Button _gameStartButton = default;

    private void Start()
    {
        VantanConnect.SystemReset();

        if (_gameStartButton == null) { return; }
        _gameStartButton.onClick.AddListener(() =>
        {
            if (Fade.Instance.IsFading) { return; }

            VantanConnect.GameStart((VC_StatusCode code) =>
            {
                SceneLoader.FadeLoad(SceneName.InGame);
            });
        });

        Fade.Instance.StartFadeIn();
    }
}
