using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

public class HomeUIController : MonoBehaviour
{
    readonly string m_SingleDeviceButtonName = "SingleDeviceButton";
    readonly string m_OnlineButtonName = "OnlineButton";

    VisualElement m_Root;
    Button m_SingleDeviceButton;
    Button m_OnlineButton;

    private void Start()
    {
        m_Root = GetComponent<UIDocument>().rootVisualElement;
        m_SingleDeviceButton = m_Root.Q<Button>(m_SingleDeviceButtonName);
        m_OnlineButton = m_Root.Q<Button>(m_OnlineButtonName);

        m_SingleDeviceButton.clicked += () => SceneUtils.LoadScene("LobbyScene");
    }
}
