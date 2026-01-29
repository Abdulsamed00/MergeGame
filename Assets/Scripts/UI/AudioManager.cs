using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Audio Mixer")]
    public AudioMixer mixer;

    [Header("UI / Game SFX")]
    public AudioClip buttonClick;
    public AudioClip mergeSes;
    public AudioClip mergeErrorSesi;
    public AudioClip spawnSesi;
    public AudioClip kazanmaSesi;
    public AudioClip kaybetmeSesi;

    const string MUSIC_VOL = "MusicVolume";
    const string SFX_VOL   = "SfxVolume";

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 🔁 MUSIC LOOP
        musicSource.loop = true;
        musicSource.playOnAwake = false;

        sfxSource.loop = false;
        sfxSource.playOnAwake = false;

        // 🔥 Kaydedilmiş volume değerleri
        float musicVol = PlayerPrefs.GetFloat(MUSIC_VOL, 1f);
        float sfxVol   = PlayerPrefs.GetFloat(SFX_VOL, 1f);

        SetMixerVolume("MusicVol", musicVol);
        SetMixerVolume("SFXVol", sfxVol);
    }

    // ================= MUSIC =================
    public void PlayLevelMusic(AudioClip clip)
    {
        if (clip == null) return;

        if (musicSource.clip == clip && musicSource.isPlaying)
            return;

        musicSource.Stop();
        musicSource.clip = clip;
        musicSource.Play(); // 🔁 loop açık
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    // ================= SFX =================
    public void PlayButtonClick() => PlaySFX(buttonClick);
    public void PlayMerge()       => PlaySFX(mergeSes);
    public void PlayMergeError()  => PlaySFX(mergeErrorSesi);
    public void PlaySpawn()       => PlaySFX(spawnSesi);
    public void PlayWin()         => PlaySFX(kazanmaSesi);
    public void PlayLose()        => PlaySFX(kaybetmeSesi);

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip);
    }

    // ================= VOLUME =================
    public void SetMusicVolume(float value)
    {
        PlayerPrefs.SetFloat(MUSIC_VOL, value);
        SetMixerVolume("MusicVol", value);
    }

    public void SetSFXVolume(float value)
    {
        PlayerPrefs.SetFloat(SFX_VOL, value);
        SetMixerVolume("SFXVol", value);
    }

    void SetMixerVolume(string param, float value)
    {
        mixer.SetFloat(
            param,
            Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20f
        );
    }
}
