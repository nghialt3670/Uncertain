using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class LobbyUIController : MonoBehaviour
{
    [SerializeField] VisualTreeAsset m_PlayerListItem;

    readonly string m_NextSceneName = "RoleAssignmentScene";
    readonly string m_PlayerListViewName = "PlayerListView";
    readonly string m_PlayerNameTextFieldName = "PlayerNameTextField";
    readonly string m_AddPlayerButtonName = "AddPlayerButton";
    readonly string m_RemovePlayerButtonName = "RemovePlayerButton";
    readonly string m_PlayerIndexLabelName = "PlayerIndexLabel";
    readonly string m_AlienCountLabelName = "AlienCountLabel";
    readonly string m_HumanCountLabelName = "HumanCountLabel";
    readonly string m_StartButtonName = "StartButton";

    VisualElement m_RootElement;
    ListView m_PlayerListView;
    Button m_AddPlayerButton;
    Label m_AlienCountLabel;
    Label m_HumanCountLabel;
    Button m_StartButton;

    private void Start()
    {
        m_RootElement = GetComponent<UIDocument>().rootVisualElement;

        m_PlayerListView = m_RootElement.Q<ListView>(m_PlayerListViewName);
        m_AddPlayerButton = m_RootElement.Q<Button>(m_AddPlayerButtonName);
        m_AlienCountLabel = m_RootElement.Q<Label>(m_AlienCountLabelName);
        m_HumanCountLabel = m_RootElement.Q<Label>(m_HumanCountLabelName);
        m_StartButton = m_RootElement.Q<Button>(m_StartButtonName);

        m_PlayerListView.makeItem = m_PlayerListItem.CloneTree;
        m_PlayerListView.bindItem = BindPlayerItem;
        m_PlayerListView.itemsSource = MatchSettingsManager.Players;

        m_AddPlayerButton.clicked += AddPlayer;
        m_AddPlayerButton.clicked += UpdateUIElements;
        m_AddPlayerButton.clicked += () => UIToolkitUtils.SmoothScrollToBottom(m_PlayerListView.Q<ScrollView>());

        m_StartButton.clicked += () => SceneUtils.LoadScene(m_NextSceneName);

        UpdateUIElements();
    }

    void Update()
    {
        m_AlienCountLabel.text = MatchSettingsManager.AlienCount.ToString();
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
