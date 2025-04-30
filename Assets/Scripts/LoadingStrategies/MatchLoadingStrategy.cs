using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "MatchLoadingStrategy", menuName = "Scriptable Objects/MatchLoadingStrategy")]
public class MatchLoadingStrategy : LoadingStrategy
{
    public override async Task<LoadingResult> Load()
    {
        try
        {
            string wordsEndpoint = "single-device/v1/generate";
            
            List<string> assignedWords = MatchSettingsManager.GetAssignedWords();
            List<WordPair> exceptedPairs = new List<WordPair>();
            
            // Create word pairs from assigned words
            for (int i = 0; i < assignedWords.Count; i++)
            {
                for (int j = i + 1; j < assignedWords.Count; j++)
                {
                    exceptedPairs.Add(new WordPair
                    {
                        first = assignedWords[i],
                        second = assignedWords[j]
                    });
                }
            }
            
            WordsRequest wordsRequest = new()
            {
                domain = MatchSettingsManager.Topic,
                language = LocalizationUtils.ConvertCodeToNativeName(MatchSettingsManager.Locale),
                exceptedPairs = exceptedPairs,
            };
            var wordsResponse = await ApiUtils.PostAsync<ApiResponse<WordsData>>(wordsEndpoint, wordsRequest);

            if (wordsResponse == null)
            {
                throw new Exception("Internal server error");
            }

            MatchSettingsManager.AssignRoles(wordsResponse.data.wordPair.first, wordsResponse.data.wordPair.second);
        }
        catch (Exception exception)
        {
            return new LoadingResult
            {
                isSuccess = false,
                exception = exception,
            };
        }

        return new LoadingResult
        {
            isSuccess = true,
        };
    }
}

[Serializable]
public class WordsRequest
{
    public string domain;
    public string language;
    public List<WordPair> exceptedPairs;
}

[Serializable]
public class WordsData
{
    public WordPair wordPair;
}

[Serializable]
public class WordPair
{
    public string first;
    public string second;
}