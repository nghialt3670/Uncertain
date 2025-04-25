using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Mono.Cecil.Cil;
using System.Threading;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;
using System.Threading.Tasks;

public class MatchSettingsUIController : MonoBehaviour
{
    public VisualTreeAsset m_MatchSettingsOverlay;

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

        m_AlienCountSliderInt.lowValue = MatchSettingsManager.GetMinAlientCount();
        m_AlienCountSliderInt.highValue = MatchSettingsManager.GetMaxAlienCount();
        m_AlienCountSliderInt.value = m_AlienCountSliderInt.lowValue;
        m_AlienCountLabel.RegisterValueChangedCallback(e => m_AlienCountLabel.text = e.newValue.ToString());

        m_OpenButton.RegisterCallback<ClickEvent>(e => ShowOverlay());
        m_SaveButton.clicked += SaveMatchSettings;
        m_CloseButton.clicked += HideOverlay;
    }

    private void Update()
    {
        m_AlienCountSliderInt.lowValue = MatchSettingsManager.GetMinAlientCount();
        m_AlienCountSliderInt.highValue = MatchSettingsManager.GetMaxAlienCount();
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
        m_LanguageDropdownField.value = LocalizationUtils.ConvertCodeToNativeName(MatchSettingsManager.LanguageCode);
        m_AlienCountSliderInt.value = MatchSettingsManager.AlienCount;
    }

    private void SaveMatchSettings()
    {
        MatchSettingsManager.LanguageCode = LocalizationUtils.ConvertNativeNameToCode(m_LanguageDropdownField.value);
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
        string languagesEndpoint = "single-device/v1/languages";
        var languagesResponse = await ApiUtils.GetAsync<ApiResponse<List<LanguageDetail>>>(languagesEndpoint);

        if (languagesResponse == null) return;

        m_LanguageDropdownField.choices = languagesResponse.data
            .Select(detail => detail.languageCode)
            .Select(LocalizationUtils.ConvertCodeToNativeName)
            .ToList();

        m_LanguageDropdownField.value = m_LanguageDropdownField.choices
            .FirstOrDefault(choice => choice == LocalizationUtils.ConvertCodeToNativeName(MatchSettingsManager.LanguageCode));
    }

    private async Task FetchTopicOptions()
    {
        string languageCode = LocalizationUtils.ConvertNativeNameToCode(m_LanguageDropdownField.value);
        string topicsEndpoint = $"single-device/v1/topics?languageCode={languageCode}";
        var topicsResponse = await ApiUtils.GetAsync<ApiResponse<List<TopicDetail>>>(topicsEndpoint);

        if (topicsResponse == null) return;

        m_TopicDropdownField.choices = topicsResponse.data
            .Select(detail => detail.topicName)
            .ToList();

        m_LanguageDropdownField.value = m_LanguageDropdownField.choices
            .FirstOrDefault(choice => choice == MatchSettingsManager.Topic);
    }


}

[Serializable]
public class ApiResponse<T>
{
    public string code;
    public string message;
    public T data;
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