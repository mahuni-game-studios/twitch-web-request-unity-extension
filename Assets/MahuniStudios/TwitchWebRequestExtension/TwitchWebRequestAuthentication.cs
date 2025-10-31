// © Copyright 2025 Mahuni Game Studios

namespace Mahuni.Twitch.Extension
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Threading;
    using UnityEngine;

    /// <summary>
    /// Authenticate to gain a token and start using web requests to the Twitch API
    /// </summary>
    public static class TwitchWebRequestAuthentication
    {
        public static event Action<bool> OnAuthenticated;
        public static ConnectionInformation Connection { get; private set; }
        private static AuthenticationStatus authenticationStatus;
        private const float AUTHENTICATION_TIMEOUT = 10.0f;
        private static HttpListener httpListener;
        private static Thread httpListenerThread;
        private static OAuth oAuth;

        #region Authentication

        /// <summary>
        /// Get if the application is authenticated and Twitch SDK methods can thereby be used
        /// </summary>
        /// <returns>True if the application is authenticated, false if authentication is not completed</returns>
        public static bool IsAuthenticated()
        {
            return authenticationStatus == AuthenticationStatus.Authenticated;
        }

        /// <summary>
        /// Reset the authentication status by clearing the token from the local storage
        /// </summary>
        public static void Reset()
        {
            TwitchLocalStorage.ClearToken();
            authenticationStatus = AuthenticationStatus.Unknown;
        }

        /// <summary>
        /// Start the authentication validation routine and thread, if there isn't already a token in the local storage
        /// </summary>
        /// <param name="monoBehaviour">The mono behaviour to run the routine on</param>
        /// <param name="connectionInformation">The connection information used to authenticate</param>
        public static void StartAuthenticationValidation(MonoBehaviour monoBehaviour, ConnectionInformation connectionInformation)
        {
            Connection = connectionInformation;
            SetAuthenticationStatusFromStorage();

            if (authenticationStatus == AuthenticationStatus.Authenticated)
            {
                OnAuthenticated?.Invoke(true);
            }
            else
            {
                monoBehaviour.StartCoroutine(UpdateAuthenticationStatus(Connection));
            }
        }

        /// <summary>
        /// Routine waiting for the thread to get information back from the browser content
        /// </summary>
        /// <param name="connectionInformation">The connection information used to authenticate</param>
        /// <returns>null</returns>
        private static IEnumerator UpdateAuthenticationStatus(ConnectionInformation connectionInformation)
        {
            httpListener = new HttpListener();
            httpListener.Prefixes.Add("http://localhost/");
            httpListener.Prefixes.Add("http://127.0.0.1/");
            httpListener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
            httpListener.Start();

            httpListenerThread = new Thread(HttpListenerThread);
            httpListenerThread.Start();

            // You can also use another browser than Microsoft Edge, just be sure to NOT use Firefox, as it does not work
            Application.OpenURL("microsoft-edge:" + GetAuthenticationURL(connectionInformation));

            float processStartTime = Time.realtimeSinceStartup;
            while (authenticationStatus != AuthenticationStatus.Authenticated)
            {
                float elapsedTime = Time.realtimeSinceStartup - processStartTime;
                if (elapsedTime >= AUTHENTICATION_TIMEOUT)
                {
                    OnAuthenticated?.Invoke(false);
                    StopHttpListenerThread();
                    Debug.LogError("Authentication attempt timed out!");
                    yield break;
                }

                yield return null;
            }

            TwitchLocalStorage.SetToken(oAuth);
            OnAuthenticated?.Invoke(true);
            StopHttpListenerThread();
        }

        /// <summary>
        /// Thread waiting for the browser URL to return authentication information
        /// </summary>
        private static void HttpListenerThread()
        {
            authenticationStatus = AuthenticationStatus.Waiting;
            while (httpListenerThread.IsAlive)
            {
                if (authenticationStatus == AuthenticationStatus.Authenticated) return;
                IAsyncResult result = httpListener?.BeginGetContext(GetHttpContextCallback, httpListener);
                result?.AsyncWaitHandle.WaitOne();
            }
        }

        /// <summary>
        /// Stop the http listener thread
        /// </summary>
        private static void StopHttpListenerThread()
        {
            if (httpListener != null && httpListener.IsListening)
            {
                httpListener.Stop();
                httpListener.Close();
            }

            if (httpListenerThread != null && httpListenerThread.IsAlive)
            {
                httpListenerThread.Abort();
            }
        }

        /// <summary>
        /// Read out the http context and look for the authentication token
        /// </summary>
        /// <param name="asyncResult">The async result</param>
        private static void GetHttpContextCallback(IAsyncResult asyncResult)
        {
            HttpListenerContext context = httpListener.EndGetContext(asyncResult);
            HttpListenerRequest request = context.Request;

            if (request.HttpMethod == "POST")
            {
                string dataText = new StreamReader(request.InputStream, request.ContentEncoding).ReadToEnd();
                oAuth = JsonUtility.FromJson<OAuth>(dataText);
                authenticationStatus = AuthenticationStatus.Authenticated;
            }

            httpListener.BeginGetContext(GetHttpContextCallback, null);

            byte[] buffer = GetResponseBuffer();
            HttpListenerResponse response = context.Response;
            response.ContentType = "text/html";
            response.ContentLength64 = buffer.Length;
            response.StatusCode = 200;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.OutputStream.Close();
        }

        #endregion

        #region Helpers
        
        /// <summary>
        /// Set the authentication status from the local storage
        /// </summary>
        private static void SetAuthenticationStatusFromStorage()
        {
            if (!TwitchLocalStorage.HasToken() || string.IsNullOrEmpty(TwitchLocalStorage.GetToken()))
            {
                Debug.LogWarning("No valid access token found in local storage.");
                authenticationStatus = AuthenticationStatus.Unknown;
                return;
            }

            authenticationStatus = AuthenticationStatus.Authenticated;
        }

        /// <summary>
        /// Get the URL string to authenticate to the Twitch API
        /// </summary>
        /// <param name="connectionInformation">The connection information used to authenticate</param>
        /// <returns>The URL string to authenticate to the Twitch API</returns>
        private static string GetAuthenticationURL(ConnectionInformation connectionInformation)
        {
            return $"https://id.twitch.tv/oauth2/authorize?client_id={connectionInformation.twitchClientId}&redirect_uri={connectionInformation.redirectUrl}&response_type=token&scope={connectionInformation.permissionScope}";
        }

        /// <summary>
        /// Get the response functionality buffer as byte array
        /// </summary>
        /// <returns>The response buffer as byte array</returns>
        private static byte[] GetResponseBuffer()
        {
            const string content = @"
                <html><head>
                <script src=""https://unpkg.com/axios/dist/axios.min.js""></script>
                <script>if (window.location.hash) 
                    {
                        let fragments = window.location.hash.substring(1).split('&').map(x => x.split('=')[1]);
                        let data = { accessToken: fragments[0], scope: fragments[1], state: fragments[2] };
                        axios.post('/', data).then(function(response) 
                            {
                                console.log(response);
                                window.close();
                            }).catch(function(error)
                            {
                                console.log(error);
                                window.close();
                            });
                    }
                </script></head>";
            return System.Text.Encoding.UTF8.GetBytes(content);
        }

        private enum AuthenticationStatus
        {
            Unknown,
            Waiting,
            Authenticated
        }
        
        [Serializable]
        public struct OAuth
        {
            public string accessToken;
            public string scope;
            public string state;
        }

        [Serializable]
        public struct ConnectionInformation
        {
            public string twitchClientId;
            public string redirectUrl;
            public string permissionScope;

            // Find more examples on permissions here: https://dev.twitch.tv/docs/authentication/scopes/
            public const string CHAT_READ = "chat:read";
            public const string CHAT_EDIT = "chat:edit";
            public const string USER_READ_SUBSCRIPTIONS = "user:read:subscriptions";
            public const string CHANNEL_MANAGE_REDEMPTIONS = "channel:manage:redemptions";
            public const string CHANNEL_MANAGE_POLLS = "channel:manage:polls";

            public ConnectionInformation(string twitchClientId, List<string> scopeList)
            {
                this.twitchClientId = twitchClientId;
                redirectUrl = "http://localhost";
                permissionScope = string.Join(" ", scopeList);
            }

            public ConnectionInformation(string twitchClientId, List<string> scopeList, string redirectUrl)
            {
                this.twitchClientId = twitchClientId;
                this.redirectUrl = redirectUrl;
                permissionScope = string.Join(" ", scopeList);
            }
        }

        #endregion
    }
}