using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScoreSceneController : MonoBehaviour
{
    public TextMeshProUGUI bossSatisfaction;
    public TextMeshProUGUI soldCopies;
    public TextMeshProUGUI morality;

    private void Start()
    {
        soldCopies.text = $"Sold Copies: {GlobalState.ScoreResult.attention}";
        bossSatisfaction.text = $"Boss Satisfaction: {GlobalState.ScoreResult.injustice}";
        morality.text = $"Morality: {GlobalState.ScoreResult.justice}";
    }

    public void OnClickNextLevel()
    {
        GlobalState.CurrentLevel = GlobalState.CurrentLevel.nextLevel;
        SceneManager.LoadScene("IntroScene");
    }
}
