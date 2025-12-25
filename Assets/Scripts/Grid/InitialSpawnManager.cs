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

    void Start()
    {
        StartCoroutine(SpawnRoutine());
        
    }

    IEnumerator SpawnRoutine()
    {
        // Grid oluşana kadar bekle
        yield return null; // 1 frame bekle
        yield return null; // garanti olsun diye 1 frame daha

        SpawnInitialObjects();
        placementManager.BeginPlacementAfterInitialSpawn();
    }

    void SpawnInitialObjects()
    {
        List<GridCell> emptyCells = new List<GridCell>(gridManager.GetAllCells());

        if (emptyCells.Count < baslangicObjeleri.Count)
        {
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

        GameObject obj = Instantiate(veri.objePrefab, spawnPos, Quaternion.identity);

        PlaceableObject po = obj.GetComponent<PlaceableObject>();
        po.verisi = veri;
        po.currentCell = cell;
        po.SetPreviewMode(false);

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
