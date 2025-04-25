using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

public class LobbyUIController : MonoBehaviour
{
    public VisualTreeAsset m_PlayerListItemVTA;

    public string m_PlayerListViewName = "PlayerListView";
    public string m_PlayerNameTextFieldName = "PlayerNameTextField";
    public string m_AddPlayerButtonName = "AddPlayerButton";
    public string m_RemovePlayerButtonName = "RemovePlayerButton";
    public string m_PlayerIndexLabelName = "PlayerIndexLabel";
    public string m_AlienCountLabelName = "AlienCountLabel";
    public string m_HumanCountLabelName = "HumanCountLabel";

    private VisualElement m_Root;
    private ListView m_PlayerListView;
    private Button m_AddPlayerButton;
    private Label m_AlienCountLabel;
    private Label m_HumanCountLabel;

    private IVisualElementScheduledItem m_PlayerListViewScrollAnimation;

    private void Start()
    {
        m_Root = GetComponent<UIDocument>().rootVisualElement;

        m_PlayerListView = m_Root.Q<ListView>(m_PlayerListViewName);
        m_AddPlayerButton = m_Root.Q<Button>(m_AddPlayerButtonName);
        m_AlienCountLabel = m_Root.Q<Label>(m_AlienCountLabelName);
        m_HumanCountLabel = m_Root.Q<Label>(m_HumanCountLabelName);

        Assert.IsNotNull(m_Root, "m_Root is null");
        Assert.IsNotNull(m_PlayerListView, "PlayerListView is null");
        Assert.IsNotNull(m_AlienCountLabel, "AlienCountLabel is null");
        Assert.IsNotNull(m_HumanCountLabel, "HumanCountLabel is null");

        SetUpPlayerListView();
        SetUpAddPlayerButton();
        UpdateAddPlayerButton();
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

            VisualElement removePlayerButton = element.Q(m_RemovePlayerButtonName);
            removePlayerButton.SetEnabled(MatchSettingsManager.CanRemovePlayer());
            removePlayerButton.RegisterCallback<ClickEvent>(e => RemovePlayer(index));
            removePlayerButton.RegisterCallback<ClickEvent>(e => UpdateHumanCountLabel());
            removePlayerButton.RegisterCallback<ClickEvent>(e => UpdateAddPlayerButton());
            removePlayerButton.RegisterCallback<ClickEvent>(e => UpdatePlayerListView());
            removePlayerButton.RegisterCallback<ClickEvent>(e => removePlayerButton.SetEnabled(MatchSettingsManager.CanRemovePlayer()));

            Label playerIndexLabel = element.Q<Label>(m_PlayerIndexLabelName);
            playerIndexLabel.text = (index + 1).ToString();
        }

        m_PlayerListView.itemsSource = MatchSettingsManager.Players;
        m_PlayerListView.makeItem = () => m_PlayerListItemVTA.CloneTree();
        m_PlayerListView.bindItem = bindPlayerItem;
        m_PlayerListView.Rebuild();
    }

    private void SetUpAddPlayerButton()
    {
        m_AddPlayerButton.clicked += AddPlayer;
        m_AddPlayerButton.clicked += UpdateHumanCountLabel;
        m_AddPlayerButton.clicked += SmoothScrollToBottom;
        m_AddPlayerButton.clicked += UpdateAddPlayerButton;
        m_AddPlayerButton.clicked += UpdatePlayerListView;
    }

    private void AddPlayer()
    {
        if (MatchSettingsManager.CanAddPlayer())
        {
            MatchSettingsManager.Players.Add(new Player());
        }
    }

    private void RemovePlayer(int index)
    {
        if (MatchSettingsManager.CanRemovePlayer())
        {
            MatchSettingsManager.Players.RemoveAt(index);
        }
    }

    private void UpdatePlayerListView()
    {
        m_PlayerListView.Rebuild();
    }

    private void UpdateAddPlayerButton()
    {
       m_AddPlayerButton.SetEnabled(MatchSettingsManager.CanAddPlayer());
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
