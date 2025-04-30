using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UIElements;

public class LobbyUIController : MonoBehaviour
{
    [SerializeField] VisualTreeAsset m_PlayerListItem;
    [SerializeField] VisualTreeAsset m_ValidationOverlay;

    readonly string m_NextSceneName = "RoleAssignmentScene";
    readonly string m_PlayerListViewName = "PlayerListView";
    readonly string m_PlayerNameTextFieldName = "PlayerNameTextField";
    readonly string m_AddPlayerButtonName = "AddPlayerButton";
    readonly string m_RemovePlayerButtonName = "RemovePlayerButton";
    readonly string m_PlayerIndexLabelName = "PlayerIndexLabel";
    readonly string m_AlienCountLabelName = "AlienCountLabel";
    readonly string m_HumanCountLabelName = "HumanCountLabel";
    readonly string m_StartButtonName = "StartButton";

    readonly string m_ValidationMessageLabelName = "MessageLabel";
    readonly string m_ValidationOvelayCloseButtonName = "CloseButton";

    VisualElement m_RootElement;
    ListView m_PlayerListView;
    Button m_AddPlayerButton;
    Label m_AlienCountLabel;
    Label m_HumanCountLabel;
    Button m_StartButton;

    VisualElement m_ValidationOverlayElement;
    Label m_ValidationMessageLabel;
    Button m_ValidationOvelayCloseButton;

    private void Start()
    {
        m_RootElement = GetComponent<UIDocument>().rootVisualElement;

        m_PlayerListView = m_RootElement.Q<ListView>(m_PlayerListViewName);
        m_AddPlayerButton = m_RootElement.Q<Button>(m_AddPlayerButtonName);
        m_AlienCountLabel = m_RootElement.Q<Label>(m_AlienCountLabelName);
        m_HumanCountLabel = m_RootElement.Q<Label>(m_HumanCountLabelName);
        m_StartButton = m_RootElement.Q<Button>(m_StartButtonName);

        m_ValidationOverlayElement = m_ValidationOverlay.CloneTree();
        m_ValidationOvelayCloseButton = m_ValidationOverlayElement.Q<Button>(m_ValidationOvelayCloseButtonName);
        m_ValidationMessageLabel = m_ValidationOverlayElement.Q<Label>(m_ValidationMessageLabelName);

        m_PlayerListView.makeItem = m_PlayerListItem.CloneTree;
        m_PlayerListView.bindItem = BindPlayerItem;
        m_PlayerListView.itemsSource = MatchSettingsManager.Players;

        m_AddPlayerButton.clicked += AddPlayer;
        m_AddPlayerButton.clicked += UpdateUIElements;
        m_AddPlayerButton.clicked += () => UIToolkitUtils.SmoothScrollToBottom(m_PlayerListView.Q<ScrollView>());

        m_StartButton.clicked += HandleStart;

        m_ValidationOvelayCloseButton.clicked += () => UIToolkitUtils.HideOverlay(m_ValidationOverlayElement);

        UpdateUIElements();
    }

    void Update()
    {
        m_AlienCountLabel.text = MatchSettingsManager.AlienCount.ToString();
    }

    void HandleStart()
    {
        var validationResult = MatchSettingsManager.ValidatePlayerNames();

        if (validationResult == PlayerNamesValidationResult.CONTAINS_EMPTY)
        {
            var binding = m_ValidationMessageLabel.GetBinding("text") as LocalizedString;
            binding.SetReference("lobby", "player_name_must_not_be_empty");
            UIToolkitUtils.ShowOverlay(m_RootElement, m_ValidationOverlayElement);
            return;
        }
        else if (validationResult == PlayerNamesValidationResult.CONTAINS_DUPLICATE)
        {
            var binding = m_ValidationMessageLabel.GetBinding("text") as LocalizedString;
            binding.SetReference("lobby", "player_name_must_not_be_duplicate");
            UIToolkitUtils.ShowOverlay(m_RootElement, m_ValidationOverlayElement);
            return;
        }

        SceneUtils.LoadScene(m_NextSceneName);
    }

    void BindPlayerItem(VisualElement element, int index)
    {
        TextField playerNameTextField = element.Q<TextField>(m_PlayerNameTextFieldName);
        Player player = MatchSettingsManager.Players.ElementAt(index);

        if (player != null)
        {
            playerNameTextField.value = player.name;
            playerNameTextField.maxLength = GameSettingsManager.PLAYER_NAME_MAX_LENGTH;
            playerNameTextField.RegisterValueChangedCallback(e => player.name = e.newValue);

            if (!MatchSettingsManager.IsNewPlayer(player))
            {
                playerNameTextField.isReadOnly = true;
            }

        }

        VisualElement removePlayerButton = element.Q(m_RemovePlayerButtonName);
        removePlayerButton.SetEnabled(MatchSettingsManager.CanRemovePlayer());
        removePlayerButton.RegisterCallback<ClickEvent>(e => RemovePlayer(index));
        removePlayerButton.RegisterCallback<ClickEvent>(e => UpdateUIElements());
        removePlayerButton.RegisterCallback<ClickEvent>(e => removePlayerButton.SetEnabled(MatchSettingsManager.CanRemovePlayer()));

        Label playerIndexLabel = element.Q<Label>(m_PlayerIndexLabelName);
        playerIndexLabel.text = (index + 1).ToString();
    }

    void AddPlayer()
    {
        if (MatchSettingsManager.CanAddPlayer())
        {
            MatchSettingsManager.Players.Add(new Player());
        }
    }

    void RemovePlayer(int index)
    {
        if (MatchSettingsManager.CanRemovePlayer())
        {
            MatchSettingsManager.Players.RemoveAt(index);
        }
    }

    void UpdateUIElements()
    {
       m_AddPlayerButton.SetEnabled(MatchSettingsManager.CanAddPlayer());
       m_AlienCountLabel.text = MatchSettingsManager.AlienCount.ToString();
       m_HumanCountLabel.text = (MatchSettingsManager.PlayerCount - MatchSettingsManager.AlienCount).ToString();
       m_PlayerListView.Rebuild();
    }
}
