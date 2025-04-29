using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class RoleAssignmentUIController : MonoBehaviour
{
    [SerializeField] VisualTreeAsset m_LoadingOverlay;
    [SerializeField] VisualTreeAsset m_RoleRevealOverlay;

    readonly string m_PlayerNameLabelName = "PlayerNameLabel";
    readonly string m_RoleCardName = "RoleCard";
    readonly string m_WordLabelName = "WordLabel";
    readonly string m_RoleRevealOverlayCloseButtonName = "CloseButton";

    VisualElement m_RootElement;
    VisualElement m_LoadingOverlayELement;
    VisualElement m_RoleRevealOverlayElement;

    Label m_PlayerNameLabel;
    VisualElement m_RoleCard;
    Label m_WordLabel;
    Button m_RoleRevealOverlayCloseButton;

    float doubleClickThreshold = 0.3f;
    float lastClickTime = -1f;
    List<Player> m_UnassignedPlayers;
    Player m_CurrentPlayer;

    void Start()
    {
        m_RootElement = GetComponent<UIDocument>().rootVisualElement;

        m_LoadingOverlayELement = m_LoadingOverlay.CloneTree();
        m_LoadingOverlayELement.StretchToParentSize();

        m_RoleRevealOverlayElement = m_RoleRevealOverlay.CloneTree();
        m_RoleRevealOverlayCloseButton = m_RoleRevealOverlayElement.Q<Button>(m_RoleRevealOverlayCloseButtonName);
        m_RoleRevealOverlayElement.StretchToParentSize();

        m_PlayerNameLabel = m_RootElement.Q<Label>(m_PlayerNameLabelName);
        m_RoleCard = m_RootElement.Q<VisualElement>(m_RoleCardName);
        m_WordLabel = m_RoleRevealOverlayElement.Q<Label>(m_WordLabelName);

        m_RoleCard.RegisterCallback<PointerDownEvent>(OnRoleCardPointerDown);
        m_RoleRevealOverlayCloseButton.clicked += () => SetUpNextPlayer();
        m_RoleRevealOverlayCloseButton.clicked += () => UIToolkitUtils.HideOverlay(m_RoleRevealOverlayElement);

        m_UnassignedPlayers = MatchSettingsManager.Players.OrderBy(x => Random.value).ToList();
        SetUpNextPlayer();
    }

    private void OnRoleCardPointerDown(PointerDownEvent evt)
    {
        float currentTime = Time.time;

        if (lastClickTime > 0 && (currentTime - lastClickTime) <= doubleClickThreshold)
        {
            m_WordLabel.text = m_CurrentPlayer.words[^1];
            UIToolkitUtils.ShowOverlay(m_RootElement, m_RoleRevealOverlayElement);
            lastClickTime = -1f; 
        }
        else
        {
            lastClickTime = currentTime;
        }
    }

    private void SetUpNextPlayer()
    {
        if (m_UnassignedPlayers.Count == 0)
        {
            SceneUtils.LoadScene("InGameScene");
            return;
        }
        m_CurrentPlayer = m_UnassignedPlayers[^1];
        m_UnassignedPlayers.RemoveAt(m_UnassignedPlayers.Count - 1);
        m_PlayerNameLabel.text = m_CurrentPlayer.name;
    }
}
