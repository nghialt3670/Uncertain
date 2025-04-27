using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using System.Threading.Tasks;
using UnityEngine.Assertions;

public class MatchSettingsUIController : MonoBehaviour
{
    public VisualTreeAsset m_MatchSettingsOverlay;

    public string m_LanguagesEndpoint = "api/single-device/v1/languages";
    public string m_TopicsEndpoint = "api/single-device/v1/topics";

    public string m_OpenButtonName = "MatchSettingsButton";
    public string m_LanguageDropdownFieldName = "LanguageDropdownField";
    public string m_TopicDropdownFieldName = "TopicDropdownField";
    public string m_AlienCountSliderName = "AlienCountSlider";
    public string m_AlienCountLabelName = "AlienCountLabel";
    public string m_SaveButtonName = "SaveButton";
    public string m_CloseButtonName = "CloseButton";

    private VisualElement m_BaseRoot;
    private VisualElement m_OpenButton;
    private VisualElement m_OverlayRoot;
    private DropdownField m_LanguageDropdownField;
    private DropdownField m_TopicDropdownField;
    private SliderInt m_AlienCountSliderInt;
    private Label m_AlienCountLabel;
    private Button m_SaveButton;
    private Button m_CloseButton;

    private IVisualElementScheduledItem m_SaveButtonMonitor;

    void Start()
    {
        m_BaseRoot = GetComponent<UIDocument>().rootVisualElement;
        m_OpenButton = m_BaseRoot.Q<VisualElement>(m_OpenButtonName);

        m_OverlayRoot = m_MatchSettingsOverlay.CloneTree();
        m_OverlayRoot.StretchToParentSize();

        m_LanguageDropdownField = m_OverlayRoot.Q<DropdownField>(m_LanguageDropdownFieldName);
        m_TopicDropdownField = m_OverlayRoot.Q<DropdownField>(m_TopicDropdownFieldName);
        m_AlienCountSliderInt = m_OverlayRoot.Q<SliderInt>(m_AlienCountSliderName);
        m_AlienCountLabel = m_OverlayRoot.Q<Label>(m_AlienCountLabelName);
        m_SaveButton = m_OverlayRoot.Q<Button>(m_SaveButtonName);
        m_CloseButton = m_OverlayRoot.Q<Button>(m_CloseButtonName);

        Assert.IsNotNull(m_OpenButton, "m_OpenButton is null");
        Assert.IsNotNull(m_LanguageDropdownField, "m_LanguageDropdownField is null");
        Assert.IsNotNull(m_TopicDropdownField, "m_TopicDropdownField is null");
        Assert.IsNotNull(m_AlienCountSliderInt, "m_AlienCountSliderInt is null");
        Assert.IsNotNull(m_AlienCountLabel, "m_AlienCountLabel is null");
        Assert.IsNotNull(m_SaveButton, "m_SaveButton is null");
        Assert.IsNotNull(m_CloseButton, "m_CloseButton is null");

        _ = FetchFieldOptions();

        m_AlienCountSliderInt.RegisterValueChangedCallback(e => m_AlienCountLabel.text = e.newValue.ToString());
        m_AlienCountLabel.text = m_AlienCountSliderInt.value.ToString();
        m_OpenButton.RegisterCallback<ClickEvent>(e => ShowOverlay());
        m_SaveButton.clicked += SaveMatchSettings;
        m_CloseButton.clicked += HideOverlay;

        StartSaveButtonMonitor();
        LoadMatchSettings();
    }

    public void ShowOverlay()
    {
        LoadMatchSettings();
        m_BaseRoot.Add(m_OverlayRoot);
    }

    public void HideOverlay()
    {
        LoadMatchSettings();
        m_BaseRoot.Remove(m_OverlayRoot);
    }

    private void LoadMatchSettings()
    {
        m_LanguageDropdownField.value = LocalizationUtils.ConvertCodeToNativeName(MatchSettingsManager.Locale);
        m_TopicDropdownField.value = MatchSettingsManager.Topic;
        m_AlienCountSliderInt.lowValue = MatchSettingsManager.GetMinAlientCount();
        m_AlienCountSliderInt.highValue = MatchSettingsManager.GetMaxAlienCount();
        m_AlienCountSliderInt.value = MatchSettingsManager.AlienCount;
        m_AlienCountSliderInt.SetEnabled((m_AlienCountSliderInt.lowValue != m_AlienCountSliderInt.highValue));
    }

    private void SaveMatchSettings()
    {
        MatchSettingsManager.Locale = LocalizationUtils.ConvertNativeNameToCode(m_LanguageDropdownField.value);
        MatchSettingsManager.Topic = m_TopicDropdownField.value;
        MatchSettingsManager.AlienCount = m_AlienCountSliderInt.value;
    }

    private async Task FetchFieldOptions()
    {
        await FetchLanguageOptions();
        await FetchTopicOptions();
        m_LanguageDropdownField.RegisterValueChangedCallback(async e => await FetchTopicOptions());
    }

    private async Task FetchLanguageOptions()
    {
        var languagesResponse = await ApiUtils.GetAsync<ApiResponse<List<LanguageDetail>>>(m_LanguagesEndpoint);

        if (languagesResponse == null) return;

        m_LanguageDropdownField.choices = languagesResponse.data
            .Select(detail => detail.languageCode)
            .Select(LocalizationUtils.ConvertCodeToNativeName)
            .ToList();

        m_LanguageDropdownField.value = m_LanguageDropdownField.choices
            .FirstOrDefault(choice => choice == LocalizationUtils.ConvertCodeToNativeName(MatchSettingsManager.Locale)) ??
            m_LanguageDropdownField.choices.FirstOrDefault();
    }

    private async Task FetchTopicOptions()
    {
        string languageCode = LocalizationUtils.ConvertNativeNameToCode(m_LanguageDropdownField.value);
        string topicsEndpoint = $"{m_TopicsEndpoint}?languageCode={languageCode}";
        var topicsResponse = await ApiUtils.GetAsync<ApiResponse<List<TopicDetail>>>(topicsEndpoint);

        if (topicsResponse == null) return;

        m_TopicDropdownField.choices = topicsResponse.data
            .Select(detail => detail.topicName)
            .ToList();

        m_TopicDropdownField.value = m_TopicDropdownField.choices
            .FirstOrDefault(choice => choice == MatchSettingsManager.Topic) ??
            m_TopicDropdownField.choices.FirstOrDefault();
    }

    private void StartSaveButtonMonitor()
    {
        m_SaveButtonMonitor?.Pause();

        m_SaveButtonMonitor = m_SaveButton.schedule.Execute(() =>
        {
            bool isLanguageChanged = LocalizationUtils.ConvertNativeNameToCode(m_LanguageDropdownField.value) != MatchSettingsManager.Locale;
            bool isTopicChanged = m_TopicDropdownField.value != MatchSettingsManager.Topic;
            bool isAlienCountChanged = m_AlienCountSliderInt.value != MatchSettingsManager.AlienCount;

            m_SaveButton.SetEnabled(isLanguageChanged || isTopicChanged || isAlienCountChanged);
        }).Every(10);
    }
}

[Serializable]
public class LanguageDetail
{
    public string id;
    public string languageName;
    public string languageCode;
}

[Serializable]
public class TopicDetail
{
    public string id;
    public string topicName;
    public string languageCode;
}