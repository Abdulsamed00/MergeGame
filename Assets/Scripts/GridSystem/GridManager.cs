using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System; 

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

                // --- DEĞİŞİKLİK BURADA ---
                // "SetTrigger" satırını SİLDİK.
                // Çünkü Animator'da "Entry -> CellAnim" bağlı olduğu için 
                // obje oluşur oluşmaz animasyon OTOMATİK başlayacak.
                
                yield return new WaitForSeconds(hucreAnimasyonSuresi);
            }
        }
        
        // Animasyonların bitmesini bekle (Örn: 0.5sn)
        yield return new WaitForSeconds(1f);

        // --- OPTİMİZASYON VE KİLİTLEME ---
        // Grid oluştu, animasyonlar bitti. Artık Animatörleri kapatalım.
        // Böylece hem tekrar oynamazlar hem de performans artar.
        foreach (var cell in cells.Values)
        {
            Animator anim = cell.GetComponent<Animator>();
            if (anim == null) anim = cell.GetComponentInChildren<Animator>();
            
            if (anim != null)
            {
                // Animasyonun son karesinde durması için enabled false yapıyoruz.
                // Eğer animasyonun "Loop Time"ı açıksa kapatmayı unutma!
                anim.enabled = false; 
            }
        }

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