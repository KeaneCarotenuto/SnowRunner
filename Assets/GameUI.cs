using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameUI : MonoBehaviour
{
    public GameObject m_pausePanel;
    public GameObject m_pauseButton;

    // Start is called before the first frame update
    void Start()
    {
        m_pausePanel.SetActive(false);
    }

    private void OnEnable() {
        // bind the pause button to pause
        m_pauseButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OpenPauseMenu);
    }

    private void OnDisable() {
        // unbind the pause button to pause
        m_pauseButton.GetComponent<UnityEngine.UI.Button>().onClick.RemoveListener(OpenPauseMenu);
    }

    // Update is called once per frame
    void Update()
    {
        // if escape is pressed, toggle the pause menu
        if (Input.GetKeyDown(KeyCode.Escape)) {
            TogglePauseMenu();
        }
    }

    public void OpenPauseMenu(){
        Time.timeScale = 0.05f;

        m_pausePanel.SetActive(true);

        // disable the pause button
        m_pauseButton.SetActive(false);
    }

    public void ClosePauseMenu(){
        Time.timeScale = 1;

        m_pausePanel.SetActive(false);

        // enable the pause button
        m_pauseButton.SetActive(true);
    }

    public void TogglePauseMenu(){
        if (m_pausePanel.activeSelf){
            ClosePauseMenu();
        } else {
            OpenPauseMenu();
        }
    }
}
