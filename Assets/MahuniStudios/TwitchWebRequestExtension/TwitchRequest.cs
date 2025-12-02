// © Copyright 2025 Mahuni Game Studios

namespace Mahuni.Twitch.Extension
{
    using System;
    using System.Collections;
    using UnityEngine.Networking;
    using UnityEngine;
    using System.Threading.Tasks;

    /// <summary>
    /// A collection of the different web request methods needed to talk to the Twitch API
    /// </summary>
    public static class TwitchRequest
    {
        public static bool LOG = true;
        private const string URL = "https://api.twitch.tv/helix/";
        private const string HEADER_TOKEN = "Authorization";
        private const string HEADER_CLIENT_ID = "Client-Id";
        private const string HEADER_CLIENT_CONTENT_TYPE = "Content-Type";
        private const int REQUEST_TIMEOUT = 10;

        #region Get
        
        /// <summary>
        /// Communicate to the Twitch API using HTTP GET synchronous
        /// </summary>
        /// <param name="uri">The URI of the resource to retrieve via HTTP GET</param>
        /// <param name="callback">A callback to get notified of the result of the HTTP GET request, containing the response code and the response body</param>
        /// <returns>null</returns>
        public static IEnumerator Get(string uri, Action<TwitchResponseCode, string> callback)
        {
            yield return SyncRequest(CreateGetRequest(uri), callback);
        }

        /// <summary>
        /// Communicate to the Twitch API using HTTP GET asynchronous
        /// </summary>
        /// <param name="uri">The URI of the resource to retrieve via HTTP GET</param>
        /// <returns>The result of the HTTP GET request, containing the response code and the response body</returns>
        public static async Task<(TwitchResponseCode responseCode, string responseBody)> AsyncGet(string uri)
        {
            return await AsyncRequest(CreateGetRequest(uri));
        }
        
        /// <summary>
        /// Communicate to the Twitch API using HTTP GET using awaitable to be asynchronous
        /// </summary>
        /// <param name="uri">The URI of the resource to retrieve via HTTP GET</param>
        /// <returns>The result of the HTTP GET request, containing the response code and the response body</returns>
        public static async Awaitable<(TwitchResponseCode responseCode, string responseBody)> AwaitableGet(string uri)
        {
            return await AwaitableRequest(CreateGetRequest(uri));
        }
        
        /// <summary>
        /// Create the Unity web request object for a GET request
        /// </summary>
        /// <param name="uri">The URI of the resource to retrieve via HTTP GET</param>
        /// <returns>The Unity web request object for a GET request</returns>
        private static UnityWebRequest CreateGetRequest(string uri)
        {
            if (LOG) Debug.Log($"TwitchRequest: Get '{uri}'...");
            UnityWebRequest webRequest = UnityWebRequest.Get(BuildURL(uri));
            webRequest.SetDefaultHeaders();
            return webRequest;
        }

        #endregion

        #region Post
        
        /// <summary>
        /// Communicate to the Twitch API using HTTP POST synchronous
        /// </summary>
        /// <param name="uri">The target URI to which the JSON format string will be transmitted</param>
        /// <param name="jsonContent">Form body data which Twitch requires to be in JSON format string</param>
        /// <param name="callback">A callback to get notified of the result of the HTTP POST request, containing the response code and the response body</param>
        /// <returns>null</returns>
        public static IEnumerator Post(string uri, string jsonContent, Action<TwitchResponseCode, string> callback)
        {
            yield return SyncRequest(CreatePostRequest(uri, jsonContent), callback);
        }
        
        /// <summary>
        /// Communicate to the Twitch API using HTTP POST asynchronous
        /// </summary>
        /// <param name="uri">The URI of the resource to retrieve via HTTP POST</param>
        /// <param name="jsonContent">Form body data which Twitch requires to be in JSON format string</param>
        /// <returns>The result of the HTTP POST request, containing the response code and the response body</returns>
        public static async Task<(TwitchResponseCode responseCode, string responseBody)> AsyncPost(string uri, string jsonContent)
        {
            return await AsyncRequest(CreatePostRequest(uri, jsonContent));
        }
        
