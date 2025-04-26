using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneUtils
{   public static void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public static void LoadPreviousScene(bool exit = true)
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        if (currentSceneIndex == 0 && exit)
        {
            Application.Quit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
            return;
        }

        SceneManager.LoadScene(currentSceneIndex - 1);
    }
}
