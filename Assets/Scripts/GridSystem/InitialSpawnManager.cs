using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitialSpawnManager : MonoBehaviour
{
    [Header("Grid")]
    public Grid grid;
    public GridManager gridManager;
    public PlacementManager placementManager;

    // Şans oranlarını hesaplamak için gereken liste
    private List<LevelSpawnVerisi> spawnDataListesi;
    private int spawnAdedi;

    public void SpawnBaslat(List<LevelSpawnVerisi> levelObjeleri, int adet)
    {
        spawnDataListesi = levelObjeleri;
        spawnAdedi = adet;

        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        yield return null; 

        SpawnInitialObjects();
        
        placementManager.BeginPlacementAfterInitialSpawn();
    }

    void SpawnInitialObjects()
    {
        if (spawnDataListesi == null || spawnDataListesi.Count == 0 || spawnAdedi <= 0) 
        {
            return;
        }

        List<GridCell> emptyCells = new List<GridCell>(gridManager.GetAllCells());

        int count = Mathf.Min(spawnAdedi, emptyCells.Count);

        Shuffle(emptyCells);

        for (int i = 0; i < count; i++)
        {
            ObjeVerisi secilenObje = GetWeightedRandomObject();
            SpawnObjectToCell(secilenObje, emptyCells[i]);
        }
    }

    ObjeVerisi GetWeightedRandomObject()
    {
        float toplamSans = 0;
        foreach (var item in spawnDataListesi) toplamSans += item.spawnYuzdesi;

        float rastgeleDeger = Random.Range(0, toplamSans);
        float suankiToplam = 0;

        foreach (var item in spawnDataListesi)
        {
            suankiToplam += item.spawnYuzdesi;
            if (rastgeleDeger <= suankiToplam) 
            {
                return item.obje;
            }
        }
        return spawnDataListesi[0].obje;
    }

    void SpawnObjectToCell(ObjeVerisi veri, GridCell cell)
    {
        if (veri == null || veri.objePrefab == null) return;

        float offset = 0.5f;
        var prefabComp = veri.objePrefab.GetComponent<PlaceableObject>();
        if (prefabComp != null) offset = prefabComp.heightOffset;

        Vector3 spawnPos = grid.GetCellCenterWorld(cell.cellPosition) + Vector3.up * offset;

        // --- DÜZELTME BURADA ---
        // Quaternion.identity yerine "veri.objePrefab.transform.rotation" kullandık.
        // Böylece prefabın Inspector'daki rotasyonu neyse aynen o şekilde doğar.
        GameObject obj = Instantiate(veri.objePrefab, spawnPos, veri.objePrefab.transform.rotation);
        // -----------------------

        PlaceableObject po = obj.GetComponent<PlaceableObject>();
        po.verisi = veri;
        po.currentCell = cell;
        
        po.SetPreviewMode(false);
        po.PlaySpawnAnimation(); 

        cell.currentObject = po;
        
        // --- İSTEĞE BAĞLI DÜZELTME ---
        // Eğer başlangıç objelerinin de puana/nüfusa etki etmesini istiyorsan
        // aşağıdaki yorum satırını açabilirsin:
        
        // if (GameManager.Instance != null)
        //    GameManager.Instance.UretimYapildi(po.verisi, spawnPos);
    }

    void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rnd = Random.Range(i, list.Count);
            (list[i], list[rnd]) = (list[rnd], list[i]); 
        }
    }
}