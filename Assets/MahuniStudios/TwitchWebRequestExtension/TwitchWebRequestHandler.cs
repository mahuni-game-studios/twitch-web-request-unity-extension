// © Copyright 2025 Mahuni Game Studios

namespace Mahuni.Twitch.Extension
{
    using System;
    using UnityEngine;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json.Linq;

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

        public void Disconnect()
        {
            BroadcasterID = string.Empty;
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
        /// <param name="rewardTitle">The custom reward’s title. The title may contain a maximum of 45 characters, and  must be unique amongst the broadcaster’s custom rewards.</param>
        /// <param name="rewardCost">The cost of the reward, in Channel Points. The minimum is 1 point.</param>
        /// <param name="isUserInputRequired">A Boolean value that determines whether the user needs to enter information when redeeming the reward.</param>
        /// <param name="redeemPrompt">The prompt shown to the viewer when they redeem the reward. Specify a prompt if is_user_input_required is true. The prompt is limited to a maximum of 200 characters.</param>
        /// <param name="isCooldownEnabled">A Boolean value that determines whether to apply a cooldown period between redemptions.</param>
        /// <param name="cooldownSeconds">The cooldown period, in seconds. Applied only if the is_global_cooldown_enabled field is true. The minimum value is 1; however, the minimum value is 60 for it to be shown in the Twitch UI.</param>
        /// <returns>Awaitable response code and response body from requesting to create a reward</returns>
        public async Awaitable<(TwitchResponseCode responseCode, string responseBody)> CreateReward(string rewardTitle, long rewardCost, bool isUserInputRequired = false, string redeemPrompt = "",
            bool isCooldownEnabled = false, int cooldownSeconds = 1)
        {
            JObject jsonObject = JObject.FromObject(new
            {
                title = rewardTitle,
                cost = rewardCost,
                is_user_input_required = isUserInputRequired,
                prompt = redeemPrompt,
                is_global_cooldown_enabled = isCooldownEnabled,
                global_cooldown_seconds = cooldownSeconds,
                should_redemptions_skip_request_queue = true // By default, we want the request queue to be fulfilled straight away without additional steps
            });
            return await TwitchRequest.AwaitablePost("channel_points/custom_rewards?broadcaster_id=" + BroadcasterID, jsonObject.ToString());
        }
        
        /// <summary>
        /// Updates a custom reward that the specified broadcaster created.
        /// <see href="https://dev.twitch.tv/docs/api/reference/#update-custom-reward">Official documentation</see>
        /// <param name="reward">The reward to update</param>
        /// </summary>
        /// <returns>Awaitable response code and response body from requesting to get the users rewards</returns>
        public async Awaitable<(TwitchResponseCode responseCode, string responseBody)> UpdateReward(Reward reward)
        {
            JObject jsonObject = JObject.FromObject(new
            {
                title = reward.title,
                cost = reward.cost,
                is_user_input_required = reward.is_user_input_required,
                prompt = reward.prompt,
                is_global_cooldown_enabled = reward.global_cooldown_setting.is_enabled,
                global_cooldown_seconds = reward.global_cooldown_setting.global_cooldown_seconds
            });
            return await TwitchRequest.AwaitablePatch($"channel_points/custom_rewards?broadcaster_id={BroadcasterID}&id={reward.id}", jsonObject.ToString());
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

        #region Poll Requests

        /// <summary>
        /// Get the polls
        /// https://dev.twitch.tv/docs/api/reference/#get-polls
        /// </summary>
        /// <returns>Awaitable response code and body from requesting the polls list</returns>
        public async Awaitable<(TwitchResponseCode responseCode, string responseBody)> GetPolls()
        {
            return await TwitchRequest.AwaitableGet($"polls?broadcaster_id={BroadcasterID}");
        }

        /// <summary>
        /// Creates a poll
        /// https://dev.twitch.tv/docs/api/reference/#create-poll
        /// </summary>
        /// <param name="pollTitle">The title of the poll</param>
        /// <param name="choices">An array of poll options</param>
        /// <param name="durationSeconds">The duration of the poll in seconds. Min = 30s, Max = 1800s</param>
        /// <param name="enableChannelPointVoting">True to enable users spending channel points per vote</param>
        /// <param name="channelPoints">The amount of channel points per additional vote (only works if boolean enableChannelPointVoting is set to true)</param>
        /// <returns>>Awaitable response code and body from requesting to create a new poll</returns>
        public async Awaitable<(TwitchResponseCode responseCode, string responseBody)> CreatePoll(string pollTitle, string[] choices, int durationSeconds = 15, bool enableChannelPointVoting = false, int channelPoints = 1)
        {
            IEnumerable<JObject> pollChoices = choices.Select(outcomeTitle => JObject.FromObject(new { title = outcomeTitle }));
            JObject jsonObject = JObject.FromObject(new
            {
                broadcaster_id = BroadcasterID,
                title = pollTitle,
                choices = pollChoices,
                duration = durationSeconds,
                channel_points_voting_enabled = enableChannelPointVoting,
                channel_points_per_vote =  channelPoints
            });
            
            return await TwitchRequest.AwaitablePost("polls", jsonObject.ToString());
        }
       
        /// <summary>
        /// End a still active poll
        /// https://dev.twitch.tv/docs/api/reference/#end-poll
        /// </summary>
        /// <param name="pollId">The ID of the poll</param>
        /// <param name="pollStatus">The status to set the poll to (either TERMINATED or ARCHIVED)</param>
        /// <returns>>Awaitable response code and body from requesting to end a poll</returns>
        public async Awaitable<(TwitchResponseCode responseCode, string responseBody)> EndPoll(string pollId, Poll.Status pollStatus = Poll.Status.TERMINATED)
        {
            JObject jsonObject = JObject.FromObject(new
            {
                broadcaster_id = BroadcasterID,
                id = pollId,
                status = pollStatus.ToString()
            });
            
            return await TwitchRequest.AwaitablePatch("polls", jsonObject.ToString());
        }

        #endregion

        #region Prediction Requests
        
        /// <summary>
        /// Get the predictions
        /// https://dev.twitch.tv/docs/api/reference/#get-predictions
        /// </summary>
        /// <returns>Awaitable response code and body from requesting the prediction list</returns>
        public async Awaitable<(TwitchResponseCode responseCode, string responseBody)> GetPredictions()
        {
            return await TwitchRequest.AwaitableGet($"predictions?broadcaster_id={BroadcasterID}");
        }

        /// <summary>
        /// Creates a prediction
        /// https://dev.twitch.tv/docs/api/reference/#create-prediction
        /// </summary>
        /// <param name="predictionTitle">The title of the prediction</param>
        /// <param name="outcomeTitles">All possible outcomes</param>
        /// <param name="durationSeconds">The duration of the prediction until it becomes locked. The minimum is 30 seconds and the maximum is 1800 seconds (30 minutes)</param>
        /// <returns>Awaitable response code and body from requesting to create a prediction</returns>
        public async Awaitable<(TwitchResponseCode responseCode, string responseBody)> CreatePrediction(string predictionTitle, string[] outcomeTitles, int durationSeconds = 30)
        {
            IEnumerable<JObject> predictionOutcomes = outcomeTitles.Select(outcomeTitle => JObject.FromObject(new { title = outcomeTitle }));
            JObject jsonObject = JObject.FromObject(new
            {
                broadcaster_id = BroadcasterID,
                title = predictionTitle,
                outcomes = predictionOutcomes,
                prediction_window = durationSeconds
            });
            
            return await TwitchRequest.AwaitablePost("predictions", jsonObject.ToString());
        }
        
        /// <summary>
        /// Resolve a locked prediction
        /// https://dev.twitch.tv/docs/api/reference/#end-prediction
        /// </summary>
        /// <param name="predictionId">The ID of the prediction to solve</param>
        /// <param name="winningOutcomeId">The ID of the predictions winning outcome. Is optional when canceling</param>
        /// <param name="status">The status to set the prediction to</param>
        /// <returns>Awaitable response code and body from requesting to resolve a prediction</returns>
        public async Awaitable<(TwitchResponseCode responseCode, string responseBody)> ResolvePrediction(string predictionId, string winningOutcomeId, Prediction.Status status = Prediction.Status.RESOLVED)
        {
            JObject jsonObject = JObject.FromObject(new
            {
                broadcaster_id = BroadcasterID,
                id = predictionId,
                status = status.ToString(),
                winning_outcome_id = winningOutcomeId
            });
            
            return await TwitchRequest.AwaitablePatch("predictions", jsonObject.ToString());
        }

        #endregion
    }
}