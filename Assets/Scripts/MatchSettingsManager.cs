using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System;

public class GameplaySettingsManager : MonoBehaviour
{
    const int MIN_PLAYERS = 3;
    const int MAX_PLAYERS = 10;

    private int m_NumPlayers = 3;
    private string m_Language;
    private string m_Topic;
    private int m_NumAliens = 1;
    private int m_NumHumans = 2;

    [SerializeField] private GameObject m_PlayerList;
    [SerializeField] private GameObject m_PlayerItem;

    [SerializeField] private GameObject m_LanguageDropdown;
    [SerializeField] private GameObject m_TopicDropdown;
    [SerializeField] private GameObject m_RolesSlider;

    [SerializeField] private GameObject m_NumPlayersDisplay;
    [SerializeField] private GameObject m_NumAliensDisplay;
    [SerializeField] private GameObject m_NumHumansDisplay;

    [SerializeField] private string m_GameOptionsApiUrl;

    private Dictionary<string, string[]> m_LanguageTopics = new Dictionary<string, string[]>();

    void Awake()
    {
        m_NumPlayers = m_PlayerList.transform.childCount;
        m_NumAliens = 1;
        m_NumHumans = m_NumPlayers - m_NumAliens;

        // Assign remove events to all existing player items
        for (int i = 0; i < m_PlayerList.transform.childCount; i++)
        {
            GameObject player = m_PlayerList.transform.GetChild(i).gameObject;
            Button removeButton = player.GetComponentInChildren<Button>();
            if (removeButton != null)
            {
                int playerIndex = i; // Capture the index in a local variable
                removeButton.onClick.AddListener(() => RemovePlayer(playerIndex));
            }
        }
        StartCoroutine(FetchGameOptions());
        UpdateRolesDisplay();
    }

    public void AddPlayer()
    {
        m_NumPlayers++;
        m_NumHumans++;
        GameObject player = Instantiate(m_PlayerItem, m_PlayerList.transform);
        
        Button removeButton = player.GetComponentInChildren<Button>();
        if (removeButton != null)
        {
            int playerIndex = m_PlayerList.transform.childCount - 1;
            removeButton.onClick.AddListener(() => RemovePlayer(playerIndex));
        }
        
        UpdateRolesDisplay();
    }

    public void RemovePlayer(int index)
    {
        if (m_NumPlayers <= MIN_PLAYERS)
        {
            return;
        }

        m_NumPlayers--;

        if (m_NumAliens < m_NumPlayers / 3)
        {
            m_NumHumans--;
        }
        else 
        {
            m_NumAliens--;
        }

        Destroy(m_PlayerList.transform.GetChild(index).gameObject);
        UpdateRolesDisplay();
    }

    public void UpdateLanguage()
    {
        m_Language = m_LanguageDropdown.GetComponent<TMP_Dropdown>().value.ToString();
        UpdateTopics();
    }

    public void UpdateTopic()
    {
        m_Topic = m_TopicDropdown.GetComponent<TMP_Dropdown>().value.ToString();
    }

    public void UpdateRoles()
    {
        m_NumAliens = (int)m_RolesSlider.GetComponent<Slider>().value;
        m_NumHumans = m_NumPlayers - m_NumAliens;
        UpdateRolesDisplay();
    }

    private void UpdateRolesDisplay()
    {
        m_NumPlayersDisplay.GetComponent<TMP_Text>().text = m_NumPlayers.ToString();
        m_NumAliensDisplay.GetComponent<TMP_Text>().text = m_NumAliens.ToString();
        m_NumHumansDisplay.GetComponent<TMP_Text>().text = m_NumHumans.ToString();
        
        Slider slider = m_RolesSlider.GetComponent<Slider>();
        slider.maxValue = m_NumPlayers;  // Set max value to total players for visual scale
        slider.minValue = 0;
        
        // Clamp the actual value to maximum allowed aliens (1/3 of players)
        int maxAllowedAliens = (int)m_NumPlayers / 3;
        slider.value = Mathf.Clamp(m_NumAliens, 1, maxAllowedAliens);
        slider.wholeNumbers = true;

        // Add a listener to enforce the 1/3 limit when sliding
        slider.onValueChanged.RemoveAllListeners();
        slider.onValueChanged.AddListener((float value) => {
            if (value > maxAllowedAliens) {
                slider.value = maxAllowedAliens;
            } else if (value < 1) {
                slider.value = 1;
            }
        });
    }
    
    private IEnumerator FetchGameOptions()
    {
        UnityWebRequest request = UnityWebRequest.Get(m_GameOptionsApiUrl);
        request.certificateHandler = new BypassCertificateHandler();
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error fetching game options: " + request.error);
            yield break;
        }

        string json = request.downloadHandler.text;
        Debug.Log("API Response: " + json);

