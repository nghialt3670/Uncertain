using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using TMPro;

public class LocalizeDropdown : MonoBehaviour
{
    private TMP_Dropdown m_Dropdown;
    
    [Tooltip("The String Table Collection that contains all dropdown option translations")]
    public string m_TableCollectionName;

    [Tooltip("Use the option text as keys in the String Table")]
    public bool m_UseOptionTextAsKey = true;
    
    // Store the original options to use as keys
    private List<string> m_OriginalOptions = new List<string>();

    private void Awake()
    {
        m_Dropdown = GetComponent<TMP_Dropdown>();
        
        // Store the original options as they are set in the editor
        foreach (var option in m_Dropdown.options)
        {
            m_OriginalOptions.Add(option.text);
        }
    }

    private void Start()
    {
        UpdateDropdown();
        LocalizationSettings.SelectedLocaleChanged += _ => UpdateDropdown();
    }

    private void UpdateDropdown()
    {
        if (m_OriginalOptions.Count == 0)
            return;
            
        m_Dropdown.ClearOptions();
        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();

        Debug.Log("Original Options: " + m_OriginalOptions.Count);

        foreach (var originalText in m_OriginalOptions)
        {
            string key = m_UseOptionTextAsKey ? originalText : originalText.ToLower().Replace(" ", "_");
            
            // Get the localized string from the table using the original text as key
            var localizedString = new LocalizedString(m_TableCollectionName, key);

            Debug.Log("Localized String: " + localizedString.GetLocalizedString());
            
            // Get the translated value
            string translatedOption = localizedString.GetLocalizedString();
            
            // If no translation is found, use the original text
            if (string.IsNullOrEmpty(translatedOption))
            {
                translatedOption = originalText;
            }
            
            // Add it to the dropdown
            options.Add(new TMP_Dropdown.OptionData(translatedOption));
        }

        m_Dropdown.AddOptions(options);
        
        // Restore previous selection if possible
        if (m_Dropdown.value >= options.Count)
        {
            m_Dropdown.value = 0;
        }
    }
}
