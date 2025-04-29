using System.Collections.Generic;
using UnityEngine;

public static class MatchSettingsManager
{
    public const int MIN_PLAYER_COUNT = 3;
    public const int MAX_PLAYER_COUNT = 10;
    public const int MIN_ALIEN_COUNT = 1;
    public const float MAX_ALIEN_FRACTION = 1f / 3;

    private const string DEFAULT_LOCALE = "en";
    private const string DEFAULT_TOPIC = "";
    private const int DEFAULT_PLAYER_COUNT = 3;
    private const int DEFAULT_ALIEN_COUNT = 1;

    private const string LOCALE_KEY = "LanguageCode";
    private const string TOPIC_KEY = "Topic";

    public static List<Player> Players { get; set; }

    static MatchSettingsManager()
    {
        Players = new List<Player>();
        for (int i = 0; i < DEFAULT_PLAYER_COUNT; i++)
        {
            Players.Add(new Player());
        }
        AlienCount = DEFAULT_ALIEN_COUNT;
    }

    public static string Locale
    {
        get => PlayerPrefs.GetString(LOCALE_KEY, DEFAULT_LOCALE);
        set
        {
            PlayerPrefs.SetString(LOCALE_KEY, value);
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
        get => Players.Count;
    }

    public static int AlienCount { get; set; }

    public static bool CanAddPlayer()
    {
        return Players.Count < MAX_PLAYER_COUNT;
    }

    public static bool CanRemovePlayer()
    {
        return Players.Count > MIN_PLAYER_COUNT;
    }

    public static int GetMinAlientCount()
    {
        return MIN_ALIEN_COUNT;
    }

    public static int GetMaxAlienCount()
    {
        return Mathf.FloorToInt(PlayerCount * MAX_ALIEN_FRACTION);
    }

    public static void AssignRoles(string firstWord, string secondWord)
    {
        foreach (var player in Players)
        {
            player.roles = new List<Role>();
            player.words = new List<string>();
            player.voteIndices = new List<int>();
        }

        List<int> alienIndexes = new List<int>();
        while (alienIndexes.Count < AlienCount)
        {
            int randomIndex = Random.Range(0, Players.Count);
            if (!alienIndexes.Contains(randomIndex))
            {
                alienIndexes.Add(randomIndex);
            }
        }

        for (int i = 0; i < Players.Count; i++)
        {
            Players[i].voteIndices.Add(-1);
            if (alienIndexes.Contains(i))
            {
                Players[i].roles.Add(Role.ALIEN);
                Players[i].words.Add(secondWord);
            }
            else
            {
                Players[i].roles.Add(Role.HUMAN);
                Players[i].words.Add(firstWord);
            }
        }
    }

    public static List<string> GetAssignedWords()
    {
        HashSet<string> allWords = new HashSet<string>();

        foreach (var player in Players)
        {
            if (player.words != null)
            {
                foreach (var word in player.words)
                {
                    allWords.Add(word);
                }
            }
        }

        return new List<string>(allWords);
    }

    public static void Reset()
    {
        PlayerPrefs.DeleteKey(LOCALE_KEY);
        PlayerPrefs.DeleteKey(TOPIC_KEY);
        PlayerPrefs.Save();
    }
}