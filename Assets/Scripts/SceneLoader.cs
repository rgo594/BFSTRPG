using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    Scene scene;
    void Start()
    {
        scene = SceneManager.GetActiveScene();
/*        if (scene.buildIndex == 0)
        {
            StartCoroutine(LoadStartScreenFromSplash());
        }*/
    }

    public static void LoadNextScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.buildIndex + 1);
/*        Debug.Log(scene.buildIndex);
        SceneManager.LoadScene(1);*/
    }
    public void NextScene()
    {
        SceneManager.LoadScene(scene.buildIndex + 1);
    }

    public IEnumerator LoadGameOver()
    {

        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("Game Over");
    }

    public void LoadStartMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(0);
    }

    public void LoadOptionsScene()
    {
        SceneManager.LoadScene(1);
    }

    public void RestartScene()
    {
        Time.timeScale = 1;
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }
}