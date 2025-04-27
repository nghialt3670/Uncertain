using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class RoleAssignmentUIController : MonoBehaviour
{
    public string m_WordsEndpoint = "single-device/v1/generate";

    public VisualTreeAsset m_RoleRevealOverlayVTA;

    public string m_PlayerNameLabelName = "PlayerNameLabel";
    public string m_RoleCardName = "RoleCard";
    public string m_WordLabelName = "WordLabel";
    public string m_OverlayCloseButtonName = "CloseButton";

    private VisualElement m_Root;
    private VisualElement m_OverlayRoot;

    private Label m_PlayerNameLabel;
    private VisualElement m_RoleCard;
    private Label m_WordLabel;
    private Button m_OverlayCloseButton;

    private float doubleClickThreshold = 0.3f;
    private float lastClickTime = -1f;
    private List<Player> m_Players;
    private Player m_CurrentPlayer;

    void Start()
    {
        m_Root = GetComponent<UIDocument>().rootVisualElement;

        m_OverlayRoot = m_RoleRevealOverlayVTA.CloneTree();
        m_OverlayCloseButton = m_OverlayRoot.Q<Button>(m_OverlayCloseButtonName);
        m_OverlayRoot.StretchToParentSize();

        m_PlayerNameLabel = m_Root.Q<Label>(m_PlayerNameLabelName);
        m_RoleCard = m_Root.Q<VisualElement>(m_RoleCardName);
        m_WordLabel = m_OverlayRoot.Q<Label>(m_WordLabelName);

        m_RoleCard.RegisterCallback<PointerDownEvent>(OnRoleCardPointerDown);
        m_OverlayCloseButton.clicked += HideOverlay;

        m_Players = MatchSettingsManager.Players.OrderBy(x => UnityEngine.Random.value).ToList();

        AssignRoles();
    }

    private void OnRoleCardPointerDown(PointerDownEvent evt)
    {
        float currentTime = Time.time;

        if (lastClickTime > 0 && (currentTime - lastClickTime) <= doubleClickThreshold)
        {
            m_WordLabel.text = m_CurrentPlayer.words[^1];
            ShowOverlay();
            lastClickTime = -1f; 
        }
        else
        {
            lastClickTime = currentTime;
        }
    }

    private void ShowOverlay()
    {
        if (m_Root.Contains(m_OverlayRoot)) return;
        m_Root.Add(m_OverlayRoot);
    }

    private void HideOverlay()
    {
        m_Root.Remove(m_OverlayRoot);
    }

    private void SetUpCurrentPlayer()
    {
        m_CurrentPlayer = m_Players[m_Players.Count - 1];
        m_Players.RemoveAt(m_Players.Count - 1);
        m_PlayerNameLabel.text = m_CurrentPlayer.name;
    }

    private async void AssignRoles()
    {
        var wordsRequest = new
        {
            domain = MatchSettingsManager.Topic,
            language = LocalizationUtils.ConvertCodeToNativeName(MatchSettingsManager.Locale),
            exceptedPairs = MatchSettingsManager.GetAssignedWords()
        };
        var wordsResponse = await ApiUtils.PostAsync<ApiResponse<WordsData>>(m_WordsEndpoint, wordsRequest);

        if (wordsResponse != null) return;

        MatchSettingsManager.AssignRoles(wordsResponse.data.wordPair.first, wordsResponse.data.wordPair.second);
        SetUpCurrentPlayer();
    }
}

[Serializable]
public class WordsData
{
    public WordPair wordPair;
}

[Serializable]
public class WordPair
{
    public string first;
    public string second;
}