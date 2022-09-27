using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// DoNotDestroy the MusicManager, 
public class MusicManager : MonoBehaviour
{
    static MusicManager instance = null;

    public float m_volume = 0.5f;

    public float m_fadeInTime = 2.0f;
    private float m_fadeInTimer = 0.0f;

    private void Awake() {
        if (instance != null) {
            Destroy(gameObject);
        } else {
            instance = this;
            GameObject.DontDestroyOnLoad(gameObject);
        }
    }

    private void Start() {
        GetComponent<AudioSource>().Play();

        // set the volume to 0
        GetComponent<AudioSource>().volume = 0.0f;

        m_fadeInTimer = m_fadeInTime;
    }

    private void Update() {
        // fade in the music
        if (m_fadeInTimer > 0.0f) {
            m_fadeInTimer -= Time.deltaTime;
            GetComponent<AudioSource>().volume = Mathf.Lerp(0.0f, m_volume * PlayerPrefs.GetFloat("Volume", 1f), 1.0f - m_fadeInTimer / m_fadeInTime);
        }
        else{
            GetComponent<AudioSource>().volume = m_volume * PlayerPrefs.GetFloat("Volume", 1f);
        }

        // follow camera
        transform.position = Camera.main.transform.position;
    }
}
