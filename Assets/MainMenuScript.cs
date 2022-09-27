using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
public class MainMenuScript : MonoBehaviour
{ //class start
    bool isUserAuthenticated = false;
    // Use this for initialization
    void Start()
    {
        PlayGamesPlatform.Activate(); // activate playgame platform
        PlayGamesPlatform.DebugLogEnabled = true; //enable debug log
    }
    // Update is called once per frame
    void Update()
    {
        if (!isUserAuthenticated)
        {
            Social.localUser.Authenticate((bool success) =>
            {
                if (success)
                {
                    Debug.Log("You've successfully logged in");
                    isUserAuthenticated = true; // set value to true
                }
                else
                {
                    Debug.Log("Login failed for some reason");
                }
            });
        }
    }
} // end class