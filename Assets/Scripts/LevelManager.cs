using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static void LoadMenu()
    {
        Time.timeScale = 1;
        UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
    }

    public static void LoadGame()
    {
        Time.timeScale = 1;
        UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
    }
    
    public static void RestartGame()
    {
        Time.timeScale = 1;
        LoadGame();
    }

    public static void Quit(){
        Application.Quit();
    }
}
