using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitialSpawnManager : MonoBehaviour
{
    [Header("Grid")]
    public Grid grid;
    public GridManager gridManager;
    public PlacementManager placementManager;

    [Header("Spawn Listesi")]
    public List<ObjeVerisi> baslangicObjeleri;

    // --- DEĞİŞİKLİK: Start fonksiyonunu SİLDİK ---
    // Artık GameManager çağıracak.

    public void SpawnBaslat()
    {
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        // Grid zaten hazır olduğu için beklemeye gerek yok ama
        // güvenli taraf için 1 frame bekleyebiliriz.
        yield return null; 

        SpawnInitialObjects();
        
        // Başlangıç objeleri konduktan sonra oyuncunun sırasını başlat
        placementManager.BeginPlacementAfterInitialSpawn();
    }

    void SpawnInitialObjects()
    {
        List<GridCell> emptyCells = new List<GridCell>(gridManager.GetAllCells());

        if (emptyCells.Count < baslangicObjeleri.Count)
        {
            // Eğer koyacak obje yoksa veya yer yoksa bile oyunu başlatmalıyız!
            if (baslangicObjeleri.Count == 0) return;
            Debug.LogWarning("Yeterli boş hücre yok!");
            return;
        }

        Shuffle(emptyCells);

        for (int i = 0; i < baslangicObjeleri.Count; i++)
        {
            SpawnObjectToCell(baslangicObjeleri[i], emptyCells[i]);
        }
    }

    void SpawnObjectToCell(ObjeVerisi veri, GridCell cell)
    {
        Vector3 spawnPos =
            grid.GetCellCenterWorld(cell.cellPosition) +
            Vector3.up * veri.objePrefab.GetComponent<PlaceableObject>().heightOffset;

        GameObject obj = Instantiate(veri.objePrefab, spawnPos, veri.objePrefab.transform.rotation);

        PlaceableObject po = obj.GetComponent<PlaceableObject>();
        po.verisi = veri;
        po.currentCell = cell;
        po.SetPreviewMode(false);
        // Burada objeye de küçük bir "Pop" animasyonu ekleyebiliriz (Spawn trigger)
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