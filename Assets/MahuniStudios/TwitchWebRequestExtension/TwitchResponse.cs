// © Copyright 2025 Mahuni Game Studios

// ReSharper disable InconsistentNaming
namespace Mahuni.Twitch.Extension
{
    using System;
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
    /// A wrapper structure that can be used to get array responses from Twitch
    /// </summary>
    /// <typeparam name="T">A generic type that holds the actual values</typeparam>
    [Serializable]
    public struct Data<T>
    {
        public T[] data;

        public T GetFirst()
        {
            if (data == null || !data.Any())
            {
                throw new ArgumentOutOfRangeException();
            }
            return data[0];
        }
    }
    
    // See https://dev.twitch.tv/docs/api/reference#get-users
    [Serializable]
    public class User
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
    
    // See https://dev.twitch.tv/docs/api/reference#get-custom-reward
    [Serializable]
    public class Reward
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