using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

[RequireComponent(typeof(TMP_Dropdown))]
public class DropdownOptionsFetcher : MonoBehaviour
{
    [Tooltip("API URL to fetch dropdown options from")]
    public string apiUrl = "https://example.com/api/options";
    
    private TMP_Dropdown dropdown;

    void Awake()
    {
        // Get the dropdown component on this same GameObject
        dropdown = GetComponent<TMP_Dropdown>();
        
        if (dropdown == null)
        {
            Debug.LogError("No TMP_Dropdown component found on this GameObject");
            return;
        }
    }

    void Start()
    {
        StartCoroutine(FetchOptions());
    }

    IEnumerator FetchOptions()
    {
        UnityWebRequest request = UnityWebRequest.Get(apiUrl);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error fetching dropdown options: " + request.error);
            dropdown.ClearOptions();
            dropdown.AddOptions(new List<string> { "Error fetching options" });
            yield break;
        }

        string json = request.downloadHandler.text;
        ApiResponse response = JsonUtility.FromJson<ApiResponse>(json);

        if (response != null && response.data != null)
        {
            List<string> options = new List<string>();
            foreach (var item in response.data)
            {
                options.Add(item);
            }

            dropdown.ClearOptions();
            dropdown.AddOptions(options);
        }
        else
        {
            Debug.LogWarning("No 'data' field found in response or invalid response format.");
            dropdown.ClearOptions();
            dropdown.AddOptions(new List<string> { "Invalid data format" });
        }
    }

    [System.Serializable]
    private class ApiResponse
    {
        public List<string> data;
    }
}