using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Advertisements;
using TMPro;
using Facebook.Unity;
using System.Linq;
using System;

public class DeathManager : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
{
    public GameObject m_deathScreen;
    public TextMeshProUGUI m_pointsText;
    public TextMeshProUGUI m_speedText;

    public Button m_restartButton;
    public Button m_reviveButton;
    public Button m_shareTwitterButton;
    public Button m_shareFacebookButton;

    
    [SerializeField] string _androidAdUnitId = "Rewarded_Android";
    [SerializeField] string _iOSAdUnitId = "Rewarded_iOS";
    string _adUnitId = null; // This will remain null for unsupported platforms

    private void Awake() {
                // Get the Ad Unit ID for the current platform:
#if UNITY_IOS
        _adUnitId = _iOSAdUnitId;
#elif UNITY_ANDROID
        _adUnitId = _androidAdUnitId;
#endif

        //Disable the button until the ad is ready to show:
        m_reviveButton.interactable = false;

        // facebook
        if (!FB.IsInitialized)
        { // if not initialized
            FB.Init(); // initialize
        }
        else
        {
            // Send an app activation event to Facebook when your app is activated.
            FB.ActivateApp(); // Activate event
        }

        // hide death screen
        m_deathScreen.SetActive(false);
    }

    private void OnEnable() {
        // share buttons
        m_shareTwitterButton.onClick.AddListener(ShareTwitterClicked);
        m_shareFacebookButton.onClick.AddListener(ShareFacebookClicked);

        // ads
        m_restartButton.onClick.AddListener(RestartClicked);
        m_reviveButton.onClick.AddListener(ReviveClicked);
    }

    private void OnDisable() {
        // share buttons
        m_shareTwitterButton.onClick.RemoveListener(ShareTwitterClicked);
        m_shareFacebookButton.onClick.RemoveListener(ShareFacebookClicked);

        // ads
        m_restartButton.onClick.RemoveListener(RestartClicked);
        m_reviveButton.onClick.RemoveListener(ReviveClicked);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RestartClicked(){
        // reload scene
        LevelManager levelManager = FindObjectOfType<LevelManager>();
    }

    public void ReviveClicked()
    {
        ShowAd();
    }

    public void PlayerDied(){
        // show death screen
        m_deathScreen.SetActive(true);

        // load ad
        Advertisement.Load(_adUnitId, this);

        // update text
        PointsManager pointsManager = FindObjectOfType<PointsManager>();
        PlayerControl pc = FindObjectOfType<PlayerControl>();
        m_pointsText.text = pointsManager.GetTotalPoints() + "pts";
        m_speedText.text = pc.m_topSpeed.ToString("0.0") + "km/h";;
    }

#region Sharing
    public void ShareFacebook()
    {
        if (!FB.IsLoggedIn)
        {
            // Debug.Log("User Not Logged In");
            FB.LogInWithReadPermissions(null, callback: onLogin);
        }
        else
        {

            FB.ShareLink(contentTitle: "SnowRunner",
            contentURL: new System.Uri("http://www.growlgamesstudio.com"),
            contentDescription: GetShareDescription(),
            callback: onShare);
        }
    }

    private void onLogin(ILoginResult result)
    {
        if (result.Cancelled)
        {
            Debug.Log(" user cancelled login");
        }
        else
        {
            ShareFacebook(); // call share() again
        }
    }
    private void onShare(IShareResult result)
    {
        if (result.Cancelled || !string.IsNullOrEmpty(result.Error))
        {
            Debug.Log("sharelink error: " + result.Error);
        }
        else if (!string.IsNullOrEmpty(result.PostId))
        {
            Debug.Log("link shared");
        }
    }

    public void ShareFacebookClicked()
    {
        ShareFacebook();
    }

    public void ShareTwitterClicked()
    {
        string twitterAddress = "http://twitter.com/intent/tweet";
        string message = GetShareDescription();//text string
        string descriptionParameter = "#SnowRunner";
        string appStoreLink = "https://play.google.com/store/apps/details?id=com.DefaultCompany.SnowRunner";
        Application.OpenURL(twitterAddress + "?text=" +
        WWW.EscapeURL(message + "\n\n" +
        descriptionParameter + "\n" +
        appStoreLink));
    }

    private string GetShareDescription()
    {
        PointsManager pointsManager = FindObjectOfType<PointsManager>();
        PlayerControl pc = FindObjectOfType<PlayerControl>();

        string description = "I just died in SnowRunner! Can you beat my score?";
        description += "\n";
        description += "Points: " + pointsManager.GetTotalPoints() + "pts\n";
        description += "Top Speed: " + pc.m_topSpeed.ToString("0.0") + "km/h";

        return description;
    }

#endregion


#region Adverts
    // Load content to the Ad Unit:
    public void LoadAd()
    {
        // IMPORTANT! Only load content AFTER initialization (in this example, initialization is handled in a different script).
        Debug.Log("Loading Ad: " + _adUnitId);
        Advertisement.Load(_adUnitId, this);
    }

        // If the ad successfully loads, add a listener to the button and enable it:
    public void OnUnityAdsAdLoaded(string adUnitId)
    {
        Debug.Log("Ad Loaded: " + adUnitId);
 
        if (adUnitId.Equals(_adUnitId))
        {
            // Enable the button for users to click:
            m_reviveButton.interactable = true;
        }
    }
 
    // Implement a method to execute when the user clicks the button:
    public void ShowAd()
    {
        // Disable the button:
        m_reviveButton.interactable = false;
        // Then show the ad:
        Advertisement.Show(_adUnitId, this);
    }
 
    // Implement the Show Listener's OnUnityAdsShowComplete callback method to determine if the user gets a reward:
    public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
    {
        if (adUnitId.Equals(_adUnitId) && showCompletionState.Equals(UnityAdsShowCompletionState.COMPLETED))
        {
            Debug.Log("Unity Ads Rewarded Ad Completed");
            
            // Grant a reward.
            PlayerControl pc = GameObject.FindObjectOfType<PlayerControl>();
            pc.Revive();

            // hide death screen
            m_deathScreen.SetActive(false);
        }
    }
 
    // Implement Load and Show Listener error callbacks:
    public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
    {
        Debug.Log($"Error loading Ad Unit {adUnitId}: {error.ToString()} - {message}");
        // Use the error details to determine whether to try to load another ad.
    }
 
    public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
    {
        Debug.Log($"Error showing Ad Unit {adUnitId}: {error.ToString()} - {message}");
        // Use the error details to determine whether to try to load another ad.
    }
 
    public void OnUnityAdsShowStart(string adUnitId) { }
    public void OnUnityAdsShowClick(string adUnitId) { }

#endregion
}