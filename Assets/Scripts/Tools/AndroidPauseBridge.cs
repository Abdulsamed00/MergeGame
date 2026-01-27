using UnityEngine;

public class AndroidPauseBridge : MonoBehaviour
{
    private PauseManager pauseManager;

    void Start()
    {
        pauseManager = FindObjectOfType<PauseManager>();
    }

    void OnApplicationPause(bool isPaused)
    {
        if (isPaused)
        {
            if (pauseManager != null)
                pauseManager.PauseGame();
        }
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            if (pauseManager != null)
                pauseManager.PauseGame();
        }
    }
}