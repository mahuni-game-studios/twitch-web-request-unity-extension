// © Copyright 2025 Mahuni Game Studios

namespace Mahuni.Twitch.Extension
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Perform web requests targeting the Twitch API
    /// </summary>
    public class TwitchWebRequestHandler
    {
        public string BroadcasterID { get; private set; }

        /// <summary>
        /// Constructor which initializes instantly
        /// </summary>
        /// <param name="channelName">The Twitch channel name to connect to</param>
        public TwitchWebRequestHandler(string channelName)
        {
            if (string.IsNullOrEmpty(channelName))
            {
                throw new ArgumentException("You need to specify a valid Twitch channel name to proceed.");
            }
        }

        /// <summary>
        /// Initialize by logging into the channel
        /// </summary>
        /// <param name="channelName">The channel name to log in to</param>
        public async Awaitable<bool> Connect(string channelName)
        {
            (TwitchResponseCode responseCode, string responseBody) response = await GetUser(channelName);
            
            if (response.responseCode != TwitchResponseCode.OK)
            {
                Debug.LogError("TwitchWebRequest: Error for initial web request setup when trying to get the broadcaster ID: " + response.responseCode);
                BroadcasterID = string.Empty;
                return false;
            }

            User user = JsonUtility.FromJson<Data<User>>(response.responseBody).GetFirst();
            BroadcasterID = user.id;
            return true;
        }

        #region Users

        /// <summary>
        /// Gets information about the user with the passed name
        /// <see href="https://dev.twitch.tv/docs/api/reference/#get-users">Official documentation</see>
        /// </summary>
        /// <param name="channelName">The channel name to get the user information from</param>
        /// <returns>Awaitable response code and response body from requesting to get the user with passed name</returns>
        public async Awaitable<(TwitchResponseCode responseCode, string responseBody)> GetUser(string channelName)
        {
            return await TwitchRequest.AwaitableGet($"users?login={channelName}");
        }

        #endregion

        #region Reward Requests
        
        /// <summary>
        /// Gets a list of custom rewards that the specified broadcaster created.
        /// <see href="https://dev.twitch.tv/docs/api/reference/#get-custom-reward">Official documentation</see>
        /// </summary>
        /// <returns>Awaitable response code and response body from requesting to get the users rewards</returns>
        public async Awaitable<(TwitchResponseCode responseCode, string responseBody)> GetRewards()
        {
            return await TwitchRequest.AwaitableGet($"channel_points/custom_rewards?broadcaster_id={BroadcasterID}");
        }
        
        /// <summary>
        /// Creates a Custom Reward in the broadcaster’s channel.
        /// <see href="https://dev.twitch.tv/docs/api/reference/#create-custom-rewards">Official documentation</see>
        /// </summary>
        /// <param name="title">The custom reward’s title. The title may contain a maximum of 45 characters, and  must be unique amongst the broadcaster’s custom rewards.</param>
        /// <param name="cost">The cost of the reward, in Channel Points. The minimum is 1 point.</param>
        /// <returns>Awaitable response code and response body from requesting to create a reward</returns>
        public async Awaitable<(TwitchResponseCode responseCode, string responseBody)> CreateReward(string title, long cost)
        {
            return await TwitchRequest.AwaitablePost("channel_points/custom_rewards?broadcaster_id=" + BroadcasterID, $"{{\"title\":\"{title}\",\"cost\":{cost}}}");
        }
        
        /// <summary>
        /// Deletes a custom reward that the broadcaster created.
        /// <see href="https://dev.twitch.tv/docs/api/reference/#delete-custom-reward">Official documentation</see> 
        /// </summary>
        /// <param name="rewardId">The ID of the custom reward to delete.</param>
        /// <returns>Awaitable response code from requesting to delete a reward</returns>
        public async Awaitable<TwitchResponseCode> DeleteReward(string rewardId)
        {
            return await TwitchRequest.AwaitableDelete($"channel_points/custom_rewards?broadcaster_id={BroadcasterID}&id={rewardId}");
        }
        
        #endregion
    }
}