using DefaultNamespace;
using UnityEngine;
using UnityEngine.UI;
using Mouse = UnityEngine.InputSystem.Mouse;

public class EndingController : MonoBehaviour
{
    [Header("Ending Images")]
    public Image knowTooMuchImage;

    public Image movingImage;

    public Image riseImage;

    public Image firedImage;

    private bool _canExit = false;
    private bool _hasClicked = false;

    private void Start()
    {
        HideAllImages();
        ShowEndingImage(GlobalState.FinalEnding);

        // 如果你之后要加 fade / delay，可以把这个延后
        _canExit = true;
    }

    private void Update()
    {
        if (!_canExit || _hasClicked)
            return;

        var mouse = Mouse.current;
        if (mouse.leftButton.isPressed)
        {
            ExitEnding();
        }
    }

    private void HideAllImages()
    {
        if (knowTooMuchImage != null) knowTooMuchImage.gameObject.SetActive(false);
        if (movingImage != null) movingImage.gameObject.SetActive(false);
        if (riseImage != null) riseImage.gameObject.SetActive(false);
        if (firedImage != null) firedImage.gameObject.SetActive(false);
    }

    private void ShowEndingImage(GameEnding ending)
    {
        switch (ending)
        {
            case GameEnding.KnowTooMuch:
                knowTooMuchImage?.gameObject.SetActive(true);
                break;

            case GameEnding.JumpShip:
                movingImage?.gameObject.SetActive(true);
                break;

            case GameEnding.Promoted:
                riseImage?.gameObject.SetActive(true);
                break;

            case GameEnding.Fired:
                firedImage?.gameObject.SetActive(true);
                break;

            default:
                Debug.LogWarning($"[EndingController] Unknown ending type: {ending}");
                riseImage?.gameObject.SetActive(true);
                break;
        }
    }

    private void ExitEnding()
    {
        _hasClicked = true;
        GlobalState.CurrentLevel = GlobalState.CurrentLevel.nextLevel;
        TransitionController.Instance.TransitionToScene("IntroScene");
    }
}
