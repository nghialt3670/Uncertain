using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class InGameUIController : MonoBehaviour
{
    [SerializeField] VisualTreeAsset m_PlayerListItem;
    [SerializeField] VisualTreeAsset m_VoteConfirmOverlay;
    [SerializeField] VisualTreeAsset m_EndRoundOverlay;

    readonly string m_PlayerListViewName = "PlayerListView";
    readonly string m_PlayerIndexLabelName = "PlayerIndexLabel";
    readonly string m_PlayerNameLabelName = "PlayerNameLabel";
    readonly string m_VotePlayerIconName = "VotePlayerIcon";
    readonly string m_UnvotedHumanCountLabelName = "UnvotedHumanCountLabel";
    readonly string m_UnvotedAlienCountLabelName = "UnvotedAlienCountLabel";

    readonly string m_VotedPlayerNameLabelName = "PlayerNameLabel";
    readonly string m_VoteConfirmButtonName = "ConfirmButton";
    readonly string m_VoteCancelButtonName = "CancelButton";

    readonly string m_WinningRoleIconName = "RoleIcon";
    readonly string m_NextRoundButtonName = "NextRoundButton";
    readonly string m_ReturnLobbyButtonName = "ReturnLobbyButton";

    VisualElement m_RootElement;
    ListView m_PlayerListView;
    Label m_UnvotedHumanCountLabel;
    Label m_UnvotedAlienCountLabel;

    VisualElement m_VoteConfirmOverlayElement;
    Label m_VotedPlayerNameLabel;
    Button m_VoteConfirmButton;
    Button m_VoteCancelButton;

    VisualElement m_EndRoundOverlayElement;
    VisualElement m_WinningRoleIcon;
    Button m_NextRoundButton;
    Button m_ReturnLobbyButton;

    Player m_CurrentVotedPlayer;

    void Start()
    {
        m_RootElement = GetComponent<UIDocument>().rootVisualElement;
        m_PlayerListView = m_RootElement.Q<ListView>(m_PlayerListViewName);
        m_UnvotedHumanCountLabel = m_RootElement.Q<Label>(m_UnvotedHumanCountLabelName);
        m_UnvotedAlienCountLabel = m_RootElement.Q<Label>(m_UnvotedAlienCountLabelName);

        m_VoteConfirmOverlayElement = m_VoteConfirmOverlay.CloneTree();
        m_VotedPlayerNameLabel = m_VoteConfirmOverlayElement.Q<Label>(m_VotedPlayerNameLabelName);
        m_VoteConfirmButton = m_VoteConfirmOverlayElement.Q<Button>(m_VoteConfirmButtonName);
        m_VoteCancelButton = m_VoteConfirmOverlayElement.Q<Button>(m_VoteCancelButtonName);

        m_EndRoundOverlayElement = m_EndRoundOverlay.CloneTree();
        m_WinningRoleIcon = m_EndRoundOverlayElement.Q<VisualElement>(m_WinningRoleIconName);
        m_NextRoundButton = m_EndRoundOverlayElement.Q<Button>(m_NextRoundButtonName);
        m_ReturnLobbyButton = m_EndRoundOverlayElement.Q<Button>(m_ReturnLobbyButtonName);

        m_PlayerListView.makeItem = m_PlayerListItem.CloneTree;
        m_PlayerListView.bindItem = BindPlayerItem;
        m_PlayerListView.itemsSource = MatchSettingsManager.Players;
        m_PlayerListView.Rebuild();

        m_VoteConfirmButton.clicked += VotePlayer;
        m_VoteCancelButton.clicked += () => UIToolkitUtils.HideOverlay(m_VoteConfirmOverlayElement);

        m_NextRoundButton.clicked += () => SceneUtils.LoadScene("RoleAssignmentScene");
        m_ReturnLobbyButton.clicked += () => SceneUtils.LoadScene("LobbyScene");

        UpdateUnvotedCountEachRole();
    }

    void VotePlayer()
    {
        MatchSettingsManager.Vote(m_CurrentVotedPlayer);
        m_CurrentVotedPlayer = null;

        m_PlayerListView.RefreshItems();
        UpdateUnvotedCountEachRole();
        UIToolkitUtils.HideOverlay(m_VoteConfirmOverlayElement);

        if (MatchSettingsManager.IsHumansWin())
        {
            m_WinningRoleIcon.AddToClassList("humanIcon");
            UIToolkitUtils.ShowOverlay(m_RootElement, m_EndRoundOverlayElement);
        }
        else if (MatchSettingsManager.IsAliensWin())
        {
            m_WinningRoleIcon.AddToClassList("alienIcon");
            UIToolkitUtils.ShowOverlay(m_RootElement, m_EndRoundOverlayElement);
        }
    }

    void UpdateUnvotedCountEachRole()
    {
        m_UnvotedHumanCountLabel.text = MatchSettingsManager.GetUnvotedHumanCount().ToString();
        m_UnvotedAlienCountLabel.text = MatchSettingsManager.GetUnvotedAlienCount().ToString();
    }

    void BindPlayerItem(VisualElement element, int index)
    {
        Player player = MatchSettingsManager.Players.ElementAt(index);

        Label playerIndexLabel = element.Q<Label>(m_PlayerIndexLabelName);
        playerIndexLabel.text = (index + 1).ToString();

        Label playerNameLabel = element.Q<Label>(m_PlayerNameLabelName);
        playerNameLabel.text = player.name;

        VisualElement votePlayerIcon = element.Q<VisualElement>(m_VotePlayerIconName);

        void HandleVoteClick(ClickEvent e)
        {
            m_CurrentVotedPlayer = player;
            m_VotedPlayerNameLabel.text = player.name;
            UIToolkitUtils.ShowOverlay(m_RootElement, m_VoteConfirmOverlayElement);
        }

        votePlayerIcon.RegisterCallback<ClickEvent>(HandleVoteClick);

        if (MatchSettingsManager.IsVoted(player)) 
        {
            element.SetEnabled(false);
        }

        if (MatchSettingsManager.IsEndGame() || MatchSettingsManager.IsVoted(player))
        {
            votePlayerIcon.RemoveFromClassList("voteIcon");
            votePlayerIcon.AddToClassList(MatchSettingsManager.IsAlien(player) ? "alienIcon" : "humanIcon");
            votePlayerIcon.UnregisterCallback<ClickEvent>(HandleVoteClick);
        }
    }
}
