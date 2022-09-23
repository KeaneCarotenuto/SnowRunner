using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameUI : MonoBehaviour
{
    public GameObject pausePanel;
    public GameObject pauseButton;

    // Start is called before the first frame update
    void Start()
    {
        pausePanel.SetActive(false);
    }

    private void OnEnable() {
        // bind the pause button to pause
        pauseButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OpenPauseMenu);
    }

    private void OnDisable() {
        // unbind the pause button to pause
        pauseButton.GetComponent<UnityEngine.UI.Button>().onClick.RemoveListener(OpenPauseMenu);
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

        pausePanel.SetActive(true);

        // disable the pause button
        pauseButton.SetActive(false);
    }

    public void ClosePauseMenu(){
        Time.timeScale = 1;

        pausePanel.SetActive(false);

        // enable the pause button
        pauseButton.SetActive(true);
    }

    public void TogglePauseMenu(){
        if (pausePanel.activeSelf){
            ClosePauseMenu();
        } else {
            OpenPauseMenu();
        }
    }
}
