# Unity Twitch Web Request Extension by Mahuni Game Studios

[![Downloads](https://img.shields.io/github/downloads/mahuni-game-studios/twitch-web-request-unity-extension/total.svg)](https://github.com/mahuni-game-studios/twitch-web-request-unity-extension/releases/)
[![Latest Version](https://img.shields.io/github/v/release/mahuni-game-studios/twitch-web-request-unity-extension)](https://github.com/mahuni-game-studios/twitch-web-request-unity-extension/releases/tag/v1.0)

An extension to use the Twitch API directly via Unity web requests. Including authorization, local storage of the token and an example implementation showing reward creation / deletion via web request.

Available HTTP request types:
- GET
- POST
- DELETE
- PATCH
- PUT

Choose between three different ways to call the web requests:
- synchronous using `IEnumerator`
- asynchronous using `Awaitable`
- asynchronous using `Task`

## Code Snippet Examples

The simplest implementation to give your game permission to access and use the Twitch API with web requests!

### Authentication

The authentication logic is packed in a git submodule and is needed if you want to use the extension as is. Read [this](#twitch-authentication-extension) to find out how to get it.

```cs
public class YourUnityClass : MonoBehaviour
{
    private void Start()
    {        
        // Register to authentication finished event
        TwitchAuthentication.OnAuthenticated += OnAuthenticated;
        
        // Set relevant information to the connection
        TwitchAuthentication.ConnectionInformation infos = new("your-client-id", new List<string>(){TwitchWebRequestAuthentication.ConnectionInformation.CHANNEL_MANAGE_REDEMPTIONS});
        
        // Start authentication
        TwitchAuthentication.StartAuthenticationValidation(this, infos);
    }
    
    private void OnAuthenticated(bool success)
    {
        if (success)
        {
            // TODO: Start using the web requests to the Twitch API from here!
        }
    }
}
```

### Create a custom reward asynchronously

Please note that creating a reward and any other calls to use the Twitch API only works after successful authentication.

```cs
public class YourUnityClass : MonoBehaviour
{
    private TwitchWebRequests twitchWebRequests;
    
    private void Start()
    { 
        // Connect to the web request class
        ConnectTwitchWebRequests(); 
    }
    
    private async void ConnectTwitchWebRequests()
    {
        twitchWebRequests = new TwitchWebRequests("channel-name");
        bool success = await twitchWebRequests.Connect(channelNameText.text);
        if (success)
        {
            // Create a reward
            OnCreateReward("My awesome reward", 500);
        }  
    }
      
    private async void OnCreateReward(string title, long cost)
    {
        var response = await twitchWebRequests.CreateReward(title, cost);        
        if (response.responseCode == TwitchResponseCode.OK)
        {
            // Retrieve the result and serialize from JSON format back to a class object
            Reward reward = JsonUtility.FromJson<Data<Reward>>(response.responseBody).GetFirst();
            Debug.Log($"Reward {reward.title} with id {reward.id} was successfully created!");
        }        
    }  
}
```

## Installation Guide

### Prerequisites

To be able to interact with the Twitch API, you need to register your Twitch application. You can follow how to do that with this [Guide from Twitch](https://dev.twitch.tv/docs/authentication/register-app/). In short:

1. Sign up to Twitch if you don't have already
2. Navigate to the [Twitch Developer Console](https://dev.twitch.tv/console/apps)
3. Create a new application and select an appropriate category, e.g. as Game Integration
4. Click on *Manage* on your application entry and you will be presented a `Client ID`. This ID will be needed to interact with Twitch.

<font color="red">The `Client ID` should stay secret, do not share or show it!</font>

#### Twitch Authentication Extension

This repository uses the [Unity Twitch Authentication Extension by Mahuni Game Studios](https://github.com/mahuni-game-studios/twitch-authentication-unity-extension) as git submodule. Be sure to either pull the submodule or grab / download / clone the authentication extension manually.

- To clone the repository with submodules: `git clone --recurse-submodules`
- To update the cloned repository to get the submodules: `git submodule update --init --recursive`
- To download the extension, go to [GitHub](https://github.com/mahuni-game-studios/twitch-authentication-unity-extension), download it and drag and drop it somewhere into the `Assets/` folder

#### Demo scene

To use the provided `TwitchWebRequestExtension_Demo` scene, the `TextMeshPro` package is required. If you do not have it yet imported into your project, simply opening the `TwitchWebRequestExtension_Demo.scene` will ask if you want to import it. Select the `Import TMP Essentials` option, close the `TMP Importer` and you are good to go.

### Setup project
1. Either open this project directly or import it to your own project in the Unity Editor
2. Make sure the git submodules are installed, see [here](#twitch-authentication-extension)
3. Start using the `TwitchWebRequestAuthentication` and the `TwitchWebRequests` scripts right away, or take a look into the `TwitchWebRequestExtension_Demo` scene to find an easy example implementation.