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
    public List<ObjeVerisi> baslangicObjeleri; //Başlangıçta oluşacak objelerin listesi

    void Start()
    {
        StartCoroutine(SpawnRoutine());
        
    }

    IEnumerator SpawnRoutine()
    {
        //Grid oluşana kadar frame bekliyor
        yield return null; //1 frame bekle
        yield return null; //Garanti olsun diye 1 frame daha

        SpawnInitialObjects();
        //Başlangıç objeleri konduktan sonra oyuncunun sırasını başlat
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

        //Hücreleri karıştırıyoruz ki her level başında objeler farklı yerlerde olsun
        Shuffle(emptyCells);

        //Sırayla objeleri boş hücrelere yerleştir
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

        //Objenin üzerindeki bileşene veriyi işleme işlemi
        PlaceableObject po = obj.GetComponent<PlaceableObject>();
        po.verisi = veri;
        po.currentCell = cell;
        po.SetPreviewMode(false);

        cell.currentObject = po;
    }

    //Liste karıştırma kodu
    void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rnd = Random.Range(i, list.Count);
            (list[i], list[rnd]) = (list[rnd], list[i]); //Yer değiştirme
        }
    }
}
