using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

public class HomeUIController : MonoBehaviour
{
    public string m_SingleDeviceButtonName = "SingleDeviceButton";
    public string m_OnlineButtonName = "OnlineButton";

    private VisualElement m_Root;
    private Button m_SingleDeviceButton;
    private Button m_OnlineButton;

    private void Start()
    {
        m_Root = GetComponent<UIDocument>().rootVisualElement;

        m_SingleDeviceButton = m_Root.Q<Button>(m_SingleDeviceButtonName);
        m_OnlineButton = m_Root.Q<Button>(m_OnlineButtonName);

        Assert.IsNotNull(m_Root, "m_Root is null");
        Assert.IsNotNull(m_SingleDeviceButton, "m_SingleDeviceButton is null");
        Assert.IsNotNull(m_OnlineButton, "m_OnlineButton is null");

        m_SingleDeviceButton.clicked += LoadSingleDeviceScene;
    }

    private void LoadSingleDeviceScene()
    {
        SceneUtils.LoadScene("LobbyScene");
    }
}
