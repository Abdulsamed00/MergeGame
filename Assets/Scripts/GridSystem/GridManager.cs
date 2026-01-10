using UnityEngine;
using System.Collections; // IEnumerator için gerekli
using System.Collections.Generic;

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
    public float hucreAnimasyonSuresi = 0.1f; // Her hücrenin düşmesi kaç saniye sürüyor?

    private Dictionary<Vector3Int, GridCell> cells = new();

    // --- DEĞİŞİKLİK BURADA: Void yerine IEnumerator ---
    public IEnumerator GridiAnimasyonluOlustur(int w, int h)
    {
        TemizleVeYokEt();

        width = w;
        height = h;
        
        // Kamera hemen ortalansın ki animasyonu izleyebilelim
        if (cameraController != null)
        {
            // Gridin tahmini merkezini hesapla
            Vector3 centerLogic = grid.GetCellCenterWorld(new Vector3Int(width / 2, 0, height / 2));
            cameraController.InitFromGridCenter(centerLogic);
        }

        // Hücreleri Tek Tek Oluşturma Döngüsü
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Vector3Int cellPos = new Vector3Int(x, 0, z); 
                Vector3 worldPos = grid.GetCellCenterWorld(cellPos); 

                // Hücreyi oluştur (Animator'ı Entry state'inde olduğu için animasyon otomatik başlar)
                GridCell cell = Instantiate(cellPrefab, worldPos, Quaternion.identity, transform);
                cell.cellPosition = cellPos;

                cells.Add(cellPos, cell);

                // --- BEKLEME ---
                // Animasyonun bitmesini (veya bir sonrakine geçmeyi) bekle
                yield return new WaitForSeconds(hucreAnimasyonSuresi);
            }
        }
        
        // Grid bitti, son hücrenin de yerine oturması için minik bir bekleme daha
        yield return new WaitForSeconds(0.1f);
    }

    // --- Diğer Fonksiyonlar Aynen Kalıyor ---
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
    
    public Vector3 GetGridExactCenterWorld()
    {
        // Grid oluşurken kamera zaten ayarlandığı için burası artık opsiyonel ama kalabilir
        Bounds bounds = new Bounds();
        bool first = true;
        foreach (var cell in cells.Values)
        {
            if (first) { bounds = new Bounds(cell.transform.position, Vector3.zero); first = false; }
            else { bounds.Encapsulate(cell.transform.position); }
        }
        return bounds.center;
    }
}