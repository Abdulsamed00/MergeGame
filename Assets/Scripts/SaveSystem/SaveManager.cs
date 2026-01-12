using UnityEngine;
using System.IO;

public static class SaveManager
{
    private static string GetPath(int levelIndex)
    {
        // PersistentDataPath mobil ve PC'de güvenli yazılabilir klasördür
        return Path.Combine(Application.persistentDataPath, $"save_level_{levelIndex}.json");
    }

    public static void Save(SaveData data, int levelIndex)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetPath(levelIndex), json);
        Debug.Log($"Oyun Kaydedildi: Level {levelIndex}");
    }

    public static SaveData Load(int levelIndex)
    {
        string path = GetPath(levelIndex);
        if (File.Exists(path))
        {
            try
            {
                string json = File.ReadAllText(path);
                return JsonUtility.FromJson<SaveData>(json);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Kayıt dosyası bozuk: " + e.Message);
                return null; // Dosya bozuksa null döner
            }
        }
        return null;
    }

    public static void DeleteSave(int levelIndex)
    {
        string path = GetPath(levelIndex);
        if (File.Exists(path)) File.Delete(path);
        Debug.Log($"Kayıt Silindi: Level {levelIndex}");
    }

    public static bool HasSaveFile(int levelIndex)
    {
        return File.Exists(GetPath(levelIndex));
    }
}