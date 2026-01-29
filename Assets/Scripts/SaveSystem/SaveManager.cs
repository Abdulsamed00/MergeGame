using UnityEngine;
using System.IO;

public static class SaveManager
{
    private static string BasePath => Application.persistentDataPath;

    private static string GetPath(int levelIndex)
    {
        return Path.Combine(BasePath, $"save_level_{levelIndex}.json");
    }

    public static void Save(SaveData data, int levelIndex)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetPath(levelIndex), json);
        Debug.Log($"Oyun kaydedildi: Level {levelIndex}");
    }

    // --- YÜKLEME ---
    public static SaveData Load(int levelIndex)
    {
        string path = GetPath(levelIndex);
        
        if (!File.Exists(path)) 
        {
            Debug.LogWarning($"Kayıt dosyası bulunamadı: {path}");
            return null;
        }

        string json = File.ReadAllText(path);
        SaveData data = JsonUtility.FromJson<SaveData>(json);
        return data;
    }

    public static void DeleteSave(int levelIndex)
    {
        string path = GetPath(levelIndex);
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"Kayıt silindi: Level {levelIndex}");
        }
    }

    public static bool HasSaveFile(int levelIndex)
    {
        return File.Exists(GetPath(levelIndex));
    }
}