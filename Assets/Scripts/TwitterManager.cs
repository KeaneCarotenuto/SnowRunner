using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwitterManager : MonoBehaviour
{
    public void OpenTwitter()
    {
        string twitterAddress = "http://twitter.com/intent/tweet";
        string message = "GET THIS AWERSOME GAME";//text string
        string descriptionParameter = "#SnowRunner";
        string appStoreLink = "https://play.google.com/store/apps/details?id=com.DefaultCompany.SnowRunner";
        Application.OpenURL(twitterAddress + "?text=" +
        WWW.EscapeURL(message + "\n\n" +
        descriptionParameter + "\n" +
        appStoreLink));

    }
}
