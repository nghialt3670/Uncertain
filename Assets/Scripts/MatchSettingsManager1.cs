using System.Collections.Generic;
using UnityEngine;

public static class MatchSettingsManager
{
    public const int MIN_PLAYERS = 3;
    public const int MAX_PLAYERS = 10;
    public const int MIN_ALIENS = 1;
    public const float MAX_ALIEN_FRACTION = 1 / 3;

    private const string DEFAULT_LANGUAGE_CODE = "en";
    private const string DEFAULT_TOPIC = "";
    private const int DEFAULT_PLAYER_COUNT = 3;
    private const int DEFAULT_ALIEN_COUNT = 1;

    private const string LANGUAGE_CODE_KEY = "LanguageCode";
    private const string TOPIC_KEY = "Topic";
    private const string PLAYER_COUNT_KEY = "PlayerCount";
    private const string ALIEN_COUNT_KEY = "AlienCount";

    public static List<Player> Players { get; set; }

    static MatchSettingsManager()
    {
        Players = new List<Player>();
        for (int i = 0; i < MIN_PLAYERS; i++)
        {
            Players.Add(new Player());
        }
    }

    public static string LanguageCode
    {
        get => PlayerPrefs.GetString(LANGUAGE_CODE_KEY, DEFAULT_LANGUAGE_CODE);
        set
        {
            PlayerPrefs.SetString(LANGUAGE_CODE_KEY, value);
            PlayerPrefs.Save();
        }
    }

    public static string Topic
    {
        get => PlayerPrefs.GetString(TOPIC_KEY, DEFAULT_TOPIC);
        set
        {
            PlayerPrefs.SetString(TOPIC_KEY, value);
            PlayerPrefs.Save();
        }
    }

    public static int PlayerCount
    {
        get => PlayerPrefs.GetInt(PLAYER_COUNT_KEY, DEFAULT_PLAYER_COUNT);
        set
        {
            PlayerPrefs.SetInt(PLAYER_COUNT_KEY, value);
            PlayerPrefs.Save();
        }
    }

    public static int AlienCount
    {
        get => PlayerPrefs.GetInt(ALIEN_COUNT_KEY, DEFAULT_ALIEN_COUNT);
        set
        {
            PlayerPrefs.SetInt(ALIEN_COUNT_KEY, value);
            PlayerPrefs.Save();
        }
    }

    public static bool CanAddPlayer()
    {
        return Players.Count < MAX_PLAYERS;
    }

    public static bool CanRemovePlayer()
    {
        return Players.Count > MIN_PLAYERS;
    }

    public static void Reset()
    {
        PlayerPrefs.DeleteKey(LANGUAGE_CODE_KEY);
        PlayerPrefs.DeleteKey(TOPIC_KEY);
        PlayerPrefs.Save();
    }
}