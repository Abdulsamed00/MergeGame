using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    // ================= MUSIC =================
    [Header("Region Musics")]
    public AudioClip turkeySes;
    public AudioClip japaneseMapSong;
    public AudioClip brazilyaSes;
    public AudioClip egyptSes;

    // ================= SFX =================
    [Header("UI / Game Sounds")]
    public AudioClip buttonClick;
    public AudioClip mergeSes;
    public AudioClip mergeErrorSesi;
    public AudioClip spawnSesi;
    public AudioClip kazanmaSesi;
    public AudioClip kaybetmeSesi;
    public AudioClip openLevelStageSound;
    public AudioClip yapiBitirmeSesi;
    public AudioClip yapiYikilma;
    public AudioClip demirMetalObjeBirlestirme;
    public AudioClip odunAgacBirlestirme;

    const string MUSIC_KEY = "MusicOn";
    const string SFX_KEY = "SfxOn";

    bool musicOn;
    bool sfxOn;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        musicOn = PlayerPrefs.GetInt(MUSIC_KEY, 1) == 1;
        sfxOn   = PlayerPrefs.GetInt(SFX_KEY, 1) == 1;

        ApplySettings();
    }

    // ================= MUSIC =================

    public void PlayRegionMusic(string region)
    {
        AudioClip clip = null;

        switch (region)
        {
            case "Turkey":
                clip = turkeySes;
                break;
            case "Japan":
                clip = japaneseMapSong;
                break;
            case "Brazil":
                clip = brazilyaSes;
                break;
            case "Egypt":
                clip = egyptSes;
                break;
        }

        if (clip == null) return;
        if (musicSource.clip == clip) return;

        musicSource.clip = clip;

        if (musicOn)
            musicSource.Play();
    }

    // ================= SFX =================

    public void PlayButtonClick()
    {
        PlaySFX(buttonClick);
    }

    public void PlayMerge()
    {
        PlaySFX(mergeSes);
    }

    public void PlayMergeError()
    {
        PlaySFX(mergeErrorSesi);
    }

    public void PlaySpawn()
    {
        PlaySFX(spawnSesi);
    }

    public void PlayWin()
    {
        PlaySFX(kazanmaSesi);
    }

    public void PlayLose()
    {
        PlaySFX(kaybetmeSesi);
    }

    public void PlayOpenLevel()
    {
        PlaySFX(openLevelStageSound);
    }

    public void PlayYapiBitirme()
    {
        PlaySFX(yapiBitirmeSesi);
    }

    public void PlayYapiYikilma()
    {
        PlaySFX(yapiYikilma);
    }

    public void PlayMetalMerge()
    {
        PlaySFX(demirMetalObjeBirlestirme);
    }

    public void PlayWoodMerge()
    {
        PlaySFX(odunAgacBirlestirme);
    }

    void PlaySFX(AudioClip clip)
    {
        if (!sfxOn || clip == null) return;
        sfxSource.PlayOneShot(clip);
    }

    // ================= SETTINGS =================

    public void ToggleMusic(bool on)
    {
        musicOn = on;
        PlayerPrefs.SetInt(MUSIC_KEY, on ? 1 : 0);

        if (on) musicSource.Play();
        else musicSource.Stop();
    }

    public void ToggleSFX(bool on)
    {
        sfxOn = on;
        PlayerPrefs.SetInt(SFX_KEY, on ? 1 : 0);
    }

    void ApplySettings()
    {
        if (!musicOn)
            musicSource.Stop();
    }
}
