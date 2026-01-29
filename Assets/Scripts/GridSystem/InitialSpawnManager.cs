using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitialSpawnManager : MonoBehaviour
{
    [Header("Grid")]
    public Grid grid;
    public GridManager gridManager;
    public PlacementManager placementManager;

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

        // --- YENİ MANTIK: Ayrıştırma ---
        List<ObjeVerisi> spawnlanacakObjeler = new List<ObjeVerisi>();
        
        List<LevelSpawnVerisi> tekSeferlikler = new List<LevelSpawnVerisi>(); // %100 ve üzeri
        List<LevelSpawnVerisi> standartlar = new List<LevelSpawnVerisi>();    // %100 altı

        foreach (var item in spawnDataListesi)
        {
            if (item.spawnYuzdesi >= 100) tekSeferlikler.Add(item);
            else standartlar.Add(item);
        }

        // 1. Önce "Zorunlu" (Unique) olanları 1'er tane ekle
        foreach (var item in tekSeferlikler)
        {
            if (spawnlanacakObjeler.Count < count)
            {
                spawnlanacakObjeler.Add(item.obje);
            }
        }

        // 2. Kalan boşlukları "Standart" listeden rastgele doldur
        // (Böylece %100 olanlar tekrar seçilmez)
        int kalanBosluk = count - spawnlanacakObjeler.Count;

        if (standartlar.Count > 0 && kalanBosluk > 0)
        {
            for (int i = 0; i < kalanBosluk; i++)
            {
                ObjeVerisi sansli = GetWeightedRandomFromList(standartlar);
                spawnlanacakObjeler.Add(sansli);
            }
        }

        // 3. Listeyi karıştır ve Spawn et
        Shuffle(spawnlanacakObjeler);
        Shuffle(emptyCells);

        for (int i = 0; i < spawnlanacakObjeler.Count; i++)
        {
            SpawnObjectToCell(spawnlanacakObjeler[i], emptyCells[i]);
        }
    }

    // Özel liste için yardımcı rastgele fonksiyonu
    ObjeVerisi GetWeightedRandomFromList(List<LevelSpawnVerisi> targetList)
    {
        float toplamSans = 0;
        foreach (var item in targetList) toplamSans += item.spawnYuzdesi;

        float rastgeleDeger = Random.Range(0, toplamSans);
        float suankiToplam = 0;

        foreach (var item in targetList)
        {
            suankiToplam += item.spawnYuzdesi;
            if (rastgeleDeger <= suankiToplam) 
            {
                return item.obje;
            }
        }
        return targetList[0].obje;
    }

    void SpawnObjectToCell(ObjeVerisi veri, GridCell cell)
    {
        if (veri == null || veri.objePrefab == null) return;

        float offset = 0.5f;
        var prefabComp = veri.objePrefab.GetComponent<PlaceableObject>();
        if (prefabComp != null) offset = prefabComp.heightOffset;

        Vector3 spawnPos = grid.GetCellCenterWorld(cell.cellPosition) + Vector3.up * offset;

        GameObject obj = Instantiate(veri.objePrefab, spawnPos, veri.objePrefab.transform.rotation);

        PlaceableObject po = obj.GetComponent<PlaceableObject>();
        po.verisi = veri;
        po.currentCell = cell;
        
        po.SetPreviewMode(false);
        po.PlaySpawnAnimation(); 

        cell.currentObject = po;
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