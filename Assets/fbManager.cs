using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Facebook.Unity;
using System.Linq;

public class fbManager : MonoBehaviour
{
    void Awake()
    {
        if (!FB.IsInitialized)
        { // if not initialized
            FB.Init(); // initialize
        }
        else
        {
            // Send an app activation event to Facebook when your app is activated.
            FB.ActivateApp(); // Activate event
        }
    }

    public void Share()
    {
        if (!FB.IsLoggedIn)
        {
            // Debug.Log("User Not Logged In");
            FB.LogInWithReadPermissions(null, callback: onLogin);
        }
        else
        {

            FB.ShareLink(contentTitle: "Growl Games Studio",
            contentURL: new System.Uri("http://www.growlgamesstudio.com"),
            contentDescription: "Like and Share my page",
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
            Share(); // call share() again
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
} // end of class
