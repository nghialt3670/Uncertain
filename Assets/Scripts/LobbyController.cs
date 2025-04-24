using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class LobbyController : MonoBehaviour
{
    public const int MIN_PLAYERS = 3;
    public const int MAX_PLAYERS = 10;
    public const int MIN_ALIENS = 1;
    public const float MAX_ALIEN_FRACTION = 1 / 3;

    public UIDocument m_LobbyBaseUIDocument;
    public UIDocument m_GameSettingsOverlayUIDocument;
    public UIDocument m_MatchSettingsOverlayUIDocument;

    public VisualTreeAsset m_PlayerListItemVTA;
    public VisualTreeAsset m_GameSettingsOverlayVTA;
    public VisualTreeAsset m_MatchSettingsModalVTA;
    public string m_PlayerListViewName = "PlayerListView";
    public string m_PlayerNameTextFieldName = "PlayerNameTextField";
    public string m_AddPlayerButtonName = "AddPlayerButton";
    public string m_RemovePlayerButtonName = "RemovePlayerButton";
    public string m_NumAliensLabelName = "NumAliensLabel";
    public string m_NumHumansLabelName = "NumHumansLabel";
    public string m_OpenGameSettingsModalButtonName = "GameSettingsButton";
    public string m_CloseGameSettingsModalButtonName = "CloseButton";
    public string m_LanguageDropdownFieldName = "LanguageDropDownField";

    private VisualElement m_RootVisualElement;
    private ListView m_PlayerListView;
    private Label m_NumAliensLabel;
    private Label m_NumHumansLabel;
    private int m_NumAliens = MIN_ALIENS;
    private List<Player> m_Players = new() {};
    private IVisualElementScheduledItem m_ScrollAnimation;

    private void OnEnable()
    {
        AssignVisualElements();
        SetUpPlayerListView();
        PopulatePlayerList();
        SetUpAddPlayerButton();
        UpdateNumHumansLabel();
        UpdateNumAliensLabel();
        SetUpGameSettingsOverlay();
    }

    private void AssignVisualElements()
    {
        m_RootVisualElement = GetComponent<UIDocument>().rootVisualElement;
        m_PlayerListView = m_RootVisualElement.Q<ListView>(m_PlayerListViewName);
        m_NumAliensLabel = m_RootVisualElement.Q<Label>(m_NumAliensLabelName);
        m_NumHumansLabel = m_RootVisualElement.Q<Label>(m_NumHumansLabelName);
    }

    private void SetUpPlayerListView()
    {
        m_PlayerListView.itemsSource = m_Players;
        m_PlayerListView.makeItem = MakePlayerItem;
        m_PlayerListView.bindItem = BindPlayerItem;
    }

    private VisualElement MakePlayerItem()
    {
        return m_PlayerListItemVTA.CloneTree();
    }

    private void BindPlayerItem(VisualElement element, int index)
    {
        TextField playerNameTextField = element.Q<TextField>(m_PlayerNameTextFieldName);
        Player player = m_Players.ElementAt(index);

        if (player != null)
        {
            playerNameTextField.value = player.name;
            playerNameTextField.RegisterValueChangedCallback(e => player.name = e.newValue);
        }

        Button removePlayerButton = element.Q<Button>(m_RemovePlayerButtonName);

        removePlayerButton.clicked += () => RemovePlayer(index);
        removePlayerButton.clicked += UpdateNumHumansLabel;
    }

    private void PopulatePlayerList()
    {
        for (int i = 0; i < MIN_PLAYERS; i++)
        {
            m_Players.Add(new Player());
        }

        m_PlayerListView.Rebuild();
    }

    private void SetUpAddPlayerButton()
    {
        Button addPlayerButton = m_RootVisualElement.Q<Button>(m_AddPlayerButtonName);
        addPlayerButton.clicked += AddPlayer;
        addPlayerButton.clicked += UpdateNumHumansLabel;
        addPlayerButton.clicked += SmoothScrollToBottom;
    }

    private void SetUpGameSettingsOverlay()
    {
        VisualElement overlay = m_GameSettingsOverlayVTA.CloneTree();
        overlay.StretchToParentSize();

        var openButton = m_RootVisualElement.Q(m_OpenGameSettingsModalButtonName);
        openButton.RegisterCallback<ClickEvent>(e => m_RootVisualElement.Add(overlay));

        var closeButton = overlay.Q(m_CloseGameSettingsModalButtonName);
        closeButton.RegisterCallback<ClickEvent>(e => m_RootVisualElement.Remove(overlay));

        DropdownField languageDropdownField = overlay.Q<DropdownField>(m_LanguageDropdownFieldName);
        languageDropdownField.RegisterCallback<ChangeEvent<string>>(e => { });
    }

    private void AddPlayer()
    {
        if (m_Players.Count >= MAX_PLAYERS)
        {
            return;
        }

        m_Players.Add(new Player());
        m_PlayerListView.Rebuild();
    }

    private void RemovePlayer(int index)
    {
        if (m_Players.Count <= MIN_PLAYERS)
        {
            return;
        }

        m_Players.RemoveAt(index);
        m_PlayerListView.Rebuild();
    }

    private void UpdateNumAliensLabel()
    {
        m_NumAliensLabel.text = m_NumAliens.ToString();
    }

    private void UpdateNumHumansLabel()
    {
        m_NumHumansLabel.text = (m_Players.Count - m_NumAliens).ToString();
    }

    private void SmoothScrollToBottom()
    {
        ScrollView scrollView = m_PlayerListView.Q<ScrollView>();
        float duration = 0.3f;
        float elapsed = 0f;
        float startValue = scrollView.scrollOffset.y;
        float endValue = scrollView.contentContainer.layout.height;

        endValue = Mathf.Max(0, endValue);

        // Cancel previous animation if still running
        m_ScrollAnimation?.Pause();

        m_ScrollAnimation = scrollView.schedule.Execute(() =>
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float newY = Mathf.Lerp(startValue, endValue, t);
            scrollView.scrollOffset = new Vector2(0, newY);

            if (t >= 1f)
            {
                m_ScrollAnimation?.Pause(); // Stop it
            }
        }).Every(10); // ~60 FPS
    }
}
