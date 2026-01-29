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

    // Hangi objelerin kilitleneceğini tutan liste
    private HashSet<ObjeVerisi> kilitlenecekObjeler = new HashSet<ObjeVerisi>();

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

        kilitlenecekObjeler.Clear(); // Listeyi temizle

        List<GridCell> emptyCells = new List<GridCell>(gridManager.GetAllCells());
        int count = Mathf.Min(spawnAdedi, emptyCells.Count);

        List<ObjeVerisi> spawnlanacakObjeler = new List<ObjeVerisi>();
        
        List<LevelSpawnVerisi> tekSeferlikler = new List<LevelSpawnVerisi>(); // %100 ve üzeri
        List<LevelSpawnVerisi> standartlar = new List<LevelSpawnVerisi>();    // %100 altı

        foreach (var item in spawnDataListesi)
        {
            if (item.spawnYuzdesi >= 100) 
            {
                tekSeferlikler.Add(item);
                // %100 olan bu objeyi "Kilitliler Listesi"ne ekle
                kilitlenecekObjeler.Add(item.obje);
            }
            else 
            {
                standartlar.Add(item);
            }
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
        
        // Temizlik (Önceki sorundan emin olmak için)
        if (po.icindekiMalzemeler == null) po.icindekiMalzemeler = new List<ObjeVerisi>();
        po.icindekiMalzemeler.Clear();
        po.icindekiMalzemeler.Add(veri);

        // --- HAREKET KİLİDİ (YENİ KISIM) ---
        // Eğer bu obje %100 listesindeyse, hareket hakkını 0 yap.
        if (kilitlenecekObjeler.Contains(veri))
        {
            po.hareketHakki = 0;
            po.kilitliMi = true; // Görsel olarak kilit simgesi varsa açar
        }
        else
        {
            po.hareketHakki = 1; // Diğerleri hareket edebilir
            po.kilitliMi = false;
        }
        // -----------------------------------

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