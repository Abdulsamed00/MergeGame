using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource; // Mixer: Musics
    public AudioSource sfxSource;   // Mixer: SFX

    // ================= MUSIC (LOOP) =================
    [Header("Region Musics (Loop)")]
    public AudioClip turkeySes;
    public AudioClip brazilyaSes;
    public AudioClip japaneseMapSong;
    public AudioClip egyptSes;

    // ================= SFX (ONE SHOT) =================
    [Header("UI / Game SFX")]
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
    const string SFX_KEY   = "SfxOn";

    bool musicOn;
    bool sfxOn;

    // ================= INIT =================
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

        // SFX Source
        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
        sfxSource.spatialBlend = 0f;

        // Music Source (🔁 LOOP GARANTİ)
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;

        ApplySettings();
    }

    // ================= MUSIC PLAY =================
    public void PlayTurkeyMusic()   => PlayMusicLoop(turkeySes);
    public void PlayBrazilMusic()   => PlayMusicLoop(brazilyaSes);
    public void PlayJapanMusic()    => PlayMusicLoop(japaneseMapSong);
    public void PlayEgyptMusic()    => PlayMusicLoop(egyptSes);

    void PlayMusicLoop(AudioClip clip)
    {
        if (!musicOn || clip == null) return;

        musicSource.Stop();
        musicSource.clip = clip;
        musicSource.loop = true;   // 🔁 burada da garanti
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    // ================= SFX =================
    public void PlayButtonClick()   => PlaySFX(buttonClick);
    public void PlayMerge()         => PlaySFX(mergeSes);
    public void PlayMergeError()    => PlaySFX(mergeErrorSesi);
    public void PlaySpawn()         => PlaySFX(spawnSesi);
    public void PlayWin()           => PlaySFX(kazanmaSesi);
    public void PlayLose()          => PlaySFX(kaybetmeSesi);
    public void PlayOpenLevel()     => PlaySFX(openLevelStageSound);
    public void PlayYapiBitirme()   => PlaySFX(yapiBitirmeSesi);
    public void PlayYapiYikilma()   => PlaySFX(yapiYikilma);
    public void PlayMetalMerge()    => PlaySFX(demirMetalObjeBirlestirme);
    public void PlayWoodMerge()     => PlaySFX(odunAgacBirlestirme);

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

        if (on && musicSource.clip != null)
            musicSource.Play();
        else
            musicSource.Stop();
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
