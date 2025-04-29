using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneUtils
{
    public static void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public static void Return()
    {
        int currSceneIndex = SceneManager.GetActiveScene().buildIndex;
        
        if (currSceneIndex == 0)
        {
            Exit();
            return;
        }

        int prevSceneIndex = currSceneIndex - 1;
        string prevSceneName = GetSceneNameByBuildIndex(prevSceneIndex);
        LoadScene(prevSceneName);
    }

    public static string GetSceneNameByBuildIndex(int index)
    {
        return System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(index));
    }

    public static void Exit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
