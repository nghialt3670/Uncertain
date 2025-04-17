using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using TMPro;
using System.Collections;

public class LocaleDropdown : MonoBehaviour
{
    private TMP_Dropdown m_LocaleDropdown;

    private void Start()
    {
        m_LocaleDropdown = GetComponent<TMP_Dropdown>();
        StartCoroutine(SetupDropdown());
    }

    private IEnumerator SetupDropdown()
    {
        // Wait for localization system to initialize
        yield return LocalizationSettings.InitializationOperation;

        m_LocaleDropdown.ClearOptions();
        foreach (var locale in LocalizationSettings.AvailableLocales.Locales)
        {
            m_LocaleDropdown.options.Add(new TMP_Dropdown.OptionData(locale.Identifier.ToString()));
        }

        // Set current locale in dropdown
        var selected = LocalizationSettings.AvailableLocales.Locales.IndexOf(LocalizationSettings.SelectedLocale);
        m_LocaleDropdown.value = selected;
        m_LocaleDropdown.RefreshShownValue();

        // Listen for changes
        m_LocaleDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
    }

    private void OnDropdownValueChanged(int index)
    {
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
    }
}
