using TMPro;
using UnityEngine;

public class LevelController : MonoBehaviour
{
    public TextMeshProUGUI title;

    private void Start()
    {
        title.text = GlobalState.CurrentLevel.displayName;
    }

    void Update()
    {
        
    }
}