        /// <summary>
        /// Communicate to the Twitch API using HTTP POST using awaitable to be asynchronous
        /// </summary>
        /// <param name="uri">The URI of the resource to retrieve via HTTP POST</param>
        /// <param name="jsonContent">Form body data which Twitch requires to be in JSON format string</param>
        /// <returns>The result of the HTTP POST request, containing the response code and the response body</returns>
        public static async Awaitable<(TwitchResponseCode responseCode, string responseBody)> AwaitablePost(string uri, string jsonContent)
        {
            return await AwaitableRequest(CreatePostRequest(uri, jsonContent));
        }
        
        /// <summary>
        /// Create the Unity web request object for a POST request
        /// </summary>
        /// <param name="uri">The URI of the resource to retrieve via HTTP POST</param>
        /// <param name="jsonContent">Form body data which Twitch requires to be in JSON format string</param>
        /// <returns>The Unity web request object for a POST request</returns>
        private static UnityWebRequest CreatePostRequest(string uri, string jsonContent)
        {
            if (LOG) Debug.Log($"TwitchRequest: Post '{uri}' with JSON content '{jsonContent}'...");
            UnityWebRequest webRequest = UnityWebRequest.Post(BuildURL(uri), jsonContent, "application/json");
            webRequest.SetDefaultHeaders();
            webRequest.SetRequestHeader(HEADER_CLIENT_CONTENT_TYPE, "application/json");
            return webRequest;
        }
        
        #endregion

        #region Put
        
        /// <summary>
        /// Communicate to the Twitch API using HTTP PUT synchronous
        /// </summary>
        /// <param name="uri">The target URI to which the JSON format string will be transmitted</param>
        /// <param name="jsonContent">Form body data which Twitch requires to be in JSON format string</param>
        /// <param name="callback">A callback to get notified of the result of the HTTP PUT request, containing the response code and the response body</param>
        /// <returns>null</returns>
        public static IEnumerator Put(string uri, string jsonContent, Action<TwitchResponseCode, string> callback)
        {
            yield return SyncRequest(CreatePutRequest(uri, jsonContent), callback);
        }
        
        /// <summary>
        /// Communicate to the Twitch API using HTTP PUT asynchronous
        /// </summary>
        /// <param name="uri">The URI of the resource to retrieve via HTTP PUT</param>
        /// <param name="jsonContent">Form body data which Twitch requires to be in JSON format string</param>
        /// <returns>The result of the HTTP PUT request, containing the response code and the response body</returns>
        public static async Task<(TwitchResponseCode responseCode, string responseBody)> AsyncPut(string uri, string jsonContent)
        {
            return await AsyncRequest(CreatePutRequest(uri, jsonContent));
        }
        
        /// <summary>
        /// Communicate to the Twitch API using HTTP PUT using awaitable to be asynchronous
        /// </summary>
        /// <param name="uri">The URI of the resource to retrieve via HTTP PUT</param>
        /// <param name="jsonContent">Form body data which Twitch requires to be in JSON format string</param>
        /// <returns>The result of the HTTP PUT request, containing the response code and the response body</returns>
        public static async Awaitable<(TwitchResponseCode responseCode, string responseBody)> AwaitablePut(string uri, string jsonContent)
        {
            return await AwaitableRequest(CreatePutRequest(uri, jsonContent));
        }

        /// <summary>
        /// Create the Unity web request object for a PUT request
        /// </summary>
        /// <param name="uri">The URI of the resource to retrieve via HTTP PUT</param>
        /// <param name="jsonContent">Form body data which Twitch requires to be in JSON format string</param>
        /// <returns>The Unity web request object for a PUT request</returns>
        private static UnityWebRequest CreatePutRequest(string uri, string jsonContent)
        {
            if (LOG) Debug.Log($"TwitchRequest: Put '{uri}'...");
            UnityWebRequest webRequest = UnityWebRequest.Put(BuildURL(uri), jsonContent);
            webRequest.SetDefaultHeaders();
            webRequest.SetRequestHeader(HEADER_CLIENT_CONTENT_TYPE, "application/json");
            return webRequest;
        }

