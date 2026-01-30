using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Ayarlar")]
    public AudioMixer mainMixer;

    [Header("Kaynaklar")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Genel Sesler")]
    public AudioClip winSound;
    public AudioClip loseSound;
    public AudioClip clickSound;
    public AudioClip placeSound;
    public AudioClip mergeSound;
    public AudioClip undoSound;

    [Header("Sahne Müzikleri")]
    public AudioClip uiMusic;
    public AudioClip creativeMusic;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void PlayMusicClip(AudioClip clip)
    {
        if (clip == null) return;

        if (musicSource.clip == clip && musicSource.isPlaying) return;
        
        musicSource.clip = clip;
        musicSource.Play();
    }

    public void PlayUIMusic() => PlayMusicClip(uiMusic);
    public void PlayCreativeMusic() => PlayMusicClip(creativeMusic);

    public void PlayCountryMusic(CountryData country)
    {
        if (country == null || country.ulkeMuzigi == null) return;

        if (musicSource.clip == country.ulkeMuzigi && musicSource.isPlaying) return;

        musicSource.clip = country.ulkeMuzigi;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null) sfxSource.PlayOneShot(clip);
    }

    public void PlayWin() => PlaySFX(winSound);
    public void PlayLose() => PlaySFX(loseSound);
    public void PlayClick() => PlaySFX(clickSound);
    public void PlayPlace() => PlaySFX(placeSound);
    public void PlayMerge() => PlaySFX(mergeSound);
    public void PlayUndo() => PlaySFX(undoSound);

    public void SetMusicVolume(float sliderValue)
    {
        float db = Mathf.Log10(Mathf.Clamp(sliderValue, 0.0001f, 1f)) * 20;
        mainMixer.SetFloat("MusicVol", db); 
    }

    public void SetSFXVolume(float sliderValue)
    {
        float db = Mathf.Log10(Mathf.Clamp(sliderValue, 0.0001f, 1f)) * 20;
        mainMixer.SetFloat("SFXVol", db);
    }
}