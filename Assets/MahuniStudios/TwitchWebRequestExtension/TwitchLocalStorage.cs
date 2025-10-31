// © Copyright 2025 Mahuni Game Studios

namespace Mahuni.Twitch.Extension
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Store the twitch authentication access token to your local machine
    /// </summary>
    public static class TwitchLocalStorage
    {
        private static readonly string TwitchOAuthTokenKey = Application.productName + "__Auth__OAuthToken";

        /// <summary>
        /// Get if there is a twitch authentication access twitch authentication access token stored locally
        /// </summary>
        /// <returns>True if there is a twitch authentication access token stored locally</returns>
        public static bool HasToken()
        {
            return PlayerPrefs.HasKey(TwitchOAuthTokenKey);
        }

        /// <summary>
        /// Get the twitch authentication access token from the local storage.
        /// </summary>
        /// <returns>The twitch authentication access token as string. If there is no token, the string will return empty</returns>
        public static string GetToken()
        {
            if (!HasToken())
            {
                Debug.LogWarning("Trying to get a twitch authentication access token from the local storage, but there is none stored.");
                return string.Empty;
            }

            TwitchWebRequestAuthentication.OAuth auth;
            try
            {
                auth = (TwitchWebRequestAuthentication.OAuth)JsonUtility.FromJson(PlayerPrefs.GetString(TwitchOAuthTokenKey), typeof(TwitchWebRequestAuthentication.OAuth));
            }
            catch (Exception e)
            {
                Debug.LogError("Could not deserialize twitch authentication access token from player preferences: " + e);
                return string.Empty;
            }

            return auth.accessToken;
        }

        /// <summary>
        /// Store the twitch authentication access token on your local machine
        /// </summary>
        /// <param name="oAuth">The authentication object to save locally</param>
        public static void SetToken(TwitchWebRequestAuthentication.OAuth oAuth)
        {
            PlayerPrefs.SetString(TwitchOAuthTokenKey, JsonUtility.ToJson(oAuth));
        }

        /// <summary>
        /// Clear the twitch authentication access token from your local storage
        /// </summary>
        public static void ClearToken()
        {
            PlayerPrefs.DeleteKey(TwitchOAuthTokenKey);
        }
    }
}