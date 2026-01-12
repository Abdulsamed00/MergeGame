using UnityEngine;
using System.IO;

public static class SaveManager
{
    // Dosyaların kaydedileceği ana klasör (Cihaza göre değişir)
    private static string BasePath => Application.persistentDataPath;

    // Her level için ayrı dosya ismi üretir: "save_level_0.json", "save_level_1.json"
    private static string GetPath(int levelIndex)
    {
        return Path.Combine(BasePath, $"save_level_{levelIndex}.json");
    }

    // --- KAYDETME ---
    public static void Save(SaveData data, int levelIndex)
    {
        string json = JsonUtility.ToJson(data, true); // Veriyi metne çevir
        File.WriteAllText(GetPath(levelIndex), json); // Dosyaya yaz
        Debug.Log($"Oyun kaydedildi: Level {levelIndex}");
    }

    // --- YÜKLEME ---
    public static SaveData Load(int levelIndex)
    {
        string path = GetPath(levelIndex);
        
        if (!File.Exists(path)) 
        {
            Debug.LogWarning($"Kayıt dosyası bulunamadı: {path}");
            return null; // Dosya yoksa null döner
        }

        string json = File.ReadAllText(path); // Dosyayı oku
        SaveData data = JsonUtility.FromJson<SaveData>(json); // Metni veriye çevir
        return data;
    }

    // --- SİLME (Oyun bitince veya yeniden başlatınca lazım) ---
    public static void DeleteSave(int levelIndex)
    {
        string path = GetPath(levelIndex);
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"Kayıt silindi: Level {levelIndex}");
        }
    }

    // --- KAYIT VAR MI KONTROLÜ ---
    public static bool HasSaveFile(int levelIndex)
    {
        return File.Exists(GetPath(levelIndex));
    }
}