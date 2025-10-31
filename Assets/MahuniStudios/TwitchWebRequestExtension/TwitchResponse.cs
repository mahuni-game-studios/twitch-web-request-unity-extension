// © Copyright 2025 Mahuni Game Studios

// ReSharper disable InconsistentNaming
namespace Mahuni.Twitch.Extension
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Response codes that are thrown by the Twitch API
    /// </summary>
    public enum TwitchResponseCode
    {
        OK = 200,
        NO_CONTENT = 204,
        BAD_REQUEST = 400,
        UNAUTHORIZED = 401,
        FORBIDDEN = 403,
        NOT_FOUND = 404,
        INTERNAL_SERVER_ERROR = 500,
    }

    /// <summary>
    /// Base wrapper class to deserialize response bodys
    /// </summary>
    /// <typeparam name="T">The type of response to deserialize</typeparam>
    [Serializable]
    public class TwitchResponse<T>
    {
        public List<T> data = new();

        public T GetData()
        {
            return data.FirstOrDefault();
        }
    }

    /// <summary>
    /// Wrapper class to deserialize response bodys from <see href="https://dev.twitch.tv/docs/api/reference/#check-user-subscription">Check User Subscription</see> 
    /// </summary>
    [Serializable]
    public class TwitchUsersResponse : TwitchResponse<TwitchUsersResponse.UserData>
    {
        public TwitchUsersResponse()
        {
            data.Add(new UserData());
        }

        [Serializable]
        public struct UserData
        {
            public string id;
            public string login;
            public string display_name;
            public string type;
            public string broadcaster_type;
            public string description;
            public string profile_image_url;
            public string offline_image_url;
            public int view_count;
            public string email;
            public string created_at;
        }
    }

    /// <summary>
    /// Wrapper class to deserialize response bodys from <see href="https://dev.twitch.tv/docs/api/reference/#create-custom-rewards">Create Custom Rewards</see> 
    /// </summary> 
    [Serializable]
    public class TwitchCreateRewardResponse : TwitchResponse<TwitchCreateRewardResponse.RewardData>
    {
        public TwitchCreateRewardResponse()
        {
            data.Add(new RewardData());
        }

        [Serializable]
        public struct RewardData
        {
            public string broadcaster_id;
            public string broadcaster_login;
            public string broadcaster_name;
            public string id;
            public string title;
            public string prompt;
            public int cost;
            public Image image;
            public Image default_image;
            public string background_color;
            public bool is_enabled;
            public bool is_user_input_required;
            public MaxSetting max_per_stream_setting;
            public MaxSetting max_per_user_per_stream_setting;
            public Cooldown global_cooldown_setting;
            public bool is_paused;
            public bool is_in_stock;
            public bool should_redemptions_skip_request_queue;
            public int redemptions_redeemed_current_stream;
            public string cooldown_expires_at;

            [Serializable]
            public struct Image
            {
                public string url_1x;
                public string url_2x;
                public string url_4x;
            }

            [Serializable]
            public struct MaxSetting
            {
                public bool is_enabled;
                public long max_per_stream;
            }

            [Serializable]
            public struct Cooldown
            {
                public bool is_enabled;
                public long global_cooldown_seconds;
            }
        }
    }
}