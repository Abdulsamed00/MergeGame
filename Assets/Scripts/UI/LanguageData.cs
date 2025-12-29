public enum Language
{
    Turkish = 0,
    English = 1
}

public static class LanguageData
{
    public static Language CurrentLanguage
    {
        get
        {
            // 🔹 İlk açılışta Türkçe
            return (Language)UnityEngine.PlayerPrefs.GetInt("Language", 0);
        }
        set
        {
            UnityEngine.PlayerPrefs.SetInt("Language", (int)value);
            UnityEngine.PlayerPrefs.Save();
        }
    }
}