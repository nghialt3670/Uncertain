using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

public class LobbyController : MonoBehaviour
{
    public VisualTreeAsset m_PlayerListItemVTA;
    public VisualTreeAsset m_GameSettingsOverlayVTA;
    public VisualTreeAsset m_GameRulesOverlayVTA;
    public VisualTreeAsset m_MatchSettingsModalVTA;

    public string m_PlayerListViewName = "PlayerListView";
    public string m_PlayerNameTextFieldName = "PlayerNameTextField";
    public string m_AddPlayerButtonName = "AddPlayerButton";
    public string m_RemovePlayerButtonName = "RemovePlayerButton";
    public string m_AlienCountLabelName = "AlienCountLabel";
    public string m_HumanCountLabelName = "HumanCountLabel";
    public string m_OpenGameSettingsOverlayButtonName = "GameSettingsButton";
    public string m_CloseGameSettingsOverlayButtonName = "CloseButton";
    public string m_SaveGameSettingsButtonName = "SaveButton";
    public string m_LanguageDropdownFieldName = "LanguageDropdownField";
    public string m_VolumeSliderName = "VolumeSlider";

    private VisualElement m_LobbyBaseRootVisualElement;
    private VisualElement m_OpenGameSettingsOverlayButton;
    private ListView m_PlayerListView;
    private Label m_AlienCountLabel;
    private Label m_HumanCountLabel;

    private VisualElement m_GameSettingsOverlayRootVisualElement;
    private DropdownField m_LanguageDropdownField;
    private Slider m_VolumeSlider;
    private Button m_SaveGameSettingsButton;
    private Button m_CloseGameSettingsOverlayButton;

    private VisualElement m_MatchSettingsOverlayRootVisualElement;
    private VisualElement m_GameRulesOverlayRootVisualElement;

    private IVisualElementScheduledItem m_PlayerListViewScrollAnimation;
    private IVisualElementScheduledItem m_SaveGameSettingsButtonMonitor;

    private void OnEnable()
    {
        InitializeUIElementReferences();
        SetUpLobbyBase();
        SetUpGameSettingsOverlay();
    }

    private void InitializeUIElementReferences()
    {
        m_LobbyBaseRootVisualElement = GetComponent<UIDocument>().rootVisualElement;

        m_OpenGameSettingsOverlayButton = m_LobbyBaseRootVisualElement.Q<VisualElement>(m_OpenGameSettingsOverlayButtonName);
        m_PlayerListView = m_LobbyBaseRootVisualElement.Q<ListView>(m_PlayerListViewName);
        m_AlienCountLabel = m_LobbyBaseRootVisualElement.Q<Label>(m_AlienCountLabelName);
        m_HumanCountLabel = m_LobbyBaseRootVisualElement.Q<Label>(m_HumanCountLabelName);

        m_GameSettingsOverlayRootVisualElement = m_GameSettingsOverlayVTA.CloneTree();
        m_LanguageDropdownField = m_GameSettingsOverlayRootVisualElement.Q<DropdownField>(m_LanguageDropdownFieldName);
        m_VolumeSlider = m_GameSettingsOverlayRootVisualElement.Q<Slider>(m_VolumeSliderName);
        m_SaveGameSettingsButton = m_GameSettingsOverlayRootVisualElement.Q<Button>(m_SaveGameSettingsButtonName);
        m_CloseGameSettingsOverlayButton = m_GameSettingsOverlayRootVisualElement.Q<Button>(m_CloseGameSettingsOverlayButtonName);
        m_GameSettingsOverlayRootVisualElement.StretchToParentSize();

        m_GameRulesOverlayRootVisualElement = m_GameRulesOverlayVTA.CloneTree();
        m_GameRulesOverlayRootVisualElement.StretchToParentSize();

        m_MatchSettingsOverlayRootVisualElement = m_MatchSettingsModalVTA.CloneTree();
        m_MatchSettingsOverlayRootVisualElement.StretchToParentSize();

        Assert.IsNotNull(m_LobbyBaseRootVisualElement, "LobbyBaseRootVisualElement is null");
        Assert.IsNotNull(m_OpenGameSettingsOverlayButton, "OpenGameSettingsOverlayButton is null");
        Assert.IsNotNull(m_PlayerListView, "PlayerListView is null");
        Assert.IsNotNull(m_AlienCountLabel, "AlienCountLabel is null");
        Assert.IsNotNull(m_HumanCountLabel, "HumanCountLabel is null");
        Assert.IsNotNull(m_GameSettingsOverlayRootVisualElement, "GameSettingsOverlayRootVisualElement is null");
        Assert.IsNotNull(m_LanguageDropdownField, "LanguageDropdownField is null");
        Assert.IsNotNull(m_VolumeSlider, "VolumeSlider is null");
        Assert.IsNotNull(m_SaveGameSettingsButton, "SaveGameSettingsButton is null");
        Assert.IsNotNull(m_CloseGameSettingsOverlayButton, "CloseGameSettingsOverlayButton is null");
        Assert.IsNotNull(m_GameRulesOverlayRootVisualElement, "GameRulesOverlayRootVisualElement is null");
        Assert.IsNotNull(m_MatchSettingsOverlayRootVisualElement, "MatchSettingsOverlayRootVisualElement is null");
    }

