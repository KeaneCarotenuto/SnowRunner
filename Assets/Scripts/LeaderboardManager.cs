using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LeaderboardManager : MonoBehaviour
{
    public TextMeshProUGUI m_highscoreText;
    public TextMeshProUGUI m_topSpeedText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // get values from player prefs
        float highscore = PlayerPrefs.GetInt("Highscore", 0);
        float topSpeed = PlayerPrefs.GetFloat("TopSpeed", 0.0f);

        // set the text
        m_highscoreText.text = "HIGHSCORE: " + highscore.ToString() + "pts";
        m_topSpeedText.text = "TOP-SPEED: " + topSpeed.ToString("0.0") + "km/h";
    }
}
