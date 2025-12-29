using UnityEngine;
using System.Collections;

public class CountryLevelsUI : MonoBehaviour
{
    public GameObject levelsPanel;

    [Header("Country Level Panels")]
    public GameObject turkeyLevelsPanel;
    public GameObject japanLevelsPanel;
    public GameObject brazilLevelsPanel;
    public GameObject egyptLevelsPanel;

    public MenuManager menuManager;

    GameObject currentCountryPanel;

    // ======================
    // COUNTRY BUTTONS
    // ======================

    public void OpenTurkey()
    {
        OpenCountry(turkeyLevelsPanel);
    }

    public void OpenJapan()
    {
        OpenCountry(japanLevelsPanel);
    }

    public void OpenBrazil()
    {
        OpenCountry(brazilLevelsPanel);
    }

    public void OpenEgypt()
    {
        OpenCountry(egyptLevelsPanel);
    }

    void OpenCountry(GameObject countryPanel)
    {
        currentCountryPanel = countryPanel;
        StartCoroutine(OpenCountryRoutine());
    }

    IEnumerator OpenCountryRoutine()
    {
        yield return menuManager.StartCoroutine("FadeOut");

        levelsPanel.SetActive(false);

        turkeyLevelsPanel.SetActive(false);
        japanLevelsPanel.SetActive(false);
        brazilLevelsPanel.SetActive(false);
        egyptLevelsPanel.SetActive(false);

        currentCountryPanel.SetActive(true);

        yield return menuManager.StartCoroutine("FadeIn");
    }

    // ======================
    // BACK
    // ======================

    public void BackToLevels()
    {
        StartCoroutine(BackRoutine());
    }

    IEnumerator BackRoutine()
    {
        yield return menuManager.StartCoroutine("FadeOut");

        if (currentCountryPanel != null)
            currentCountryPanel.SetActive(false);

        levelsPanel.SetActive(true);

        yield return menuManager.StartCoroutine("FadeIn");
    }
}