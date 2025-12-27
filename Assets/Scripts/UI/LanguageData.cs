public enum Language
{
    Turkish,
    English
}

public static class LanguageData
{
    public static Language CurrentLanguage
    {
        get
        {
            return (Language)UnityEngine.PlayerPrefs.GetInt("Language", 1);
        }
        set
        {
            UnityEngine.PlayerPrefs.SetInt("Language", (int)value);
        }
    }
}