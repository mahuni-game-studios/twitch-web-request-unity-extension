// © Copyright 2025 Mahuni Game Studios

namespace Mahuni.Twitch.Extension
{
    using System;
    using System.Collections;
    using UnityEngine;

    /// <summary>
    /// Perform web requests targeting the Twitch API
    /// </summary>
    public static class TwitchWebRequests
    {
        private static string broadcasterID;
        private static string channelName;
        private static bool isInitialized;

        #region Initialization
        
        /// <summary>
        /// Initialize the web request handler by passing the channel name
        /// </summary>
        /// <param name="broadcasterName">The channel name to use to get the broadcaster ID from</param>
        public static void Initialize(string broadcasterName)
        {
            channelName = broadcasterName;
            isInitialized = true;
        }

        /// <summary>
        /// Get if the web requests are ready to start making calls
        /// </summary>
        /// <returns>True if the web requests are ready to start making calls</returns>
        private static bool IsReady()
        {
            return isInitialized && !string.IsNullOrEmpty(broadcasterID);
        }

        /// <summary>
        /// Get ready to send web requests by logging to get the broadcaster ID, which is required for all requests.
        /// </summary>
        /// <returns>null</returns>
        private static IEnumerator GetReady()
        {
            if (!isInitialized)
            {
                Debug.LogError("Cannot setup TwitchWebRequests because TwitchWebRequests is not yet initialized.");
                yield break;
            }
            
            yield return TwitchRequest.Get("users?login=" + channelName, (code, message) =>
            {
                if (code != TwitchResponseCode.OK)
                {
                    Debug.LogError("Error for initial web request setup when trying to get the broadcaster ID: " + code);
                    broadcasterID = string.Empty;
                    return;
                }

                TwitchUsersResponse result = (TwitchUsersResponse)JsonUtility.FromJson(message, typeof(TwitchUsersResponse));
                broadcasterID = result.GetData().id;
            });

            if (string.IsNullOrEmpty(broadcasterID))
            {
                Debug.LogError("BroadcasterID is empty, cannot proceed!");
            }
        }
        
        #endregion

        #region Reward Requests

        /// <summary>
        /// Creates a Custom Reward in the broadcaster’s channel.
        /// <see href="https://dev.twitch.tv/docs/api/reference/#create-custom-rewards">Official documentation</see>
        /// </summary>
        /// <param name="title">The custom reward’s title. The title may contain a maximum of 45 characters, and  must be unique amongst the broadcaster’s custom rewards.</param>
        /// <param name="cost">The cost of the reward, in Channel Points. The minimum is 1 point.</param>
        /// <param name="callback">A callback to get notified of the result of the request</param>
        /// <returns>null</returns>
        public static IEnumerator CreateReward(string title, long cost, Action<TwitchResponseCode, string> callback)
        {
            if (!IsReady()) yield return GetReady();
            yield return TwitchRequest.Post("channel_points/custom_rewards?broadcaster_id=" + broadcasterID, $"{{\"title\":\"{title}\",\"cost\":{cost}}}", callback);
        }

        /// <summary>
        /// Deletes a custom reward that the broadcaster created.
        /// <see href="https://dev.twitch.tv/docs/api/reference/#delete-custom-reward">Official documentation</see> 
        /// </summary>
        /// <param name="rewardId">The ID of the custom reward to delete.</param>
        /// <param name="callback">A callback to get notified of the result of the request</param>
        /// <returns>null</returns>
        public static IEnumerator DeleteReward(string rewardId, Action<TwitchResponseCode> callback)
        {
            if (!IsReady()) yield return GetReady();
            yield return TwitchRequest.Delete($"channel_points/custom_rewards?broadcaster_id={broadcasterID}&id={rewardId}", callback);
        }
        
        #endregion
    }
}