using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    // Hangi levelin kaydı?
    public int levelIndex;
    
    // O anki toplam nüfus (GameManager'dan gelecek)
    public int currentPopulation;
    
    public int undoRights; // Kalan geri alma hakkı

    // --- GRID ÜZERİNDEKİ OBJELER ---
    // Hangi hücrede hangi obje var?
    public List<GridObjectData> placedObjects = new List<GridObjectData>();

    // --- SIRADAKİ OBJELER (PlacementManager'dan gelecek) ---
    // Bunları şimdiden hazırlıyoruz, 3. adımda dolduracağız.
    public string currentSpawnObjectID;
    public string nextSpawnObjectID;
}

[System.Serializable]
public class GridObjectData
{
    // Objenin Tipi (ScriptableObject saveID'si)
    public string objectID;
    
    // Konumu (GridCell.cellPosition'dan)
    public int x;
    public int z;

    // --- PLACEABLE OBJECT ÖZELLİKLERİ (2. Adımda detaylanacak) ---
    public bool isLocked;       // Kilitli mi?
    public int movementRights;  // Hareket hakkı
    
    // Stack (Merge) sistemi için içindeki malzemeler
    public List<string> stackedItemIDs = new List<string>();
}