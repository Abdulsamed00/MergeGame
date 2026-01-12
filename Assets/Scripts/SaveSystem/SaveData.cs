using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    // --- Genel Veriler ---
    public int levelIndex;
    public int currentPopulation;
    
    // --- Spawn Sırası (Stratejinin bozulmaması için) ---
    public string currentSpawnObjectID; // siradakiObjeVerisi
    public string nextSpawnObjectID;    // sonrakiObjeVerisi
    
    // --- Griddeki Objeler ---
    public List<GridObjectData> placedObjects = new List<GridObjectData>();
}

[System.Serializable]
public class GridObjectData
{
    // Temel Veriler
    public string objectID;
    public int x;
    public int z;
    
    // Durum Verileri
    public int movementRights; // hareketHakki
    public bool isLocked;      // kilitliMi
    
    // Stack Verisi (İçindeki Malzemeler)
    // Sadece ID'leri tutacağız
    public List<string> stackedItemIDs = new List<string>();
}