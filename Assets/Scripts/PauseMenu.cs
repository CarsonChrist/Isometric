using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{

    public static bool isPaused = false;
    public GameObject pauseMenu;
    public GameObject HUD;
    public GameObject controls;

    // Update is called once per frame
    void Update()
    {
        // Toggle pausing the game
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused && controls.activeSelf == false)
            {
                resume();
            }
            if (!isPaused)
            {
                pause();
            }
        }
    }
    // Resume game
    public void resume()
    {
        pauseMenu.SetActive(false);
        HUD.SetActive(true);
        Time.timeScale=1;
        isPaused = false;
    }
    // Pause game
    void pause()
    {
        pauseMenu.SetActive(true);
        HUD.SetActive(false);
        Time.timeScale=0;
        isPaused = true;
    }
    // Show controls menu
    public void showControls()
    {
        pauseMenu.SetActive(false);
        controls.SetActive(true);
    }
    // Hide controls menu
    public void hideControls()
    {
        pauseMenu.SetActive(true);
        controls.SetActive(false);
    }
    // Quit game
    public void quitGame()
    {
        Application.Quit();
    }
}