using System.Collections.Generic;
using System.Linq;
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

    public static PlayerNamesValidationResult ValidatePlayerNames()
    {
        HashSet<string> existingNames = new HashSet<string>();

        foreach (Player player in Players)
        {
            if (player.name == string.Empty)
            {
                return PlayerNamesValidationResult.CONTAINS_EMPTY;
            }

            if (existingNames.Contains(player.name))
            {
                return PlayerNamesValidationResult.CONTAINS_DUPLICATE;
            }

            existingNames.Add(player.name);
        }

        return PlayerNamesValidationResult.VALID;
    }

    public static void AssignRoles(string firstWord, string secondWord)
    {
        foreach (var player in Players)
        {
            player.roles = new List<PlayerRole>();
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
                Players[i].roles.Add(PlayerRole.ALIEN);
                Players[i].words.Add(secondWord);
            }
            else
            {
                Players[i].roles.Add(PlayerRole.HUMAN);
                Players[i].words.Add(firstWord);
            }
        }
    }

    public static List<string> GetAssignedWords()
    {
        HashSet<string> assignedWords = new HashSet<string>();

        foreach (var player in Players)
        {
            if (player.words != null)
            {
                foreach (var word in player.words)
                {
                    assignedWords.Add(word);
                }
            }
        }

        return new List<string>(assignedWords);
    }

    public static void Vote(Player player)
    {
        player.voteIndices[^1] = Players.Select(player => player.voteIndices[^1]).Min() + 1;
    }

    public static bool IsEndGame()
    {
        return IsAliensWin() || IsHumansWin();
    }

    public static bool IsAliensWin()
    {
        int unvotedAlienCount = GetUnvotedAlienCount();
        int unvotedHumanCount = GetUnvotedHumanCount();

        return unvotedAlienCount != 0 && unvotedAlienCount == unvotedHumanCount;
    }

    public static bool IsHumansWin()
    {
        return GetUnvotedAlienCount() == 0;
    }

    public static int GetUnvotedAlienCount()
    {
        return Players.Where(player => IsAlien(player) && !IsVoted(player)).Count();
    }

    public static int GetUnvotedHumanCount()
    {
        return Players.Where(player => IsHuman(player) && !IsVoted(player)).Count();
    }

    public static bool IsAlien(Player player)
    {
        return player.roles[^1] == PlayerRole.ALIEN;
    }

    public static bool IsHuman(Player player)
    {
        return player.roles[^1] == PlayerRole.HUMAN;
    }

    public static bool IsVoted(Player player)
    {
        return player.voteIndices[^1] != -1;
    }

    public static bool IsNewPlayer(Player player)
    {
        return player.roles.Count == 0;
    }

    public static void Reset()
    {
        PlayerPrefs.DeleteKey(LOCALE_KEY);
        PlayerPrefs.DeleteKey(TOPIC_KEY);
        PlayerPrefs.Save();
    }
}

public enum PlayerNamesValidationResult
{
    VALID,
    CONTAINS_EMPTY,
    CONTAINS_DUPLICATE,
}