using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System; // Action için gerekli

public class GridManager : MonoBehaviour
{
    [Header("Grid Ayarları")]
    public Grid grid;
    public GridCell cellPrefab;

    [HideInInspector] public int width = 4;
    [HideInInspector] public int height = 4;
    
    [Header("Camera")]
    public CameraControlTool cameraController;

    [Header("Animasyon Zamanlaması")]
    public float hucreAnimasyonSuresi = 0.1f;

    private Dictionary<Vector3Int, GridCell> cells = new();

    // --- DEĞİŞİKLİK: Action onComplete parametresi eklendi ---
    public IEnumerator GridiAnimasyonluOlustur(int w, int h, GridCell zeminPrefabi, Action onComplete = null)
    {
        TemizleVeYokEt();

        width = w;
        height = h;
        
        if (cameraController != null)
        {
            Vector3 centerLogic = grid.GetCellCenterWorld(new Vector3Int(width / 2, 0, height / 2));
            cameraController.InitFromGridCenter(centerLogic);
        }

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Vector3Int cellPos = new Vector3Int(x, 0, z); 
                Vector3 worldPos = grid.GetCellCenterWorld(cellPos); 

                GridCell kullanilacakPrefab = zeminPrefabi != null ? zeminPrefabi : cellPrefab;

                GridCell cell = Instantiate(
                    kullanilacakPrefab,
                    worldPos,
                    kullanilacakPrefab.transform.rotation,
                    transform
                );
                cell.cellPosition = cellPos;
                cells.Add(cellPos, cell);

                yield return new WaitForSeconds(hucreAnimasyonSuresi);
            }
        }
        
        // Son bir bekleme (Animasyonların tamamen oturması için)
        yield return new WaitForSeconds(0.1f);

        // --- DEĞİŞİKLİK: Grid bitti, GameManager'a haber ver ---
        if (onComplete != null)
        {
            onComplete.Invoke();
        }
    }

    public bool GridTamamenDoluMu()
    {
        foreach (var cell in cells.Values)
        {
            if (cell.IsEmpty()) return false;
        }
        return true;
    }

    public GridCell GetCell(Vector3Int pos)
    {
        cells.TryGetValue(pos, out GridCell cell);
        return cell;
    }

    public GridCell GetFirstEmptyCell()
    {
        foreach (var cell in cells.Values)
        {
            if (cell.IsEmpty()) return cell;
        }
        return null;
    }

    public List<GridCell> GetAllCells()
    {
        return new List<GridCell>(cells.Values);
    }

    private void TemizleVeYokEt()
    {
        foreach (var cell in cells.Values)
        {
            if (cell.currentObject != null) Destroy(cell.currentObject.gameObject);
            Destroy(cell.gameObject);
        }
        cells.Clear();
    }
    
    public void TemizleVeYokEtPublic()
    {
        foreach (var cell in cells.Values)
        {
            if (cell.currentObject != null)
            {
                Destroy(cell.currentObject.gameObject);
                cell.currentObject = null;
            }
        }
    }
}