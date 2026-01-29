using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;
    public GameObject levelsPanel;
    public GameObject BasarimPanel;

    [Header("Fade")]
    public Image fadeImage;
    public float fadeDuration = 0.5f;

    public GameObject BilgiPaneli, TR, BR, JP, MSR, TarifP;
    public Button BilgiPaneliB, TrbButton, BrButton, JpButton, MsrButton, TarifButton, TarifGeriButton;
    void Start()
    {
        Time.timeScale = 1f;

        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
        levelsPanel.SetActive(false);
        TR.SetActive(false);
        BR.SetActive(false);
        JP.SetActive(false);
        MSR.SetActive(false);
        
        if (fadeImage != null)
        {
            fadeImage.raycastTarget = false;
            StartCoroutine(FadeIn());
        }
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
        StartCoroutine(FadeToScene(Scenes.SampleScene));
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
        Debug.Log("Oyun kapatıldı");
    }
    
    public void CreativeScene()
    {
        SceneManager.LoadScene("CreativeMode");
    }

    public void BasarimPanelAc()
    {
        mainMenuPanel.SetActive(false);
        BasarimPanel.SetActive(true);
    }
    
    public void BasarimPanelKapat()
    {
        BasarimPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }
    
    public void BilgiPaneliAc()
    {
        mainMenuPanel.SetActive(false);
        BilgiPaneli.SetActive(true);
    }

    public void BilgiPaneliKapat()
    {
        BilgiPaneli.SetActive(false);
        mainMenuPanel.SetActive(true);
    }
    
    public void TRAC()
    {
        BilgiPaneli.SetActive(false);
        TR.SetActive(true);
    }
    public void BRAC()
    {
        BilgiPaneli.SetActive(false);
        BR.SetActive(true);
    }
    public void JPAC()
    {
        BilgiPaneli.SetActive(false);
        JP.SetActive(true);
    }
    public void MSRAC()
    {
        BilgiPaneli.SetActive(false);
        MSR.SetActive(true);
    }
    
    public void TRKAPA()
    {
        TR.SetActive(false);
        BilgiPaneli.SetActive(true);
    }
    public void BRKAPA()
    {
        BR.SetActive(false);
        BilgiPaneli.SetActive(true);
    }
    public void JPKAPA()
    {
        JP.SetActive(false);
        BilgiPaneli.SetActive(true);
    }
    public void MSRKAPA()
    {
        MSR.SetActive(false);
        BilgiPaneli.SetActive(true);
    }

    public void TarifPAcma()
    {
        BilgiPaneli.SetActive(false);
        TarifP.SetActive(true);
    }
    
    public void TarifPKapama()
    {
        BilgiPaneli.SetActive(true);
        TarifP.SetActive(false);
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

    IEnumerator FadeToScene(Scenes scene)
    {
        yield return FadeOut();
        SceneManager.LoadScene(scene.ToString());
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
        SimpleLanguageText[] texts = FindObjectsOfType<SimpleLanguageText>(true);
        foreach (var t in texts)
        {
            t.Refresh();
        }
    }
}



public enum Scenes
{
    UiScene,
    SampleScene
}
