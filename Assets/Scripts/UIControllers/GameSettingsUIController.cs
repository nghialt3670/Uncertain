using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;

public class GameSettingsUIController : MonoBehaviour
{
    public VisualTreeAsset m_GameSettingsOverlay;

    public string m_OpenButtonName = "GameSettingsButton";
    public string m_LanguageDropdownFieldName = "LanguageDropdownField";
    public string m_VolumeSliderName = "VolumeSlider";
    public string m_SaveButtonName = "SaveButton";
    public string m_CloseButtonName = "CloseButton";

    private VisualElement m_BaseRoot;
    private VisualElement m_OpenButton;
    private VisualElement m_OverlayRoot;
    private DropdownField m_LanguageDropdownField;
    private Slider m_VolumeSlider;
    private Button m_SaveButton;
    private Button m_CloseButton;

    private IVisualElementScheduledItem m_SaveButtonMonitor;

    void Start()
    {
        LocalizationSettings.InitializationOperation.WaitForCompletion();

        m_BaseRoot = GetComponent<UIDocument>().rootVisualElement;
        m_OpenButton = m_BaseRoot.Q<VisualElement>(m_OpenButtonName);

        m_OverlayRoot = m_GameSettingsOverlay.CloneTree();
        m_OverlayRoot.StretchToParentSize();

        m_LanguageDropdownField = m_OverlayRoot.Q<DropdownField>(m_LanguageDropdownFieldName);
        m_VolumeSlider = m_OverlayRoot.Q<Slider>(m_VolumeSliderName);
        m_SaveButton = m_OverlayRoot.Q<Button>(m_SaveButtonName);
        m_CloseButton = m_OverlayRoot.Q<Button>(m_CloseButtonName);

        m_OpenButton.RegisterCallback<ClickEvent>(e => ShowOverlay());
        m_LanguageDropdownField.RegisterCallback<ChangeEvent<string>>(e =>
        {
            // Prevent events that come from labels being translated
            if (e.target == e.currentTarget && e.previousValue != e.newValue)
            {
                LocalizationUtils.SetLocaleByNativeName(e.newValue);
            }
        });
        m_VolumeSlider.RegisterValueChangedCallback(e => AudioListener.volume = e.newValue);
        m_SaveButton.clicked += SaveGameSettings;
        m_CloseButton.clicked += HideOverlay;

        StartSaveButtonMonitor();
        LoadGameSettings();
    }

    public void ShowOverlay()
    {
        LoadGameSettings();
        m_BaseRoot.Add(m_OverlayRoot);
    }

    public void HideOverlay()
    {
        LoadGameSettings();
        m_BaseRoot.Remove(m_OverlayRoot);
    }

    private void LoadGameSettings()
    {
        m_LanguageDropdownField.choices = LocalizationUtils.GetAvailableNativeNames();
        m_LanguageDropdownField.value = LocalizationUtils.ConvertCodeToNativeName(GameSettingsManager.Locale);
        m_VolumeSlider.value = GameSettingsManager.Volume;

        LocalizationUtils.SetLocaleByCode(GameSettingsManager.Locale);
        AudioListener.volume = GameSettingsManager.Volume;
    }

    private void SaveGameSettings()
    {
        GameSettingsManager.Locale = LocalizationUtils.ConvertNativeNameToCode(m_LanguageDropdownField.value);
        GameSettingsManager.Volume = m_VolumeSlider.value;
    }

    private void StartSaveButtonMonitor()
    {
        m_SaveButtonMonitor?.Pause();

        m_SaveButtonMonitor = m_SaveButton.schedule.Execute(() =>
        {
            bool isLanguageChanged = LocalizationUtils.ConvertNativeNameToCode(m_LanguageDropdownField.value) != GameSettingsManager.Locale;
            bool isVolumeChanged = !Mathf.Approximately(m_VolumeSlider.value, GameSettingsManager.Volume);

            m_SaveButton.SetEnabled(isLanguageChanged || isVolumeChanged);
        }).Every(10);
    }
}
