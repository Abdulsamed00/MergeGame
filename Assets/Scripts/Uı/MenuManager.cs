using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;
    public GameObject levelsPanel;

    [Header("Fade")]
    public Image fadeImage;
    public float fadeDuration = 0.5f;

    void Start()
    {
        Time.timeScale = 1f;

        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
        levelsPanel.SetActive(false);

        StartCoroutine(FadeIn());
    }

    // ======================
    // MAIN MENU
    // ======================

    public void OpenLevels()
    {
        StartCoroutine(SwitchPanel(mainMenuPanel, levelsPanel));
    }

    public void PlayGame()
    {
        StartCoroutine(FadeToScene("SampleScene"));
    }

    public void OpenSettings()
    {
        StartCoroutine(SwitchPanel(mainMenuPanel, settingsPanel));
    }

    public void CloseSettings()
    {
        StartCoroutine(SwitchPanel(settingsPanel, mainMenuPanel));
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    // ======================
    // LEVELS
    // ======================

    public void CloseLevels()
    {
        StartCoroutine(SwitchPanel(levelsPanel, mainMenuPanel));
    }

    // ======================
    // PANEL & FADE
    // ======================

    IEnumerator SwitchPanel(GameObject close, GameObject open)
    {
        yield return FadeOut();
        close.SetActive(false);
        open.SetActive(true);
        yield return FadeIn();
    }

    IEnumerator FadeToScene(string sceneName)
    {
        yield return FadeOut();
        SceneManager.LoadScene(sceneName);
    }

    IEnumerator FadeIn()
    {
        if (fadeImage == null) yield break;

        float t = fadeDuration;
        while (t > 0)
        {
            t -= Time.deltaTime;
            fadeImage.color = new Color(0, 0, 0, t / fadeDuration);
            yield return null;
        }
    }

    IEnumerator FadeOut()
    {
        if (fadeImage == null) yield break;

        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeImage.color = new Color(0, 0, 0, t / fadeDuration);
            yield return null;
        }
    }

    // ======================
    // LANGUAGE
    // ======================

    public void SetLanguageTurkish()
    {
        LanguageData.CurrentLanguage = Language.Turkish;
        RefreshAllTexts();
    }

    public void SetLanguageEnglish()
    {
        LanguageData.CurrentLanguage = Language.English;
        RefreshAllTexts();
    }

    void RefreshAllTexts()
    {
        SimpleLanguageText[] texts =
            FindObjectsOfType<SimpleLanguageText>(true);

        foreach (var t in texts)
        {
            t.Refresh();
        }
    }
}
