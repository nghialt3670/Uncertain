using UnityEngine;
using UnityEngine.UIElements;

public class GameRulesUIController : MonoBehaviour
{
    [SerializeField] VisualTreeAsset m_GameRulesOverlay;

    readonly string m_OpenIconName = "GameRulesButton";
    readonly string m_CloseButtonName = "CloseButton";

    VisualElement m_RootElement;
    VisualElement m_GameRulesOverlayElement;
    VisualElement m_OpenIcon;
    Button m_CloseButton;

    void Start()
    {
        m_RootElement = GetComponent<UIDocument>().rootVisualElement;
        m_OpenIcon = m_RootElement.Q<VisualElement>(m_OpenIconName);

        m_GameRulesOverlayElement = m_GameRulesOverlay.CloneTree();
        m_CloseButton = m_GameRulesOverlayElement.Q<Button>(m_CloseButtonName);

        m_OpenIcon.RegisterCallback<ClickEvent>(e => UIToolkitUtils.ShowOverlay(m_RootElement, m_GameRulesOverlayElement));
        m_CloseButton.clicked += () => UIToolkitUtils.HideOverlay(m_GameRulesOverlayElement);
    }
}