    private void SetUpLobbyBase()
    {
        SetUpPlayerListView();
        SetUpAddPlayerButton();
        UpdateHumanCountLabel();
        UpdateAlienCountLabel();
    }

    private void SetUpPlayerListView()
    {
        void bindPlayerItem(VisualElement element, int index)
        {
            TextField playerNameTextField = element.Q<TextField>(m_PlayerNameTextFieldName);
            Player player = MatchSettingsManager.Players.ElementAt(index);

            if (player != null)
            {
                playerNameTextField.value = player.name;
                playerNameTextField.RegisterValueChangedCallback(e => player.name = e.newValue);
            }

            Button removePlayerButton = element.Q<Button>(m_RemovePlayerButtonName);

            removePlayerButton.clicked += () => RemovePlayer(index);
            removePlayerButton.clicked += UpdateHumanCountLabel;
        }

        m_PlayerListView.itemsSource = MatchSettingsManager.Players;
        m_PlayerListView.makeItem = () => m_PlayerListItemVTA.CloneTree();
        m_PlayerListView.bindItem = bindPlayerItem;
        m_PlayerListView.Rebuild();
    }

    private void SetUpAddPlayerButton()
    {
        Button addPlayerButton = m_LobbyBaseRootVisualElement.Q<Button>(m_AddPlayerButtonName);
        addPlayerButton.clicked += AddPlayer;
        addPlayerButton.clicked += UpdateHumanCountLabel;
        addPlayerButton.clicked += SmoothScrollToBottom;
    }

    private void SetUpGameSettingsOverlay()
    {
        RegisterGameSettingsCallbacks();
        StartSaveButtonMonitor();
    }

    private void RegisterGameSettingsCallbacks()
    {
        m_LanguageDropdownField.RegisterValueChangedCallback(e => LocalizationUtils.SetLocaleByNativeName(e.newValue));
        m_VolumeSlider.RegisterValueChangedCallback(e => AudioListener.volume = e.newValue);
        m_OpenGameSettingsOverlayButton.RegisterCallback<ClickEvent>(e =>
        {
            LoadGameSettingsToFields();
            m_LobbyBaseRootVisualElement.Add(m_GameSettingsOverlayRootVisualElement);
        });
        m_SaveGameSettingsButton.clicked += SaveGameSettingsFromFields;
        m_CloseGameSettingsOverlayButton.clicked += () => m_LobbyBaseRootVisualElement.Remove(m_GameSettingsOverlayRootVisualElement);
    }

    private void StartSaveButtonMonitor()
    {
        m_SaveGameSettingsButtonMonitor?.Pause();

        m_SaveGameSettingsButtonMonitor = m_SaveGameSettingsButton.schedule.Execute(() =>
        {
            bool isLanguageChanged = LocalizationUtils.ConvertNativeNameToCode(m_LanguageDropdownField.value) != GameSettingsManager.Locale;
            bool isVolumeChanged = !Mathf.Approximately(m_VolumeSlider.value, GameSettingsManager.Volume);

            m_SaveGameSettingsButton.SetEnabled(isLanguageChanged || isVolumeChanged);
        }).Every(100);
    }


    private void LoadGameSettingsToFields()
    {
        m_LanguageDropdownField.choices = LocalizationUtils.GetAvailableNativeNames();
        m_LanguageDropdownField.value = LocalizationUtils.ConvertCodeToNativeName(GameSettingsManager.Locale);
        m_VolumeSlider.value = GameSettingsManager.Volume;
    }

    private void SaveGameSettingsFromFields()
    {
        GameSettingsManager.Locale = LocalizationUtils.ConvertNativeNameToCode(m_LanguageDropdownField.value);
        GameSettingsManager.Volume = m_VolumeSlider.value;
    }

    private void AddPlayer()
    {
        if (MatchSettingsManager.CanAddPlayer())
        {
            MatchSettingsManager.Players.Add(new Player());
            m_PlayerListView.Rebuild();
        }
    }

    private void RemovePlayer(int index)
    {
        if (MatchSettingsManager.CanRemovePlayer())
        {
            MatchSettingsManager.Players.RemoveAt(index);
            m_PlayerListView.Rebuild();
        }
    }

    private void UpdateAlienCountLabel()
    {
        m_AlienCountLabel.text = MatchSettingsManager.AlienCount.ToString();
    }

    private void UpdateHumanCountLabel()
    {
        m_HumanCountLabel.text = (MatchSettingsManager.PlayerCount - MatchSettingsManager.AlienCount).ToString();
    }

    private void SmoothScrollToBottom()
    {
        ScrollView scrollView = m_PlayerListView.Q<ScrollView>();
        float duration = 0.3f;
        float elapsed = 0f;
        float startValue = scrollView.scrollOffset.y;
        float endValue = scrollView.contentContainer.layout.height;

        endValue = Mathf.Max(0, endValue);

        m_PlayerListViewScrollAnimation?.Pause();

        m_PlayerListViewScrollAnimation = scrollView.schedule.Execute(() =>
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float newY = Mathf.Lerp(startValue, endValue, t);
            scrollView.scrollOffset = new Vector2(0, newY);

            if (t >= 1f)
            {
                m_PlayerListViewScrollAnimation?.Pause();
            }
        }).Every(10);
    }
}
