using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    public void LoadLevel(int index)
    {
        StartCoroutine(LoadLevelAsync(index));
    }

    public void LoadLevel(string index)
    {
        StartCoroutine(LoadLevelAsync(index));
    }

    IEnumerator LoadLevelAsync(int index)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(index);
        //LoadingScreen.SetActive(true);
        while (!asyncLoad.isDone)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            //Slider.value = progress;
            yield return null;
        }
        //LoadingScreen.SetActive(false);
        GameManager.Instance.UpdateGameState(GameState.Ready);
    }

    IEnumerator LoadLevelAsync(string index)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(index);
        //LoadingScreen.SetActive(true);
        while (!asyncLoad.isDone)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            //Slider.value = progress;
            yield return null;
        }
        //LoadingScreen.SetActive(false);
        GameManager.Instance.UpdateGameState(GameState.Ready);
    }
}
