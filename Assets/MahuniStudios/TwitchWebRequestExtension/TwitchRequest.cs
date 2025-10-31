// © Copyright 2025 Mahuni Game Studios

namespace Mahuni.Twitch.Extension
{
    using System;
    using System.Collections;
    using UnityEngine.Networking;
    using UnityEngine;

    /// <summary>
    /// A collection of the different web request methods needed to talk to the Twitch API
    /// </summary>
    public static class TwitchRequest
    {
        private const string URL = "https://api.twitch.tv/helix/";
        private const string HEADER_TOKEN = "Authorization";
        private const string HEADER_CLIENT_ID = "Client-Id";
        private const string HEADER_CLIENT_CONTENT_TYPE = "Content-Type";
        private const int REQUEST_TIMEOUT = 10;
        
        /// <summary>
        /// Communicate to the Twitch API using HTTP GET
        /// </summary>
        /// <param name="uri">The URI of the resource to retrieve via HTTP GET</param>
        /// <param name="callback">A callback to get notified of the result of the HTTP GET request</param>
        /// <returns>null</returns>
        public static IEnumerator Get(string uri, Action<TwitchResponseCode, string> callback)
        {
            Debug.Log($"TwitchRequest: Get '{uri}'...");
            using (UnityWebRequest webRequest = UnityWebRequest.Get(BuildURL(uri)))
            {
                webRequest.timeout = REQUEST_TIMEOUT;
                webRequest.SetRequestHeader(HEADER_CLIENT_ID, TwitchWebRequestAuthentication.Connection.twitchClientId);
                webRequest.SetRequestHeader(HEADER_TOKEN, GetToken());

                yield return webRequest.SendWebRequest();
                SendCallback(webRequest, callback);
            }
        }

        /// <summary>
        /// Communicate to the Twitch API using HTTP POST
        /// </summary>
        /// <param name="uri">The target URI to which the JSON format string will be transmitted</param>
        /// <param name="jsonString">Form body data which Twitch requires to be in JSON format string</param>
        /// <param name="callback">A callback to get notified of the result of the HTTP POST request</param>
        /// <returns>null</returns>
        public static IEnumerator Post(string uri, string jsonString, Action<TwitchResponseCode, string> callback)
        {
            Debug.Log($"TwitchRequest: Post '{uri}' with JSON content '{jsonString}'...");
            using (UnityWebRequest webRequest = UnityWebRequest.Post(BuildURL(uri), jsonString, "application/json"))
            {
                webRequest.timeout = REQUEST_TIMEOUT;
                webRequest.SetRequestHeader(HEADER_CLIENT_ID, TwitchWebRequestAuthentication.Connection.twitchClientId);
                webRequest.SetRequestHeader(HEADER_TOKEN, GetToken());
                webRequest.SetRequestHeader(HEADER_CLIENT_CONTENT_TYPE, "application/json");

                yield return webRequest.SendWebRequest();
                SendCallback(webRequest, callback);
            }
        }

        /// <summary>
        /// Communicate to the Twitch API using HTTP DELETE
        /// </summary>
        /// <param name="uri">The URI to which a DELETE request should be sent</param>
        /// <param name="callback">A callback to get notified of the result of the HTTP DELETE request</param>
        /// <returns>null</returns>
        public static IEnumerator Delete(string uri, Action<TwitchResponseCode> callback)
        {
            Debug.Log($"TwitchRequest: Delete '{uri}'...");
            using (UnityWebRequest webRequest = UnityWebRequest.Delete(BuildURL(uri)))
            {
                webRequest.timeout = REQUEST_TIMEOUT;
                webRequest.SetRequestHeader(HEADER_CLIENT_ID, TwitchWebRequestAuthentication.Connection.twitchClientId);
                webRequest.SetRequestHeader(HEADER_TOKEN, GetToken());

                yield return webRequest.SendWebRequest();
                SendCallback(webRequest, callback);
            }
        }
        
        /// <summary>
        /// Communicate to the Twitch API using HTTP PATCH, which is forwarded as PUT internally to work with UnityWebRequest.
        /// </summary>
        /// <param name="uri">The URI to which the data will be sent</param>
        /// <param name="content">The data to transmit to the remote server</param>
        /// <param name="callback"></param>
        /// <returns>null</returns>
        public static IEnumerator Patch(string uri, string content, Action<TwitchResponseCode, string> callback)
        {
            Debug.Log($"TwitchRequest: Patch '{uri}' with JSON content '{content}'...");
            using (UnityWebRequest webRequest = UnityWebRequest.Put(BuildURL(uri), content))
            {
                webRequest.method = "PATCH";
                webRequest.timeout = REQUEST_TIMEOUT;
                webRequest.SetRequestHeader(HEADER_CLIENT_ID, TwitchWebRequestAuthentication.Connection.twitchClientId);
                webRequest.SetRequestHeader(HEADER_TOKEN, GetToken());
                webRequest.SetRequestHeader(HEADER_CLIENT_CONTENT_TYPE, "application/json");

                yield return webRequest.SendWebRequest();
                SendCallback(webRequest, callback);
            }
        }

        #region Helpers
        
        private static void SendCallback(UnityWebRequest request, Action<TwitchResponseCode> callback)
        {
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"TwitchRequest ERROR from {request.method} method: '{request.error}', {request.result}, Response code: {(TwitchResponseCode)request.responseCode}");
            }
            callback.Invoke((TwitchResponseCode)request.responseCode);
        }

        private static void SendCallback(UnityWebRequest request, Action<TwitchResponseCode, string> callback)
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                callback.Invoke((TwitchResponseCode)request.responseCode, request.downloadHandler.text);
            }
            else
            {
                Debug.LogWarning($"TwitchRequest ERROR from {request.method} method: '{request.error}', {request.result}, Response code: {(TwitchResponseCode)request.responseCode}");
                callback.Invoke((TwitchResponseCode)request.responseCode, request.error);
            }
        }

        private static string BuildURL(string uri)
        {
            return URL + uri;
        }

        private static string GetToken()
        {
            return "Bearer " + TwitchLocalStorage.GetToken();
        }
        
        #endregion
    }
}