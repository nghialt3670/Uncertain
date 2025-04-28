using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

public class RoleAssignmentUIController : MonoBehaviour
{
    public string m_WordsEndpoint = "single-device/v1/generate";

    public VisualTreeAsset m_LoadingOverlay;
    public VisualTreeAsset m_RoleRevealOverlay;

    public string m_PlayerNameLabelName = "PlayerNameLabel";
    public string m_RoleCardName = "RoleCard";
    public string m_WordLabelName = "WordLabel";
    public string m_RoleRevealOverlayCloseButtonName = "CloseButton";

    private VisualElement m_Root;
    private VisualElement m_LoadingOverlayRoot;
    private VisualElement m_RoleRevealOverlayRoot;

    private Label m_PlayerNameLabel;
    private VisualElement m_RoleCard;
    private Label m_WordLabel;
    private Button m_RoleRevealOverlayCloseButton;

    private float doubleClickThreshold = 0.3f;
    private float lastClickTime = -1f;
    private List<Player> m_Players;
    private Player m_CurrentPlayer;

    void Start()
    {
        m_Root = GetComponent<UIDocument>().rootVisualElement;

        m_LoadingOverlayRoot = m_LoadingOverlay.CloneTree();
        m_LoadingOverlayRoot.StretchToParentSize();

        m_RoleRevealOverlayRoot = m_RoleRevealOverlay.CloneTree();
        m_RoleRevealOverlayCloseButton = m_RoleRevealOverlayRoot.Q<Button>(m_RoleRevealOverlayCloseButtonName);
        m_RoleRevealOverlayRoot.StretchToParentSize();

        m_PlayerNameLabel = m_Root.Q<Label>(m_PlayerNameLabelName);
        m_RoleCard = m_Root.Q<VisualElement>(m_RoleCardName);
        m_WordLabel = m_RoleRevealOverlayRoot.Q<Label>(m_WordLabelName);

        Assert.IsNotNull(m_Root, "m_Root is null");
        Assert.IsNotNull(m_RoleRevealOverlayRoot, "m_OverlayRoot is null");
        Assert.IsNotNull(m_RoleRevealOverlayCloseButton, "m_RoleRevealOverlayCloseButton is null");
        Assert.IsNotNull(m_PlayerNameLabel, "m_PlayerNameLabel is null");
        Assert.IsNotNull(m_RoleCard, "m_RoleCard is null");
        Assert.IsNotNull(m_WordLabel, "m_WordLabel is null");

        m_RoleCard.RegisterCallback<PointerDownEvent>(OnRoleCardPointerDown);
        m_RoleRevealOverlayCloseButton.clicked += () => SetUpCurrentPlayer();
        m_RoleRevealOverlayCloseButton.clicked += () => HideOverlay(m_RoleRevealOverlayRoot);

        m_Players = MatchSettingsManager.Players.OrderBy(x => UnityEngine.Random.value).ToList();

        _ = AssignRoles();
    }

    private void OnRoleCardPointerDown(PointerDownEvent evt)
    {
        float currentTime = Time.time;

        if (lastClickTime > 0 && (currentTime - lastClickTime) <= doubleClickThreshold)
        {
            m_WordLabel.text = m_CurrentPlayer.words[^1];
            ShowOverlay(m_RoleRevealOverlayRoot);
            lastClickTime = -1f; 
        }
        else
        {
            lastClickTime = currentTime;
        }
    }

    private void ShowOverlay(VisualElement overlayRoot)
    {
        if (m_Root.Contains(overlayRoot)) return;
        m_Root.Add(overlayRoot);
    }

    private void HideOverlay(VisualElement overlayRoot)
    {
        m_Root.Remove(overlayRoot);
    }

    private void SetUpCurrentPlayer()
    {
        if (m_Players.Count == 0)
        {
            SceneUtils.LoadPreviousScene();
            return;
        }
        m_CurrentPlayer = m_Players[^1];
        m_Players.RemoveAt(m_Players.Count - 1);
        m_PlayerNameLabel.text = m_CurrentPlayer.name;
    }

    private async Task AssignRoles()
    {
        ShowOverlay(m_LoadingOverlayRoot);

        WordsRequest wordsRequest = new()
        {
            domain=MatchSettingsManager.Topic,
            language=LocalizationUtils.ConvertCodeToNativeName(MatchSettingsManager.Locale),
            exceptedPairs=new List<WordPair>(),
        };
        var wordsResponse = await ApiUtils.PostAsync<ApiResponse<WordsData>>(m_WordsEndpoint, wordsRequest);
        Debug.Log(wordsResponse.ToString());

        if (wordsResponse == null) return;

        MatchSettingsManager.AssignRoles(wordsResponse.data.wordPair.first, wordsResponse.data.wordPair.second);
        SetUpCurrentPlayer();
        HideOverlay(m_LoadingOverlayRoot);
    }
}

[Serializable]
public class WordsRequest
{
    public string domain;
    public string language;
    public List<WordPair> exceptedPairs;
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
