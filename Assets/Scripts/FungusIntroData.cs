using UnityEngine;
using Fungus;

[CreateAssetMenu(menuName = "Game/Fungus Intro Dialogue")]
public class FungusIntroData : ScriptableObject
{
    [Header("Flowchart Template")]
    public Flowchart flowchartPrefab;

    [Header("Entry Block Name")]
    public string entryBlock = "Intro";
}