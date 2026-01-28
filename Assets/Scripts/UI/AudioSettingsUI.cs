using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSettingsUI : MonoBehaviour
{
    [Header("Mixer")]
    public AudioMixer audioMixer;

    [Header("Sliders")]
    public Slider musicSlider;
    public Slider sfxSlider;

    const string MUSIC_VOL = "MusicVolume";
    const string SFX_VOL   = "SFXVolume";

    void Start()
    {
        // Kaydedilmiş değerleri yükle
        float musicValue = PlayerPrefs.GetFloat(MUSIC_VOL, 1f);
        float sfxValue   = PlayerPrefs.GetFloat(SFX_VOL, 1f);

        musicSlider.value = musicValue;
        sfxSlider.value   = sfxValue;

        SetMusicVolume(musicValue);
        SetSFXVolume(sfxValue);

        // Slider event bağla
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    public void SetMusicVolume(float value)
    {
        value = Mathf.Clamp(value, 0.0001f, 1f);
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(value) * 20f);
        PlayerPrefs.SetFloat(MUSIC_VOL, value);
    }

    public void SetSFXVolume(float value)
    {
        value = Mathf.Clamp(value, 0.0001f, 1f);
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(value) * 20f);
        PlayerPrefs.SetFloat(SFX_VOL, value);
    }
}