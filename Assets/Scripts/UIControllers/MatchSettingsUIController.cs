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

    readonly string m_LanguagesEndpoint = "single-device/v1/languages";
    readonly string m_TopicsEndpoint = "single-device/v1/topics";
    readonly string m_OpenButtonName = "MatchSettingsButton";
    readonly string m_LanguageDropdownFieldName = "LanguageDropdownField";
    readonly string m_TopicDropdownFieldName = "TopicDropdownField";
    readonly string m_AlienCountSliderName = "AlienCountSlider";
    readonly string m_AlienCountLabelName = "AlienCountLabel";
    readonly string m_SaveButtonName = "SaveButton";
    readonly string m_CloseButtonName = "CloseButton";

    VisualElement m_RootElement;
    Button m_OpenButton;
    VisualElement m_MatchSettingsOverlayElement;
    DropdownField m_LanguageDropdownField;
    DropdownField m_TopicDropdownField;
    SliderInt m_AlienCountSliderInt;
    Label m_AlienCountLabel;
    Button m_SaveButton;
    Button m_CloseButton;

    void Start()
    {
        m_RootElement = GetComponent<UIDocument>().rootVisualElement;
        m_OpenButton = m_RootElement.Q<Button>(m_OpenButtonName);
        m_MatchSettingsOverlayElement = m_MatchSettingsOverlay.CloneTree();
        m_LanguageDropdownField = m_MatchSettingsOverlayElement.Q<DropdownField>(m_LanguageDropdownFieldName);
        m_TopicDropdownField = m_MatchSettingsOverlayElement.Q<DropdownField>(m_TopicDropdownFieldName);
        m_AlienCountSliderInt = m_MatchSettingsOverlayElement.Q<SliderInt>(m_AlienCountSliderName);
        m_AlienCountLabel = m_MatchSettingsOverlayElement.Q<Label>(m_AlienCountLabelName);
        m_SaveButton = m_MatchSettingsOverlayElement.Q<Button>(m_SaveButtonName);
        m_CloseButton = m_MatchSettingsOverlayElement.Q<Button>(m_CloseButtonName);

        _ = FetchFieldOptions();

        m_AlienCountSliderInt.RegisterValueChangedCallback(e => m_AlienCountLabel.text = e.newValue.ToString());
        m_AlienCountLabel.text = m_AlienCountSliderInt.value.ToString();

        m_OpenButton.clicked += LoadMatchSettings;
        m_OpenButton.clicked += () => UIToolkitUtils.ShowOverlay(m_RootElement, m_MatchSettingsOverlayElement);

        m_SaveButton.clicked += SaveMatchSettings;

        m_CloseButton.clicked += LoadMatchSettings;
        m_CloseButton.clicked += () => UIToolkitUtils.HideOverlay(m_MatchSettingsOverlayElement);

        UIToolkitUtils.MonitorElementEnablement(m_SaveButton, IsSettingsChanged);
        LoadMatchSettings();
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

    bool IsSettingsChanged()
    {
        bool isLanguageChanged = LocalizationUtils.ConvertNativeNameToCode(m_LanguageDropdownField.value) != MatchSettingsManager.Locale;
        bool isTopicChanged = m_TopicDropdownField.value != MatchSettingsManager.Topic;
        bool isAlienCountChanged = m_AlienCountSliderInt.value != MatchSettingsManager.AlienCount;

        return isLanguageChanged || isTopicChanged || isAlienCountChanged;
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