// © Copyright 2025 Mahuni Game Studios

using System.Collections.Generic;
using System.Linq;
using Mahuni.Twitch.Extension;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A class to demonstrate and test the authentication and calls using web requests to the Twitch API.
/// Use it together with the TwitchWebRequestExtension_Demo scene.
/// </summary>
public class TwitchWebRequestExtensionDemoUI : MonoBehaviour
{
    [Header("Authentication")] 
    public TMP_InputField channelNameText;
    public TMP_InputField twitchClientIdText;
    public TextMeshProUGUI authenticationDescriptionText;
    public Button authenticateButton, resetAuthenticationButton;
    public GameObject authenticateBlocker;

    [Header("Rewards")] 
    public TMP_InputField rewardTitleText;
    public TMP_InputField rewardCostText;
    public TextMeshProUGUI rewardResultText;
    public Button createRewardButton, deleteRewardButton, getRewardsButton;
    public GameObject rewardBlocker;
    private string rewardId;
    private TwitchWebRequests twitchWebRequests;
    
    /// <summary>
    /// Start is called on the frame when a script is enabled just before any of the Update methods are called for the first time.
    /// </summary>
    private void Start()
    {
        authenticateButton.onClick.AddListener(OnAuthenticationButtonClicked);
        resetAuthenticationButton.onClick.AddListener(OnResetAuthenticationButtonClicked);
        createRewardButton.onClick.AddListener(OnCreateRewardButtonClicked);
        deleteRewardButton.onClick.AddListener(OnDeleteRewardButtonClicked);
        getRewardsButton.onClick.AddListener(OnGetRewardsButtonClicked);
        resetAuthenticationButton.interactable = TwitchLocalStorage.HasToken();
        authenticationDescriptionText.text = "Enter the channel name and your Twitch client ID to authenticate.";
        ValidateFields();
    }

    #region Authentication

    /// <summary>
    /// The authentication button was clicked by the user
    /// </summary>
    private void OnAuthenticationButtonClicked()
    {
        authenticationDescriptionText.text = "<color=\"orange\">Authentication ongoing...";
        
        TwitchWebRequestAuthentication.OnAuthenticated += OnAuthenticated;
        TwitchWebRequestAuthentication.ConnectionInformation infos = new(twitchClientIdText.text, new List<string>(){TwitchWebRequestAuthentication.ConnectionInformation.CHANNEL_MANAGE_REDEMPTIONS});
        TwitchWebRequestAuthentication.StartAuthenticationValidation(this, infos);

        twitchWebRequests = new TwitchWebRequests(channelNameText.text);
    }

    /// <summary>
    /// The authentication returned with a result
    /// </summary>
    /// <param name="success">True if authentication was successful</param>
    private void OnAuthenticated(bool success)
    {
        if (success) authenticationDescriptionText.text = "<color=\"green\">Authentication successful!";
        else authenticationDescriptionText.text = "<color=\"red\">Authentication failed!";
        resetAuthenticationButton.interactable = success;
        ValidateFields();
    }

    /// <summary>
    /// The reset authentication button was clicked by the user
    /// </summary>
    private void OnResetAuthenticationButtonClicked()
    {
        authenticationDescriptionText.text = "<color=\"orange\">You cleared the token and need to authenticate again.";
        resetAuthenticationButton.interactable = false;
        TwitchWebRequestAuthentication.Reset();
        ValidateFields();
    }

    #endregion

    #region Rewards

