using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization.Settings;

[CreateAssetMenu(fileName = "GameLoadingStrategy", menuName = "Scriptable Objects/GameLoadingStrategy")]
public class GameLoadingStrategy : LoadingStrategy
{
    public override Task<LoadingResult> Load()
    {
        LoadingResult result;
        
        try
        {
            _ = LocalizationSettings.InitializationOperation.WaitForCompletion();
            result = new LoadingResult
            {
                isSuccess = true,
            };
        }
        catch (Exception exception)
        {
            result = new LoadingResult
            {
                isSuccess = false,
                exception = exception,
            };
        }

        return Task.FromResult(result);
    }
}