        #endregion

        #region Delete

        /// <summary>
        /// Communicate to the Twitch API using HTTP DELETE synchronous 
        /// </summary>
        /// <param name="uri">The URI to which a DELETE request should be sent</param>
        /// <param name="callback">A callback to get notified of the result of the HTTP DELETE request, containing the response code</param>
        /// <returns>null</returns>
        public static IEnumerator Delete(string uri, Action<TwitchResponseCode> callback)
        {
            yield return SyncRequest(CreateDeleteRequest(uri), callback);
        }
        
        /// <summary>
        /// Communicate to the Twitch API using HTTP DELETE asynchronous
        /// </summary>
        /// <param name="uri">The URI of the resource to retrieve via HTTP DELETE</param>
        /// <returns>The result of the HTTP DELETE request, containing the response code and the response body</returns>
        public static async Task<(TwitchResponseCode responseCode, string responseBody)> AsyncDelete(string uri)
        {
            return await AsyncRequest(CreateDeleteRequest(uri));
        }
        
        /// <summary>
        /// Communicate to the Twitch API using HTTP DELETE using awaitable to be asynchronous
        /// </summary>
        /// <param name="uri">The URI of the resource to retrieve via HTTP DELETE</param>
        /// <returns>The result of the HTTP DELETE request, containing the response code and the response body</returns>
        public static async Awaitable<(TwitchResponseCode responseCode, string responseBody)> AwaitableDelete(string uri)
        {
            return await AwaitableRequest(CreateDeleteRequest(uri));
        }
        
        /// <summary>
        /// Create the Unity web request object for a DELETE request
        /// </summary>
        /// <param name="uri">The URI of the resource to retrieve via HTTP DELETE</param>
        /// <returns>The Unity web request object for a DELETE request</returns>
        private static UnityWebRequest CreateDeleteRequest(string uri)
        {
            if (LOG) Debug.Log($"TwitchRequest: Delete '{uri}'...");
            UnityWebRequest webRequest = UnityWebRequest.Delete(BuildURL(uri));
            webRequest.SetDefaultHeaders();
            return webRequest;
        }
        
        #endregion

        #region Patch
        
        /// <summary>
        /// Communicate to the Twitch API using HTTP PATCH synchronous (which is forwarded as PUT internally to work with UnityWebRequest)
        /// </summary>
        /// <param name="uri">The URI to which the data will be sent</param>
        /// <param name="jsonContent">The data to transmit to the remote server</param>
        /// <param name="callback">A callback to get notified of the result of the HTTP PATCH request, containing the response code and the response body</param>
        /// <returns>null</returns>
        public static IEnumerator Patch(string uri, string jsonContent, Action<TwitchResponseCode, string> callback)
        {
            yield return SyncRequest(CreatePatchRequest(uri, jsonContent), callback);
        }
        
        /// <summary>
        /// Communicate to the Twitch API using HTTP PATCH asynchronous (which is forwarded as PUT internally to work with UnityWebRequest)
        /// </summary>
        /// <param name="uri">The URI of the resource to retrieve via HTTP PATCH</param>
        /// <param name="jsonContent">Form body data which Twitch requires to be in JSON format string</param>
        /// <returns>The result of the HTTP PATCH request, containing the response code and the response body</returns>
        public static async Task<(TwitchResponseCode responseCode, string responseBody)> AsyncPatch(string uri, string jsonContent)
        {
            return await AsyncRequest(CreatePatchRequest(uri, jsonContent));
        }
        