    /// <summary>
    /// The create reward button was clicked by the user
    /// </summary>
    private async void OnCreateRewardButtonClicked()
    {
        if (rewardTitleText.text.Length > 45)
        {
            rewardResultText.text = "<color=\"red\">The maximum length of a reward is 45 characters.!";
            Debug.LogError(rewardResultText.text);
            return;
        }
        
        long.TryParse(rewardCostText.text, out long cost);
        if (cost < 1)
        {
            rewardResultText.text = "<color=\"red\">The minimum cost of a reward is 1.";
            Debug.LogError(rewardResultText.text);
            return;
        }
        
        (TwitchResponseCode responseCode, string responseBody) response = await twitchWebRequests.CreateReward(rewardTitleText.text, cost);
        
        if (response.responseCode == TwitchResponseCode.OK)
        {
            TwitchCreateRewardResponse result = (TwitchCreateRewardResponse)JsonUtility.FromJson(response.responseBody, typeof(TwitchCreateRewardResponse));
            rewardId = result.GetData().id;
        
            rewardResultText.text = "<color=\"green\">Reward created!";
            Debug.Log(rewardResultText.text);
            ValidateFields();
        }
        else
        {
            rewardResultText.text = "<color=\"red\">Error occured for creating a reward: " + response.responseCode;

            // Give additional hint that this might be due to reward already present in 
            if (response.responseCode == TwitchResponseCode.BAD_REQUEST)
            {
                rewardResultText.text += " You might have this reward already created, then please delete from Twitch website.";
            }
            
            Debug.LogError(rewardResultText.text);
        }
    }

    /// <summary>
    /// The delete reward button was clicked by the user
    /// </summary>
    private async void OnDeleteRewardButtonClicked()
    {
        TwitchResponseCode responseCode = await twitchWebRequests.DeleteReward(rewardId);

        // Reading the documentation, we see that code 204 actually means deletion was a success
        if (responseCode == TwitchResponseCode.NO_CONTENT)
        {
            rewardResultText.text = "<color=\"green\">Reward deleted!";
            Debug.Log(rewardResultText.text);
        
            rewardId = string.Empty;
            ValidateFields();
        }
        else
        {
            rewardResultText.text = $"<color=\"red\">Error occured for deleting a reward with id {rewardId}: {responseCode}";
            Debug.LogError(rewardResultText.text);
        }
    }
    
    /// <summary>
    /// The get rewards button was clicked by the user
    /// </summary>
    private async void OnGetRewardsButtonClicked()
    {
        (TwitchResponseCode responseCode, string responseBody) response = await twitchWebRequests.GetRewards();
       
        if (response.responseCode == TwitchResponseCode.OK)
        {
            TwitchCreateRewardResponse result = (TwitchCreateRewardResponse)JsonUtility.FromJson(response.responseBody, typeof(TwitchCreateRewardResponse));
            rewardResultText.text = $"<color=\"green\">Rewards: {(!result.data.Any() ? "no rewards yet." : string.Join(", ", result.data.Select(d => d.title)))}";
            Debug.Log(rewardResultText.text);
        }
        else
        {
            rewardResultText.text = $"<color=\"red\">Error occured for getting rewards list: {response.responseCode}";
            Debug.LogError(rewardResultText.text);
        }
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Validate the UI elements and their interactivity
    /// </summary>
    public void ValidateFields()
    {
        bool isAuthenticated = TwitchWebRequestAuthentication.IsAuthenticated();

        authenticateButton.interactable = !isAuthenticated && !string.IsNullOrEmpty(channelNameText.text) && !string.IsNullOrEmpty(twitchClientIdText.text);
        createRewardButton.interactable = isAuthenticated && !string.IsNullOrEmpty(rewardTitleText.text) && !string.IsNullOrEmpty(rewardCostText.text);
        deleteRewardButton.interactable = isAuthenticated && !string.IsNullOrEmpty(rewardId);
        getRewardsButton.interactable = isAuthenticated;
        
        authenticateBlocker.SetActive(isAuthenticated);
        rewardBlocker.SetActive(!isAuthenticated);
        
        // Channel name or Client ID input is missing
        if (!isAuthenticated && (string.IsNullOrEmpty(channelNameText.text) || string.IsNullOrEmpty(twitchClientIdText.text)))
        {
            authenticationDescriptionText.text = "Enter the channel name and your Twitch client ID to authenticate.";
        }
    }

    #endregion
}