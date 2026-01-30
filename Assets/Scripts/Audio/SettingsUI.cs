using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    [Header("UI Referansları")]
    public Slider musicSlider;
    public Slider sfxSlider;

    private void Start()
    {
        float savedMusicVol = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float savedSFXVol = PlayerPrefs.GetFloat("SFXVolume", 1f);

        musicSlider.value = savedMusicVol;
        sfxSlider.value = savedSFXVol;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(savedMusicVol);
            AudioManager.Instance.SetSFXVolume(savedSFXVol);
        }

        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    public void SetMusicVolume(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(value);
        }

        PlayerPrefs.SetFloat("MusicVolume", value);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(value);
        }

        PlayerPrefs.SetFloat("SFXVolume", value);
        PlayerPrefs.Save();
    }
}