using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;

public class GameSettingsUIController : MonoBehaviour
{
    [SerializeField] VisualTreeAsset m_GameSettingsOverlay;

    readonly string m_OpenButtonName = "GameSettingsButton";
    readonly string m_LanguageDropdownFieldName = "LanguageDropdownField";
    readonly string m_VolumeSliderName = "VolumeSlider";
    readonly string m_ReturnButtonName = "ReturnButton";
    readonly string m_SaveButtonName = "SaveButton";
    readonly string m_CloseButtonName = "CloseButton";

    VisualElement m_RootElement;
    VisualElement m_OpenButton;
    VisualElement m_OverlayElement;
    DropdownField m_LanguageDropdownField;
    Slider m_VolumeSlider;
    Button m_ReturnButton;
    Button m_SaveButton;
    Button m_CloseButton;
    
    void Start()
    {
        m_RootElement = GetComponent<UIDocument>().rootVisualElement;
        m_OverlayElement = m_GameSettingsOverlay.CloneTree();
        m_OpenButton = m_RootElement.Q<VisualElement>(m_OpenButtonName);
        m_LanguageDropdownField = m_OverlayElement.Q<DropdownField>(m_LanguageDropdownFieldName);
        m_VolumeSlider = m_OverlayElement.Q<Slider>(m_VolumeSliderName);
        m_ReturnButton = m_OverlayElement.Q<Button>(m_ReturnButtonName);
        m_SaveButton = m_OverlayElement.Q<Button>(m_SaveButtonName);
        m_CloseButton = m_OverlayElement.Q<Button>(m_CloseButtonName);
        
        m_OpenButton.RegisterCallback<ClickEvent>(e => LoadGameSettings());
        m_OpenButton.RegisterCallback<ClickEvent>(e => UIToolkitUtils.ShowOverlay(m_RootElement, m_OverlayElement));

        m_LanguageDropdownField.RegisterCallback<ChangeEvent<string>>(e =>
        {
            // Prevent events that come from labels being translated
            if (e.target == e.currentTarget && e.previousValue != e.newValue)
            {
                LocalizationUtils.SetLocaleByNativeName(e.newValue);
            }
        });

        m_VolumeSlider.RegisterValueChangedCallback(e => AudioListener.volume = e.newValue);

        m_ReturnButton.clicked += SceneUtils.Return;

        m_SaveButton.clicked += SaveGameSettings;

        m_CloseButton.clicked += LoadGameSettings;
        m_CloseButton.clicked += () => UIToolkitUtils.HideOverlay(m_OverlayElement);

        UIToolkitUtils.MonitorElementEnablement(m_SaveButton, IsSettingsChanged);
        LoadGameSettings();
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

    private bool IsSettingsChanged()
    {
        bool isLanguageChanged = LocalizationUtils.ConvertNativeNameToCode(m_LanguageDropdownField.value) != GameSettingsManager.Locale;
        bool isVolumeChanged = !Mathf.Approximately(m_VolumeSlider.value, GameSettingsManager.Volume);
        
        return isLanguageChanged || isVolumeChanged;
    }
}