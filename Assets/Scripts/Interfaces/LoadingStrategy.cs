using System;
using System.Threading.Tasks;
using UnityEngine;

public class LoadingResult
{
    public bool isSuccess;
    public Exception exception;
}

public abstract class LoadingStrategy : ScriptableObject
{
    public abstract Task<LoadingResult> Load();
} 