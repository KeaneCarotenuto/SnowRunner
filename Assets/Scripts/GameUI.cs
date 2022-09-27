using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    public GameObject m_pausePanel;
    public GameObject m_pauseButton;

    public TextMeshProUGUI m_volumeText;
    public Slider m_volumeSlider;

    public Toggle m_swapControlsToggle;
    public TextMeshProUGUI m_leftKeyText;
    public TextMeshProUGUI m_rightKeyText;

    // Start is called before the first frame update
    void Start()
    {
        m_pausePanel.SetActive(false);

        // set slider
        m_volumeSlider.value = PlayerPrefs.GetFloat("Volume", 1.0f);

        // set SwapControls toggle
        m_swapControlsToggle.isOn = PlayerPrefs.GetInt("SwapControls", 0) == 1;
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

        // disable key text
        m_leftKeyText.gameObject.SetActive(false);
        m_rightKeyText.gameObject.SetActive(false);
    }

    public void ClosePauseMenu(){
        Time.timeScale = 1;

        m_pausePanel.SetActive(false);

        // enable the pause button
        m_pauseButton.SetActive(true);

        // enable key text
        m_leftKeyText.gameObject.SetActive(true);
        m_rightKeyText.gameObject.SetActive(true);
    }

    public void TogglePauseMenu(){
        if (m_pausePanel.activeSelf){
            ClosePauseMenu();
        } else {
            OpenPauseMenu();
        }
    }

    public void VolumeSliderChanged(float value){
        // set the player prefs
        PlayerPrefs.SetFloat("Volume", value);

        m_volumeText.text = "Volume: " + (int)(value * 100) + "%";
    }

    public void SwapRotationControlsToggleChanged(bool value){
        // set the player prefs
        PlayerPrefs.SetInt("SwapControls", value ? 1 : 0);
    }
}
