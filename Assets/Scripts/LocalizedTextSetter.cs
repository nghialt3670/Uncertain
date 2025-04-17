using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;

public class LocalizedTextSetter : MonoBehaviour
{
    public LocalizedString localizedString;

    void Start()
    {
        localizedString.StringChanged += UpdateText;
    }

    void UpdateText(string value)
    {
        GetComponent<TMPro.TextMeshProUGUI>().text = value;
    }
}
