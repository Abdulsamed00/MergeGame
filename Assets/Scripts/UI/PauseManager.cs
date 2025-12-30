using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public GameObject pausePanel;

    private bool isPaused = false;

    void Start()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }



    public void PauseGame()
    {
        if (isPaused) return;

        isPaused = true;
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        isPaused = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

   

    public void ExitToMenu()
    {
        isPaused = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1f;

        SceneManager.LoadScene("UiScence");
    }
}