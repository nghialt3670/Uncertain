using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Static utility class for handling API requests in Unity
/// </summary>
public static class ApiUtils
{
    private static string BaseUrl = "https://52.79.250.196/api/";
    private static readonly int DefaultTimeout = 10; // seconds

    #region GET Requests

    /// <summary>
    /// Performs a GET request to the specified endpoint
    /// </summary>
    public static async Task<string> GetAsync(string endpoint, Dictionary<string, string> headers = null)
    {
        string url = BaseUrl + endpoint;
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.certificateHandler = new BypassCertificateHandler();

            // Add default timeout
            request.timeout = DefaultTimeout;

            // Add headers if provided
            AddRequestHeaders(request, headers);

            return await SendRequestAsync(request);
        }
    }

    /// <summary>
    /// Performs a GET request and deserializes the JSON response to the specified type
    /// </summary>
    public static async Task<T> GetAsync<T>(string endpoint, Dictionary<string, string> headers = null)
    {
        string responseJson = await GetAsync(endpoint, headers);
        if (string.IsNullOrEmpty(responseJson))
            return default;

        return JsonUtility.FromJson<T>(responseJson);
    }

    #endregion

    #region POST Requests

    /// <summary>
    /// Performs a POST request with JSON body to the specified endpoint
    /// </summary>
    public static async Task<string> PostAsync(string endpoint, object data, Dictionary<string, string> headers = null)
    {
        string url = BaseUrl + endpoint;
        string jsonData = JsonUtility.ToJson(data);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            // Add default timeout
            request.timeout = DefaultTimeout;

            // Add headers if provided
            AddRequestHeaders(request, headers);

            return await SendRequestAsync(request);
        }
    }

    /// <summary>
    /// Performs a POST request and deserializes the JSON response to the specified type
    /// </summary>
    public static async Task<T> PostAsync<T>(string endpoint, object data, Dictionary<string, string> headers = null)
    {
        string responseJson = await PostAsync(endpoint, data, headers);
        if (string.IsNullOrEmpty(responseJson))
            return default;

        return JsonUtility.FromJson<T>(responseJson);
    }

    #endregion

    #region PUT Requests

    public static async Task<string> PutAsync(string endpoint, object data, Dictionary<string, string> headers = null)
    {
        string url = BaseUrl + endpoint;
        string jsonData = JsonUtility.ToJson(data);

        using (UnityWebRequest request = new UnityWebRequest(url, "PUT"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            // Add default timeout
            request.timeout = DefaultTimeout;

            // Add headers if provided
            AddRequestHeaders(request, headers);

            return await SendRequestAsync(request);
        }
    }

    #endregion

    #region DELETE Requests

    public static async Task<string> DeleteAsync(string endpoint, Dictionary<string, string> headers = null)
    {
        string url = BaseUrl + endpoint;
        using (UnityWebRequest request = UnityWebRequest.Delete(url))
        {
            // Add a download handler to get response
            request.downloadHandler = new DownloadHandlerBuffer();

            // Add default timeout
            request.timeout = DefaultTimeout;

            // Add headers if provided
            AddRequestHeaders(request, headers);

            return await SendRequestAsync(request);
        }
    }

    #endregion

    #region Helper Methods

    private static void AddRequestHeaders(UnityWebRequest request, Dictionary<string, string> headers)
    {
        if (headers != null)
        {
            foreach (var header in headers)
            {
                request.SetRequestHeader(header.Key, header.Value);
            }
        }
    }

    private static async Task<string> SendRequestAsync(UnityWebRequest request)
    {
        try
        {
            var operation = request.SendWebRequest();

            while (!operation.isDone)
                await Task.Yield();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"API Error: {request.error} - {request.downloadHandler?.text}");
                return null;
            }

            return request.downloadHandler.text;
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            return null;
        }
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Sets the base URL for all API requests
    /// </summary>
    public static void SetBaseUrl(string baseUrl)
    {
        if (!string.IsNullOrEmpty(baseUrl))
        {
            // Make sure the URL ends with a slash
            if (!baseUrl.EndsWith("/"))
                baseUrl += "/";

            BaseUrl = baseUrl;
        }
    }

    /// <summary>
    /// Downloads an image from a URL asynchronously
    /// </summary>
    public static async Task<Texture2D> DownloadImageAsync(string imageUrl)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl))
        {
            var operation = request.SendWebRequest();

            while (!operation.isDone)
                await Task.Yield();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Image download failed: {request.error}");
                return null;
            }

            return ((DownloadHandlerTexture)request.downloadHandler).texture;
        }
    }

    #endregion

    private class BypassCertificateHandler : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            return true; // Always trust (unsafe in production!)
        }
    }
}