using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class AudioSettingsUI : MonoBehaviour
{
    public AudioMixer mixer;

    public Slider musicSlider;
    public Slider sfxSlider;

    const string MUSIC_VOL = "MusicVolume";
    const string SFX_VOL   = "SfxVolume";

    void Start()
    {
        float musicVol = PlayerPrefs.GetFloat(MUSIC_VOL, 1f);
        float sfxVol   = PlayerPrefs.GetFloat(SFX_VOL, 1f);

      
        musicSlider.SetValueWithoutNotify(musicVol);
        sfxSlider.SetValueWithoutNotify(sfxVol);

        musicSlider.onValueChanged.AddListener(ApplyMusic);
        sfxSlider.onValueChanged.AddListener(ApplySFX);
    }

    void ApplyMusic(float value)
    {
        PlayerPrefs.SetFloat(MUSIC_VOL, value);
        mixer.SetFloat(
            "MusicVol",
            Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20
        );
    }

    void ApplySFX(float value)
    {
        PlayerPrefs.SetFloat(SFX_VOL, value);
        mixer.SetFloat(
            "SFXVol",
            Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20
        );
    }
}