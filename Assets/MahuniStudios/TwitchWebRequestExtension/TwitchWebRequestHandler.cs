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

        #region Twitch Connection

        /// <summary>
        /// Initialize by logging into the channel
        /// </summary>
        /// <param name="channelName">The channel name to log in to</param>
        public async Awaitable<bool> Connect(string channelName)
        {
            (TwitchResponseCode responseCode, User user) response = await GetUser(channelName);
            
            if (response.responseCode != TwitchResponseCode.OK)
            {
                Debug.LogError("TwitchWebRequest: Error for initial web request setup when trying to get the broadcaster ID: " + response.responseCode);
                BroadcasterID = string.Empty;
                return false;
            }

            BroadcasterID = response.user.id;
            return true;
        }

        public void Disconnect()
        {
            BroadcasterID = string.Empty;
        }
        
        #endregion

        #region Users

        /// <summary>
        /// Gets information about the user with the passed name
        /// <see href="https://dev.twitch.tv/docs/api/reference/#get-users">Official documentation</see>
        /// </summary>
        /// <param name="channelName">The channel name to get the user information from</param>
        /// <returns>Awaitable response code and response body from requesting to get the user with passed name</returns>
        public async Awaitable<(TwitchResponseCode responseCode, User user)> GetUser(string channelName)
        {
            (TwitchResponseCode responseCode, string responseBody) response = await TwitchRequest.AwaitableGet($"users?login={channelName}");
            bool success = response.responseCode == TwitchResponseCode.OK;
            return (response.responseCode, success ? JsonUtility.FromJson<Data<User>>(response.responseBody).GetFirst() : null);
        }

        #endregion

        #region Reward Requests
        
        /// <summary>
        /// Gets a list of custom rewards that the specified broadcaster created.
        /// <see href="https://dev.twitch.tv/docs/api/reference/#get-custom-reward">Official documentation</see>
        /// </summary>
        /// <returns>Awaitable response code and response body from requesting to get the users rewards</returns>
        public async Awaitable<(TwitchResponseCode responseCode, Dictionary<string, Reward> rewards)> GetRewards()
        {
            (TwitchResponseCode responseCode, string responseBody) response = await TwitchRequest.AwaitableGet($"channel_points/custom_rewards?broadcaster_id={BroadcasterID}");

            bool success = response.responseCode == TwitchResponseCode.OK;
            Dictionary<string, Reward> rewards = new();
            if (success)
            {
                rewards = JsonUtility.FromJson<Data<Reward>>(response.responseBody).data.ToDictionary(reward => reward.id);
            }

            return (response.responseCode, success ? rewards : null);
        }

        /// <summary>
        /// Creates a Custom Reward in the broadcaster’s channel.
        /// <see href="https://dev.twitch.tv/docs/api/reference/#create-custom-rewards">Official documentation</see>
        /// </summary>
        /// <param name="rewardTitle">The custom reward’s title. The title may contain a maximum of 45 characters, and  must be unique amongst the broadcaster’s custom rewards.</param>
        /// <param name="rewardCost">The cost of the reward, in Channel Points. The minimum is 1 point.</param>
        /// <param name="automaticRedemption">If redemptions should be set to FULFILLED status immediately when a reward is redeemed. If false, status is set to UNFULFILLED and follows the normal request queue process</param>
        /// <param name="isUserInputRequired">A Boolean value that determines whether the user needs to enter information when redeeming the reward.</param>
        /// <param name="redeemPrompt">The prompt shown to the viewer when they redeem the reward. Specify a prompt if is_user_input_required is true. The prompt is limited to a maximum of 200 characters.</param>
        /// <param name="isCooldownEnabled">A Boolean value that determines whether to apply a cooldown period between redemptions.</param>
        /// <param name="cooldownSeconds">The cooldown period, in seconds. Applied only if the is_global_cooldown_enabled field is true. The minimum value is 1; however, the minimum value is 60 for it to be shown in the Twitch UI.</param>
        /// <param name="customColorHex">The background color to use for the reward. Specify the color using Hex format (for example, #9147FF).</param>
        /// <returns>Awaitable response code and response body from requesting to create a reward</returns>
        public async Awaitable<(TwitchResponseCode responseCode, Reward reward)> CreateReward(string rewardTitle, long rewardCost, bool automaticRedemption = true, bool isUserInputRequired = false, string redeemPrompt = "",
            bool isCooldownEnabled = false, int cooldownSeconds = 1, string customColorHex = null)
        {
            JObject jsonObject = JObject.FromObject(new
            {
                title = rewardTitle,
                cost = rewardCost,
                is_enabled = true, // By default, we want the request to be enabled
                is_user_input_required = isUserInputRequired,
                prompt = redeemPrompt,
                is_global_cooldown_enabled = isCooldownEnabled,
                global_cooldown_seconds = cooldownSeconds,
                background_color = customColorHex,
                should_redemptions_skip_request_queue = automaticRedemption
            });
            
            (TwitchResponseCode responseCode, string responseBody) response = await TwitchRequest.AwaitablePost("channel_points/custom_rewards?broadcaster_id=" + BroadcasterID, jsonObject.ToString());
            
            bool success = response.responseCode == TwitchResponseCode.OK;
            return (response.responseCode, success ? JsonUtility.FromJson<Data<Reward>>(response.responseBody).GetFirst() : null);
        }
        
        /// <summary>
        /// Updates a custom reward that the specified broadcaster created.
        /// <see href="https://dev.twitch.tv/docs/api/reference/#update-custom-reward">Official documentation</see>
        /// <param name="reward">The reward to update</param>
        /// </summary>
        /// <returns>Awaitable response code and response body from requesting to get the users rewards</returns>
        public async Awaitable<(TwitchResponseCode responseCode, Reward reward)> UpdateReward(Reward reward)
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
            
            (TwitchResponseCode responseCode, string responseBody) response = await TwitchRequest.AwaitablePatch($"channel_points/custom_rewards?broadcaster_id={BroadcasterID}&id={reward.id}", jsonObject.ToString());
            
            bool success = response.responseCode == TwitchResponseCode.OK;
            return (response.responseCode, success ? JsonUtility.FromJson<Data<Reward>>(response.responseBody).GetFirst() : null);
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
        public async Awaitable<(TwitchResponseCode responseCode, List<Poll> polls)> GetPolls()
        {
            (TwitchResponseCode responseCode, string responseBody) response = await TwitchRequest.AwaitableGet($"polls?broadcaster_id={BroadcasterID}");
            
            bool success = response.responseCode == TwitchResponseCode.OK;
            return (response.responseCode, success ? JsonUtility.FromJson<Data<Poll>>(response.responseBody).data.ToList() : null);
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
        public async Awaitable<(TwitchResponseCode responseCode, Poll poll)> CreatePoll(string pollTitle, string[] choices, int durationSeconds = 15, bool enableChannelPointVoting = false, int channelPoints = 1)
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
            
            (TwitchResponseCode responseCode, string responseBody) response = await TwitchRequest.AwaitablePost("polls", jsonObject.ToString());
            
            bool success = response.responseCode == TwitchResponseCode.OK;
            return (response.responseCode, success ? JsonUtility.FromJson<Data<Poll>>(response.responseBody).GetFirst() : null);
        }
       
        /// <summary>
        /// End a still active poll
        /// https://dev.twitch.tv/docs/api/reference/#end-poll
        /// </summary>
        /// <param name="pollId">The ID of the poll</param>
        /// <param name="pollStatus">The status to set the poll to (either TERMINATED or ARCHIVED)</param>
        /// <returns>>Awaitable response code and body from requesting to end a poll</returns>
        public async Awaitable<(TwitchResponseCode responseCode, Poll poll)> EndPoll(string pollId, Poll.Status pollStatus = Poll.Status.TERMINATED)
        {
            JObject jsonObject = JObject.FromObject(new
            {
                broadcaster_id = BroadcasterID,
                id = pollId,
                status = pollStatus.ToString()
            });
            
            (TwitchResponseCode responseCode, string responseBody) response = await TwitchRequest.AwaitablePatch("polls", jsonObject.ToString());
            
            bool success = response.responseCode == TwitchResponseCode.OK;
            return (response.responseCode, success ? JsonUtility.FromJson<Data<Poll>>(response.responseBody).GetFirst() : null);
        }

        #endregion

        #region Prediction Requests
        
        /// <summary>
        /// Get the predictions
        /// https://dev.twitch.tv/docs/api/reference/#get-predictions
        /// </summary>
        /// <returns>Awaitable response code and body from requesting the prediction list</returns>
        public async Awaitable<(TwitchResponseCode responseCode, List<Prediction> prediction)> GetPredictions()
        {
            (TwitchResponseCode responseCode, string responseBody) response = await TwitchRequest.AwaitableGet($"predictions?broadcaster_id={BroadcasterID}");
            
            bool success = response.responseCode == TwitchResponseCode.OK;
            return (response.responseCode, success ? JsonUtility.FromJson<Data<Prediction>>(response.responseBody).data.ToList() : null);
        }

        /// <summary>
        /// Creates a prediction
        /// https://dev.twitch.tv/docs/api/reference/#create-prediction
        /// </summary>
        /// <param name="predictionTitle">The title of the prediction</param>
        /// <param name="outcomeTitles">All possible outcomes</param>
        /// <param name="durationSeconds">The duration of the prediction until it becomes locked. The minimum is 30 seconds and the maximum is 1800 seconds (30 minutes)</param>
        /// <returns>Awaitable response code and body from requesting to create a prediction</returns>
        public async Awaitable<(TwitchResponseCode responseCode, Prediction prediction)> CreatePrediction(string predictionTitle, string[] outcomeTitles, int durationSeconds = 30)
        {
            IEnumerable<JObject> predictionOutcomes = outcomeTitles.Select(outcomeTitle => JObject.FromObject(new { title = outcomeTitle }));
            JObject jsonObject = JObject.FromObject(new
            {
                broadcaster_id = BroadcasterID,
                title = predictionTitle,
                outcomes = predictionOutcomes,
                prediction_window = durationSeconds
            });
            
            (TwitchResponseCode responseCode, string responseBody) response = await TwitchRequest.AwaitablePost("predictions", jsonObject.ToString());
            
            bool success = response.responseCode == TwitchResponseCode.OK;
            return (response.responseCode, success ? JsonUtility.FromJson<Data<Prediction>>(response.responseBody).GetFirst() : null);
        }
        
        /// <summary>
        /// Resolve a locked prediction
        /// https://dev.twitch.tv/docs/api/reference/#end-prediction
        /// </summary>
        /// <param name="predictionId">The ID of the prediction to solve</param>
        /// <param name="winningOutcomeId">The ID of the predictions winning outcome. Is optional when canceling</param>
        /// <param name="status">The status to set the prediction to</param>
        /// <returns>Awaitable response code and body from requesting to resolve a prediction</returns>
        public async Awaitable<(TwitchResponseCode responseCode, Prediction prediction)> ResolvePrediction(string predictionId, string winningOutcomeId, Prediction.Status status = Prediction.Status.RESOLVED)
        {
            JObject jsonObject = JObject.FromObject(new
            {
                broadcaster_id = BroadcasterID,
                id = predictionId,
                status = status.ToString(),
                winning_outcome_id = winningOutcomeId
            });
            
            (TwitchResponseCode responseCode, string responseBody) response = await TwitchRequest.AwaitablePatch("predictions", jsonObject.ToString());
            
            bool success = response.responseCode == TwitchResponseCode.OK;
            return (response.responseCode, success ? JsonUtility.FromJson<Data<Prediction>>(response.responseBody).GetFirst() : null);
        }

        #endregion

        #region Ad Requests
        
        /// <summary>
        /// Get the ad schedule
        /// https://dev.twitch.tv/docs/api/reference/#get-ad-schedule
        /// </summary>
        /// <returns>Awaitable response code and body from requesting the ad schedule</returns>
        public async Awaitable<(TwitchResponseCode responseCode, AdSchedule schedule)> GetAdSchedule()
        {
            (TwitchResponseCode responseCode, string responseBody) response =  await TwitchRequest.AwaitableGet($"channels/ads?broadcaster_id={BroadcasterID}");
            
            bool success = response.responseCode == TwitchResponseCode.OK;
            return (response.responseCode, success ? JsonUtility.FromJson<Data<AdSchedule>>(response.responseBody).GetFirst() : null);
            
        }
        
        /// <summary>
        /// Snooze the next ad
        /// https://dev.twitch.tv/docs/api/reference/#snooze-next-ad
        /// </summary>
        /// <returns>Awaitable response code and body from requesting to snooze the next ad</returns>
        public async Awaitable<(TwitchResponseCode responseCode, AdSchedule schedule)> SnoozeNextAd()
        {
            (TwitchResponseCode responseCode, string responseBody) response = await TwitchRequest.AwaitablePost($"channels/ads/schedule/snooze?broadcaster_id={BroadcasterID}", new JObject().ToString());
            
            bool success = response.responseCode == TwitchResponseCode.OK;
            return (response.responseCode, success ? JsonUtility.FromJson<Data<AdSchedule>>(response.responseBody).GetFirst() : null);
        }
        
        #endregion

        #region Chat Requests
        
        /// <summary>
        /// Sends a message to the broadcaster’s chat room
        /// https://dev.twitch.tv/docs/api/reference/#send-chat-message
        /// </summary>
        /// <param name="message">The message to send. The message is limited to a maximum of 500 characters</param>
        /// <param name="pin">If true, the message will be sent and immediately pinned</param>
        /// <returns>Awaitable response code and body from requesting to send a chat message</returns>
        public async Awaitable<(TwitchResponseCode responseCode, ChatMessage chatMessage)> ChatSendMessage(string message, bool pin = false)
        {
            JObject jsonObject = JObject.FromObject(new
            {
                broadcaster_id = BroadcasterID,
                sender_id = BroadcasterID,
                message,
                pin
            });
            
            (TwitchResponseCode responseCode, string responseBody) response = await TwitchRequest.AwaitablePost($"chat/messages", jsonObject.ToString());
            
            bool success = response.responseCode == TwitchResponseCode.OK;
            return (response.responseCode, success ? JsonUtility.FromJson<Data<ChatMessage>>(response.responseBody).GetFirst() : null);
        }
        
        /// <summary>
        /// Sends an announcement to the broadcaster’s chat room
        /// https://dev.twitch.tv/docs/api/reference/#send-chat-announcement
        /// </summary>
        /// <param name="message">The announcement to make in the broadcaster’s chat room. Announcements are limited to a maximum of 500 characters</param>
        /// <param name="color">The color used to highlight the announcement</param>
        /// <returns>Awaitable response code and body from requesting to send a chat announcement</returns>
        public async Awaitable<(TwitchResponseCode responseCode, ChatMessage chatMessage)> ChatSendAnnouncement(string message, ChatColor color = ChatColor.primary)
        {
            JObject jsonObject = JObject.FromObject(new
            {
                message,
                color = color.ToString()
            });
            
            (TwitchResponseCode responseCode, string responseBody) response = await TwitchRequest.AwaitablePost($"chat/announcements?broadcaster_id={BroadcasterID}&moderator_id={BroadcasterID}", jsonObject.ToString());
            
            bool success = response.responseCode == TwitchResponseCode.OK;
            return (response.responseCode, success ? JsonUtility.FromJson<Data<ChatMessage>>(response.responseBody).GetFirst() : null);
        }
        
        #endregion
    }
}