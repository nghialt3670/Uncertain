using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class InGameUIController : MonoBehaviour
{
    [SerializeField] VisualTreeAsset m_PlayerListItem;
    [SerializeField] VisualTreeAsset m_VoteConfirmOverlay;

    readonly string m_PlayerListViewName = "PlayerListView";
    readonly string m_PlayerIndexLabelName = "PlayerIndexLabel";
    readonly string m_VotePlayerIconName = "VotePlayerIcon";
    readonly string m_PlayerNameLabelName = "PlayerNameLabel";
    readonly string m_VotedPlayerNameLabelName = "PlayerNameLabel";
    readonly string m_VoteConfirmButtonName = "ConfirmButton";
    readonly string m_VoteCancelButtonName = "CancelButton";

    VisualElement m_RootElement;
    ListView m_PlayerListView;

    VisualElement m_VoteConfirmOverlayElement;
    Label m_VotedPlayerNameLabel;
    Button m_VoteConfirmButton;
    Button m_VoteCancelButton;

    Player m_CurrentVotedPlayer;
    int m_CurrentVoteIndex;

    void Start()
    {
        m_RootElement = GetComponent<UIDocument>().rootVisualElement;
        m_PlayerListView = m_RootElement.Q<ListView>(m_PlayerListViewName);
        m_VoteConfirmOverlayElement = m_VoteConfirmOverlay.CloneTree();
        m_VotedPlayerNameLabel = m_VoteConfirmOverlayElement.Q<Label>(m_VotedPlayerNameLabelName);
        m_VoteConfirmButton = m_VoteConfirmOverlayElement.Q<Button>(m_VoteConfirmButtonName);
        m_VoteCancelButton = m_VoteConfirmOverlayElement.Q<Button>(m_VoteCancelButtonName);

        m_CurrentVoteIndex = 0;

        m_PlayerListView.makeItem = m_PlayerListItem.CloneTree;
        m_PlayerListView.bindItem = BindPlayerItem;
        m_PlayerListView.itemsSource = MatchSettingsManager.Players;
        m_PlayerListView.Rebuild();

        m_VoteConfirmButton.clicked += () =>
        {
            m_CurrentVotedPlayer.voteIndices[^1] = m_CurrentVoteIndex;
            m_CurrentVoteIndex++;
            m_PlayerListView.RefreshItems();
            UIToolkitUtils.HideOverlay(m_VoteConfirmOverlayElement);
        };

        m_VoteCancelButton.clicked += () => UIToolkitUtils.HideOverlay(m_VoteConfirmOverlayElement);
    }

    void BindPlayerItem(VisualElement element, int index)
    {
        Player player = MatchSettingsManager.Players.ElementAt(index);

        Label playerIndexLabel = element.Q<Label>(m_PlayerIndexLabelName);
        playerIndexLabel.text = (index + 1).ToString();

        Label playerNameLabel = element.Q<Label>(m_PlayerNameLabelName);
        playerNameLabel.text = player.name;

        VisualElement votePlayerIcon = element.Q<VisualElement>(m_VotePlayerIconName);
        votePlayerIcon.RegisterCallback<ClickEvent>(e => 
        {
            m_CurrentVotedPlayer = player;
            m_VotedPlayerNameLabel.text = player.name;
            UIToolkitUtils.ShowOverlay(m_RootElement, m_VoteConfirmOverlayElement);
        });

        bool isPlayerVoted = player.voteIndices[^1] != -1;
        if (isPlayerVoted) 
        {
            element.SetEnabled(false);
            votePlayerIcon.RemoveFromClassList("voteIcon");
            votePlayerIcon.AddToClassList(player.roles[^1] == Role.ALIEN ? "alienIcon" : "humanIcon");
        }
    }
}
