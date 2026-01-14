// © Copyright 2025 Mahuni Game Studios

using System.Collections.Generic;
using System.Linq;
using Mahuni.Twitch.Extension;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// A class to demonstrate and test the web requests to the Twitch API.
/// Use it together with the TwitchWebRequestExtension_Demo scene.
/// </summary>
public class TwitchWebRequestExtensionDemoUI : MonoBehaviour
{
    private TwitchWebRequestHandler twitchWebRequestHandler;
    
    [Header("Authentication")] 
    public TMP_InputField channelNameText;
    public TMP_InputField twitchClientIdText;
    public TextMeshProUGUI authenticationDescriptionText;
    public Button authenticateButton;
    public GameObject authenticateBlocker;

    [Header("Rewards")] 
    public TMP_InputField rewardTitleText;
    public TMP_InputField rewardCostText;
    public TextMeshProUGUI rewardResultText;
    public Button createRewardButton, deleteRewardButton, getRewardsButton;
    public GameObject rewardBlocker;
    private string rewardId;

    /// <summary>
    /// Start is called on the frame when a script is enabled just before any of the Update methods are called for the first time.
    /// </summary>
    private void Start()
    {
        TwitchAuthentication.OnAuthenticated += OnAuthenticated;
        TwitchAuthentication.Reset();  // If you authenticated before, it might be better to reset the token, to be sure the right permissions are set
        
        channelNameText.onValueChanged.AddListener(ValidateFields);
        twitchClientIdText.onValueChanged.AddListener(ValidateFields);
        authenticationDescriptionText.text = "Enter the channel name and your Twitch client ID to authenticate.";
        authenticateButton.onClick.AddListener(OnAuthenticationButtonClicked);
        
        rewardTitleText.onValueChanged.AddListener(ValidateFields);
        rewardCostText.onValueChanged.AddListener(ValidateFields);
        createRewardButton.onClick.AddListener(OnCreateRewardButtonClicked);
        deleteRewardButton.onClick.AddListener(OnDeleteRewardButtonClicked);
        getRewardsButton.onClick.AddListener(OnGetRewardsButtonClicked);
        
        ValidateFields();
    }

    #region Authentication

    /// <summary>
    /// The authentication button was clicked by the user
    /// </summary>
    private void OnAuthenticationButtonClicked()
    {
        authenticationDescriptionText.text = "<color=\"orange\">Authentication ongoing...";
        TwitchAuthentication.ConnectionInformation infos = new(twitchClientIdText.text, new List<string>(){TwitchAuthentication.ConnectionInformation.CHANNEL_MANAGE_REDEMPTIONS});
        TwitchAuthentication.StartAuthenticationValidation(this, infos);
    }

    /// <summary>
    /// The authentication returned with a result
    /// </summary>
    /// <param name="success">True if authentication was successful</param>
    private void OnAuthenticated(bool success)
    {
        if (success)
        {
            authenticationDescriptionText.text = "<color=\"green\">Authentication successful!";
            ConnectTwitchWebRequests();
        }
        else
        {
            authenticationDescriptionText.text = "<color=\"red\">Authentication failed!";
        }
    }

    #endregion

    #region Connect Web Requests

    /// <summary>
    /// Connect to the web request class
    /// </summary>
    private async void ConnectTwitchWebRequests()
    {
        twitchWebRequestHandler = new TwitchWebRequestHandler(channelNameText.text);
        bool success = await twitchWebRequestHandler.Connect(channelNameText.text);
        if (!success)
        {
            Debug.LogError("<color=\"red\">Twitch web request connection was not established!");
            return;
        }
        
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
        
        (TwitchResponseCode responseCode, string responseBody) response = await twitchWebRequestHandler.CreateReward(rewardTitleText.text, cost);
        
        if (response.responseCode == TwitchResponseCode.OK)
        {
            Reward reward = JsonUtility.FromJson<Data<Reward>>(response.responseBody).GetFirst();
            rewardId = reward.id;
        
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
        TwitchResponseCode responseCode = await twitchWebRequestHandler.DeleteReward(rewardId);

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
        (TwitchResponseCode responseCode, string responseBody) response = await twitchWebRequestHandler.GetRewards();
       
        if (response.responseCode == TwitchResponseCode.OK)
        {
            Reward[] rewardData = JsonUtility.FromJson<Data<Reward>>(response.responseBody).data;
            rewardResultText.text = $"<color=\"green\">Rewards: {(!rewardData.Any() ? "No rewards yet." : string.Join(", ", rewardData.Select(d => d.title)))}";
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
    /// Update is called every frame if the MonoBehaviour is enabled
    /// </summary>
    private void Update()
    {
        // Tab through formular
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (EventSystem.current.currentSelectedGameObject == null || EventSystem.current.currentSelectedGameObject == rewardCostText.gameObject)
            {
                EventSystem.current.SetSelectedGameObject(channelNameText.interactable ? channelNameText.gameObject : rewardTitleText.gameObject, new BaseEventData(EventSystem.current));
            }
            else if (EventSystem.current.currentSelectedGameObject == channelNameText.gameObject)
            {
                EventSystem.current.SetSelectedGameObject(twitchClientIdText.gameObject, new BaseEventData(EventSystem.current));
            }
            else if (EventSystem.current.currentSelectedGameObject == twitchClientIdText.gameObject)
            {
                EventSystem.current.SetSelectedGameObject(rewardTitleText.interactable ? rewardTitleText.gameObject : channelNameText.gameObject, new BaseEventData(EventSystem.current));
            }
            else if (EventSystem.current.currentSelectedGameObject == rewardTitleText.gameObject)
            {
                EventSystem.current.SetSelectedGameObject(rewardCostText.interactable ? rewardCostText.gameObject : channelNameText.gameObject, new BaseEventData(EventSystem.current));
            }
        }
    }

    /// <summary>
    /// Validate the UI elements and their interactivity
    /// </summary>
    public void ValidateFields(string value = "")
    {
        bool isAuthenticated = TwitchAuthentication.IsAuthenticated();

        channelNameText.interactable = !isAuthenticated;
        twitchClientIdText.interactable = !isAuthenticated;
        authenticateButton.interactable = !isAuthenticated && !string.IsNullOrEmpty(channelNameText.text) && !string.IsNullOrEmpty(twitchClientIdText.text);
        authenticateBlocker.SetActive(isAuthenticated);
        
        // Channel name or Client ID input is missing
        if (!isAuthenticated && (string.IsNullOrEmpty(channelNameText.text) || string.IsNullOrEmpty(twitchClientIdText.text)))
        {
            authenticationDescriptionText.text = "Enter the channel name and your Twitch client ID to authenticate.";
        }
        
        rewardTitleText.interactable = isAuthenticated;
        rewardCostText.interactable = isAuthenticated;
        createRewardButton.interactable = isAuthenticated && !string.IsNullOrEmpty(rewardTitleText.text) && !string.IsNullOrEmpty(rewardCostText.text);
        deleteRewardButton.interactable = isAuthenticated && !string.IsNullOrEmpty(rewardId);
        getRewardsButton.interactable = isAuthenticated;
        rewardBlocker.SetActive(!isAuthenticated);
    }

    #endregion
}