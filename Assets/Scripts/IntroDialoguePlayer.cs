using System;
using UnityEngine;
using Fungus;
using UnityEngine.SceneManagement;

public class IntroDialoguePlayer : MonoBehaviour
{
    private Flowchart _activeFlowchart;

    void Start()
    {
        var level = GlobalState.CurrentLevel;
        if (level == null || level.introDialogue == null)
            return;

        PlayIntro(level.introDialogue);
    }

    void PlayIntro(FungusIntroData data)
    {
        _activeFlowchart = Instantiate(data.flowchartPrefab);

        _activeFlowchart.ExecuteBlock(data.entryBlock);
    }

    private static void OnIntroFinished()
    {
        SceneManager.LoadScene("LevelScene");
    }
}