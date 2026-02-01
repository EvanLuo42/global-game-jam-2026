using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FungusEndListener : MonoBehaviour
{
    public void NotifyEnd()
    {
        StartCoroutine(Wait());
    }
    
    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene("LevelScene");
        Destroy(gameObject);
    }
}