        /// <summary>
        /// Communicate to the Twitch API using HTTP PATCH using awaitable to be asynchronous (which is forwarded as PUT internally to work with UnityWebRequest)
        /// </summary>
        /// <param name="uri">The URI of the resource to retrieve via HTTP PATCH</param>
        /// <param name="jsonContent">Form body data which Twitch requires to be in JSON format string</param>
        /// <returns>The result of the HTTP PATCH request, containing the response code and the response body</returns>
        public static async Awaitable<(TwitchResponseCode responseCode, string responseBody)> AwaitablePatch(string uri, string jsonContent)
        {
            return await AwaitableRequest(CreatePatchRequest(uri, jsonContent));
        }

        /// <summary>
        /// Create the Unity web request object for a PATCH request
        /// </summary>
        /// <param name="uri">The URI of the resource to retrieve via HTTP PATCH</param>
        /// <param name="jsonContent">Form body data which Twitch requires to be in JSON format string</param>
        /// <returns>The Unity web request object for a PATCH request</returns>
        private static UnityWebRequest CreatePatchRequest(string uri, string jsonContent)
        {
            if (LOG) Debug.Log($"TwitchRequest: Patch '{uri}' with JSON content '{jsonContent}'...");
            UnityWebRequest webRequest = UnityWebRequest.Put(BuildURL(uri), jsonContent);
            webRequest.SetDefaultHeaders();
            webRequest.method = "PATCH";
            webRequest.SetRequestHeader(HEADER_CLIENT_CONTENT_TYPE, "application/json");
            return webRequest;
        }

        #endregion

        #region Helpers

        private static IEnumerator SyncRequest(UnityWebRequest webRequest, Action<TwitchResponseCode> callback)
        {
            yield return webRequest.SendWebRequest();
            LogOnError(webRequest);
            callback.Invoke((TwitchResponseCode)webRequest.responseCode);
        }
        
        private static IEnumerator SyncRequest(UnityWebRequest webRequest, Action<TwitchResponseCode, string> callback)
        {
            yield return webRequest.SendWebRequest();
            LogOnError(webRequest);
            callback.Invoke((TwitchResponseCode)webRequest.responseCode, webRequest.result == UnityWebRequest.Result.Success ? webRequest.downloadHandler.text : webRequest.error);
        }
        
        private static async Task<(TwitchResponseCode responseCode, string responseBody)> AsyncRequest(UnityWebRequest webRequest)
        {
            using UnityWebRequest request = webRequest;
            await request.SendWebRequest();
            LogOnError(webRequest);
            return ((TwitchResponseCode)webRequest.responseCode, webRequest.result == UnityWebRequest.Result.Success ? webRequest.downloadHandler.text : webRequest.error);
        }
        
        private static async Awaitable<(TwitchResponseCode responseCode, string responseBody)> AwaitableRequest(UnityWebRequest webRequest)
        {
            using UnityWebRequest request = webRequest;
            await request.SendWebRequest();
            LogOnError(webRequest);
            return ((TwitchResponseCode)webRequest.responseCode, webRequest.result == UnityWebRequest.Result.Success ? webRequest.downloadHandler.text : webRequest.error);
        }

        private static string BuildURL(string uri)
        {
            return URL + uri;
        }

        private static void SetDefaultHeaders(this UnityWebRequest webRequest)
        {
            webRequest.timeout = REQUEST_TIMEOUT;
            webRequest.SetRequestHeader(HEADER_CLIENT_ID, TwitchWebRequestAuthentication.Connection.twitchClientId);
            webRequest.SetRequestHeader(HEADER_TOKEN, GetToken());
        }

        private static string GetToken()
        {
            return "Bearer " + TwitchLocalStorage.GetToken();
        }

        private static void LogOnError(UnityWebRequest request)
        {
            if (request.result == UnityWebRequest.Result.Success) return;
            if (LOG) Debug.LogError($"TwitchRequest: ERROR from {request.method} method: '{request.error}', {request.result}, Response code: {(TwitchResponseCode)request.responseCode}");
        }
        
        #endregion
    }
}