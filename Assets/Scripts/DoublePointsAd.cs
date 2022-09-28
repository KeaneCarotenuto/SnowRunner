using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Advertisements;
using TMPro;

public class DoublePointsAd : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
{
    [SerializeField] public Button _showAdButton;
    [SerializeField] string _androidAdUnitId = "Interstitial_Android";
    [SerializeField] string _iOsAdUnitId = "Interstitial_iOS";
    string _adUnitId;

    public float m_doublePointsMinutes = 60.0f;
 
    void Awake()
    {
        // Get the Ad Unit ID for the current platform:
        _adUnitId = (Application.platform == RuntimePlatform.IPhonePlayer)
            ? _iOsAdUnitId
            : _androidAdUnitId;

        //Disable the button until the ad is ready to show:
        _showAdButton.interactable = false;
    }

    private void Start() {
        // load ad
        //LoadAd();
    }

    private void OnEnable() {
        AdsInitializer adsInitializer = FindObjectOfType<AdsInitializer>();
        if (adsInitializer != null) {
            adsInitializer.onInitialized += LoadAd;
        }
    }

    private void OnDisable() {
        AdsInitializer adsInitializer = FindObjectOfType<AdsInitializer>();
        if (adsInitializer != null) {
            adsInitializer.onInitialized -= LoadAd;
        }
    }

    private void Update() {
        // get the date and time from PlayerPrefs
        string lastAdShownString = PlayerPrefs.GetString("LastAdShown", "");

        // if there is a date and time stored
        if (lastAdShownString != "") {
            // convert the date and time string to a DateTime
            System.DateTime lastAdShown = System.DateTime.Parse(lastAdShownString);

            // if the time since the last ad shown is greater than the double points time
            if (System.DateTime.Now.Subtract(lastAdShown).TotalMinutes > m_doublePointsMinutes) {
                // disable the double points
                PlayerPrefs.SetInt("DoublePoints", 0);
            }
            else{
                // update the double points text
                if (_showAdButton != null) {
                    int mins = (int)(m_doublePointsMinutes - System.DateTime.Now.Subtract(lastAdShown).TotalMinutes);
                    _showAdButton.GetComponentInChildren<TextMeshProUGUI>().text = "Double Points: " + mins + " minutes\nWatch Ad to refresh";
                } 
            }
        }
        else{
            // disable the double points
            PlayerPrefs.SetInt("DoublePoints", 0);
        }

    }
 
    // Load content to the Ad Unit:
    public void LoadAd()
    {
        // IMPORTANT! Only load content AFTER initialization (in this example, initialization is handled in a different script).
        Debug.Log("Loading Ad: " + _adUnitId);
        Advertisement.Load(_adUnitId, this);
    }
 
    // Show the loaded content in the Ad Unit:
    public void ShowAd()
    {
        // Disable the button:
        _showAdButton.interactable = false;
        // Note that if the ad content wasn't previously loaded, this method will fail
        Debug.Log("Showing Ad: " + _adUnitId);
        Advertisement.Show(_adUnitId, this);
    }
 
    // Implement Load Listener and Show Listener interface methods: 
    public void OnUnityAdsAdLoaded(string adUnitId)
    {
        Debug.Log("Ad Loaded: " + adUnitId);
 
        if (adUnitId.Equals(_adUnitId))
        {
            // Configure the button to call the ShowAd() method when clicked:
            _showAdButton.onClick.AddListener(ShowAd);
            // Enable the button for users to click:
            _showAdButton.interactable = true;
        }
    }
 
    public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
    {
        Debug.Log($"Error loading Ad Unit: {adUnitId} - {error.ToString()} - {message}");
        // Optionally execute code if the Ad Unit fails to load, such as attempting to try again.
    }
 
    public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
    {
        Debug.Log($"Error showing Ad Unit {adUnitId}: {error.ToString()} - {message}");
        // Optionally execute code if the Ad Unit fails to show, such as loading another ad.
    }
 
    public void OnUnityAdsShowStart(string adUnitId) { }
    public void OnUnityAdsShowClick(string adUnitId) { }
    public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState) {
        // Set the date and time when the ad was last shown:
        PlayerPrefs.SetString("LastAdShown", System.DateTime.Now.ToString());

        // enable the double points
        PlayerPrefs.SetInt("DoublePoints", 1);

        // load ad
        LoadAd();

        Social.ReportProgress(SnowRunnerAchievements.achievement_doubled_up, 100.0f, (bool success) => {
                // handle success or failure
            });
     }
}