using UnityEngine;

public static class GameSettingsManager
{
    public const int PLAYER_NAME_MAX_LENGTH = 15;

    private const string DEFAULT_LOCALE = "en";
    private const float DEFAULT_VOLUME = 0.5f;

    private const string LOCALE_KEY = "Locale";
    private const string VOLUME_KEY = "Volume";

    public static string Locale
    {
        get => PlayerPrefs.GetString(LOCALE_KEY, DEFAULT_LOCALE);
        set
        {
            PlayerPrefs.SetString(LOCALE_KEY, value);
            PlayerPrefs.Save();
        }
    }

    public static float Volume
    {
        get => PlayerPrefs.GetFloat(VOLUME_KEY, DEFAULT_VOLUME);
        set
        {
            PlayerPrefs.SetFloat(VOLUME_KEY, value);
            PlayerPrefs.Save();
        }
    }

    public static void Reset()
    {
        PlayerPrefs.DeleteKey(LOCALE_KEY);
        PlayerPrefs.DeleteKey(VOLUME_KEY);
        PlayerPrefs.Save();
    }
}
