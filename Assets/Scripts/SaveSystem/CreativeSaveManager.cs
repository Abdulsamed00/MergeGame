using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class CreativeSaveManager : MonoBehaviour
{
    public static CreativeSaveManager Instance;

    [Header("Referanslar")]
    public CreativeGridManager gridManager;
    public CollectionDatabase tumObjelerDB;

    private string saveFileName = "creative_save.json";

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        LoadCreativeMap();
    }

    private void OnApplicationQuit()
    {
        SaveCreativeMap();
    }

    public void SaveCreativeMap()
    {
        Debug.Log("--- KAYIT BAŞLADI ---");
        SaveData data = new SaveData();
        data.levelIndex = -1;

        List<CreativeGridCell> doluHucreler = gridManager.GetAllOccupiedCells();
        Debug.Log("Bulunan Dolu Hücre Sayısı: " + doluHucreler.Count);

        foreach (var cell in doluHucreler)
        {
            if (cell.storedData == null) 
            {
                Debug.LogError("HATA: Hücre dolu ama 'storedData' (Obje Verisi) BOŞ! PlacementManager kodunu kontrol et.");
                continue;
            }

            Debug.Log("Kaydedilen Obje: " + cell.storedData.saveID + " Konum: " + cell.CellPosition);

            GridObjectData objData = new GridObjectData();
            objData.objectID = cell.storedData.saveID;
            objData.x = cell.CellPosition.x;
            objData.z = cell.CellPosition.z;

            data.placedObjects.Add(objData);
        }

        string json = JsonUtility.ToJson(data, true);
        string path = Path.Combine(Application.persistentDataPath, saveFileName);
        File.WriteAllText(path, json);
    
        Debug.Log("JSON Dosyası Yazıldı: " + path);
        Debug.Log("--- KAYIT BİTTİ ---");
    }
    
    private void OnDisable()
    {
        SaveCreativeMap();
    }

    public void LoadCreativeMap()
    {
        string path = Path.Combine(Application.persistentDataPath, saveFileName);

        if (!File.Exists(path))
        {
            return;
        }

        string json = File.ReadAllText(path);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        gridManager.ClearAllGrid();

        Debug.Log("Yüklenecek Obje Sayısı: " + data.placedObjects.Count);

        foreach (var objData in data.placedObjects)
        {
            ObjeVerisi veri = FindObjectByID(objData.objectID);

            if (veri != null)
            {
                Vector3Int pos = new Vector3Int(objData.x, 0, objData.z);
                
                if(gridManager.GetCell(pos) == null) 
                {
                    Debug.LogError("HATA: Grid hücresi bulunamadı! Grid oluşmamış olabilir. Pos: " + pos);
                    continue;
                }

                GameObject go = Instantiate(veri.objePrefab);
                
                bool basarili = gridManager.PlaceObject(pos, go, veri);

                if (basarili)
                {
                    Debug.Log("Başarıyla Yerleştirildi: " + veri.objeAdi);
                }
                else
                {
                    Debug.LogError("Yerleştirme Başarısız! Hücre dolu olabilir.");
                }
            }
            else
            {
                Debug.LogWarning("Veritabanında bulunamayan ID: " + objData.objectID);
            }
        }
    }

    private ObjeVerisi FindObjectByID(string id)
    {
        foreach (var obje in tumObjelerDB.tumObjelerSorted)
        {
            if (obje.saveID == id)
            {
                return obje;
            }
        }
        Debug.LogWarning("Bulunamayan ID: " + id);
        return null;
    }
}