        try
        {
            // Manual parsing approach since JsonUtility can't handle dynamic properties
            Dictionary<string, string[]> parsedTopics = new Dictionary<string, string[]>();
            
            // Extract language codes and topic arrays using string manipulation
            int dataIndex = json.IndexOf("\"data\":");
            if (dataIndex == -1)
            {
                Debug.LogError("No 'data' field found in response");
                yield break;
            }
            
            // Find the start of the data object
            int dataStartIndex = json.IndexOf('{', dataIndex);
            if (dataStartIndex == -1)
            {
                Debug.LogError("Invalid data format");
                yield break;
            }
            
            // Find the end of the data object by counting braces
            int dataEndIndex = -1;
            int braceCount = 1;
            for (int i = dataStartIndex + 1; i < json.Length; i++)
            {
                if (json[i] == '{') braceCount++;
                else if (json[i] == '}')
                {
                    braceCount--;
                    if (braceCount == 0)
                    {
                        dataEndIndex = i;
                        break;
                    }
                }
            }
            
            if (dataEndIndex == -1)
            {
                Debug.LogError("Could not find end of data object");
                yield break;
            }
            
            // Extract the data JSON object
            string dataJson = json.Substring(dataStartIndex, dataEndIndex - dataStartIndex + 1);
            
            // Extract language keys using regular expressions
            System.Text.RegularExpressions.Regex languageKeyRegex = 
                new System.Text.RegularExpressions.Regex("\"([^\"]+)\":\\s*\\[");
            var matches = languageKeyRegex.Matches(dataJson);
            
            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                string languageKey = match.Groups[1].Value;
                int arrayStartIndex = dataJson.IndexOf('[', match.Index);
                
                if (arrayStartIndex != -1)
                {
                    // Find the end of the array
                    int arrayEndIndex = -1;
                    int bracketCount = 1;
                    for (int i = arrayStartIndex + 1; i < dataJson.Length; i++)
                    {
                        if (dataJson[i] == '[') bracketCount++;
                        else if (dataJson[i] == ']')
                        {
                            bracketCount--;
                            if (bracketCount == 0)
                            {
                                arrayEndIndex = i;
                                break;
                            }
                        }
                    }
                    
                    if (arrayEndIndex != -1)
                    {
                        // Parse the array content
                        string arrayContent = dataJson.Substring(arrayStartIndex + 1, arrayEndIndex - arrayStartIndex - 1);
                        string[] topics = ParseStringArray(arrayContent);
                        parsedTopics[languageKey] = topics;
                    }
                }
            }
            
            // Clear existing data
            m_LanguageTopics.Clear();
            
            // Add language options to dropdown
            TMP_Dropdown languageDD = m_LanguageDropdown.GetComponent<TMP_Dropdown>();
            languageDD.ClearOptions();
            
            List<string> languageOptions = new List<string>();
            
            foreach (var language in parsedTopics)
            {
                m_LanguageTopics[language.Key] = language.Value;
                languageOptions.Add(language.Key);
            }
            
            languageDD.AddOptions(languageOptions);
            
            // Set up language change listener
            languageDD.onValueChanged.RemoveAllListeners();
            languageDD.onValueChanged.AddListener(delegate { UpdateTopics(); });
            
            // Initialize topics for first language
            if (languageOptions.Count > 0)
            {
                UpdateTopics();
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error parsing game options: " + e.ToString());
        }
    }
    
    private string[] ParseStringArray(string arrayContent)
    {
        List<string> items = new List<string>();
        
        // Match all strings in quotes
        System.Text.RegularExpressions.Regex stringRegex = 
            new System.Text.RegularExpressions.Regex("\"([^\"]*)\"");
            
        var matches = stringRegex.Matches(arrayContent);
        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            items.Add(match.Groups[1].Value);
        }
        
        return items.ToArray();
    }

    private void UpdateTopics()
    {
        TMP_Dropdown languageDD = m_LanguageDropdown.GetComponent<TMP_Dropdown>();
        TMP_Dropdown topicDD = m_TopicDropdown.GetComponent<TMP_Dropdown>();
        
        // Get the selected language code
        string selectedLanguage = languageDD.options[languageDD.value].text;
        Debug.Log("Updating topics for language: " + selectedLanguage);
        
        if (m_LanguageTopics.ContainsKey(selectedLanguage))
        {
            topicDD.ClearOptions();
            List<string> topics = new List<string>(m_LanguageTopics[selectedLanguage]);
            Debug.Log("Found " + topics.Count + " topics for " + selectedLanguage);
            topicDD.AddOptions(topics);
        }
        else
        {
            Debug.LogWarning("No topics found for language: " + selectedLanguage);
        }
    }

    private class BypassCertificateHandler : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            return true; // Bypasses all certificate validation
        }
    }
}

