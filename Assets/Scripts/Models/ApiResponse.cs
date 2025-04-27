using System;

[Serializable]
public class ApiResponse<T>
{
    public string code;
    public string message;
    public T data;
}