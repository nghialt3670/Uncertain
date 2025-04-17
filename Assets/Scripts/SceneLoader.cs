using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public string m_SceneName;
    private Button m_Button;

    private void Start()
    {
        m_Button = GetComponent<Button>();
        m_Button.onClick.AddListener(LoadScene);
    }

    public void LoadScene()
    {
        SceneManager.LoadScene(m_SceneName);
    }
}
