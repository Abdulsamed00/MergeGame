using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    public int levelIndex;
    public int currentPopulation;
    public int undoRights;
    public List<GridObjectData> placedObjects = new List<GridObjectData>();
    public string currentSpawnObjectID;
    public string nextSpawnObjectID;
}

[System.Serializable]
public class GridObjectData
{
    public string objectID;
    public int x;
    public int z;
    public bool isLocked;
    public int movementRights;
    public List<string> stackedItemIDs = new List<string>();
}