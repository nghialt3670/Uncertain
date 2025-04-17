using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using System.Collections;

public class GameplaySettingsManager : MonoBehaviour
{
    const int MIN_PLAYERS = 3;
    const int MAX_PLAYERS = 10;

    private int numPlayers = 3;
    private string language;
    private string topic;
    private int numAliens = 1;
    private int numHumans = 2;

    public GameObject playerList;
    public GameObject playerItem;

    public GameObject languageDropdown;
    public GameObject topicDropdown;
    public GameObject rolesSlider;

    public GameObject numPlayersDisplay;
    public GameObject numAliensDisplay;
    public GameObject numHumansDisplay;

    void Awake()
    {
        numPlayers = playerList.transform.childCount;
        numAliens = 1;
        numHumans = numPlayers - numAliens;

        // Assign remove events to all existing player items
        for (int i = 0; i < playerList.transform.childCount; i++)
        {
            GameObject player = playerList.transform.GetChild(i).gameObject;
            Button removeButton = player.GetComponentInChildren<Button>();
            if (removeButton != null)
            {
                int playerIndex = i; // Capture the index in a local variable
                removeButton.onClick.AddListener(() => RemovePlayer(playerIndex));
            }
        }

        UpdateRolesDisplay();
    }

    private void ScrollToBottom()
    {
        Canvas.ForceUpdateCanvases();
        if (playerList.transform.parent != null)
        {
            ScrollRect scrollRect = playerList.transform.parent.GetComponentInParent<ScrollRect>();
            if (scrollRect != null)
            {
                StartCoroutine(SmoothScrollToBottom(scrollRect));
            }
        }
    }

    private IEnumerator SmoothScrollToBottom(ScrollRect scrollRect)
    {
        float duration = 0.3f; // Duration of the scroll animation in seconds
        float elapsedTime = 0f;
        float startPosition = scrollRect.normalizedPosition.y;
        float targetPosition = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / duration;
            
            // Use smoothstep for easing
            float easedTime = normalizedTime * normalizedTime * (3f - 2f * normalizedTime);
            
            float newY = Mathf.Lerp(startPosition, targetPosition, easedTime);
            scrollRect.normalizedPosition = new Vector2(scrollRect.normalizedPosition.x, newY);
            yield return null;
        }

        // Ensure we end up exactly at the target position
        scrollRect.normalizedPosition = new Vector2(scrollRect.normalizedPosition.x, targetPosition);
    }

    public void AddPlayer()
    {
        numPlayers++;
        numHumans++;
        GameObject player = Instantiate(playerItem, playerList.transform);
        
        Button removeButton = player.GetComponentInChildren<Button>();
        if (removeButton != null)
        {
            int playerIndex = playerList.transform.childCount - 1;
            removeButton.onClick.AddListener(() => RemovePlayer(playerIndex));
        }
        
        UpdateRolesDisplay();
        ScrollToBottom();
    }

    public void RemovePlayer(int index)
    {
        if (numPlayers <= MIN_PLAYERS)
        {
            return;
        }

        numPlayers--;

        if (numAliens < numPlayers / 3)
        {
            numHumans--;
        }
        else 
        {
            numAliens--;
        }

        Destroy(playerList.transform.GetChild(index).gameObject);
        UpdateRolesDisplay();
    }

    public void UpdateLanguage()
    {
        language = languageDropdown.GetComponent<TMP_Dropdown>().value.ToString();
    }

    public void UpdateTopic()
    {
        topic = topicDropdown.GetComponent<TMP_Dropdown>().value.ToString();
    }

    public void UpdateRoles()
    {
        numAliens = (int)rolesSlider.GetComponent<Slider>().value;
        numHumans = numPlayers - numAliens;
        UpdateRolesDisplay();
    }

    private void UpdateRolesDisplay()
    {
        numPlayersDisplay.GetComponent<TMP_Text>().text = numPlayers.ToString();
        numAliensDisplay.GetComponent<TMP_Text>().text = numAliens.ToString();
        numHumansDisplay.GetComponent<TMP_Text>().text = numHumans.ToString();
        
        Slider slider = rolesSlider.GetComponent<Slider>();
        slider.maxValue = numPlayers;  // Set max value to total players for visual scale
        slider.minValue = 0;
        
        // Clamp the actual value to maximum allowed aliens (1/3 of players)
        int maxAllowedAliens = (int)numPlayers / 3;
        slider.value = Mathf.Clamp(numAliens, 1, maxAllowedAliens);
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
    
    
}
