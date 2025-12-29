using UnityEngine;
using UnityEngine.Tilemaps; // Grid sistemiyle uyumlu çalışabilmek için
using System.Collections.Generic; // Dictionary kullanabilmek için

public class GridManager : MonoBehaviour
{
    [Header("Grid Ayarları")]
    public Grid grid;
    public GridCell cellPrefab; // Hücre için instantiate edilecek prefab

    // Bu değerleri artık GameManager yönetecek, o yüzden HideInInspector kalabilir
    [HideInInspector] public int width = 4;
    [HideInInspector] public int height = 4;

    [Header("Camera")]
    public CameraControlTool cameraController;

    //Hücreleri tuttuğumuz sözlük
    private Dictionary<Vector3Int, GridCell> cells = new();

    //GameManager tarafından çağrılacak ana fonksiyon
    public void GridiOlustur(int w, int h)
    {
        //Eski grid varsa temizle
        TemizleVeYokEt();

        //Boyutları ayarla
        width = w;
        height = h;

        //Grid'i fiziksel olarak oluştur
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Vector3Int cellPos = new Vector3Int(x, 0, z);
                Vector3 worldPos = grid.GetCellCenterWorld(cellPos);

                GridCell cell = Instantiate(cellPrefab, worldPos, Quaternion.identity, transform);
                cell.cellPosition = cellPos;

                cells.Add(cellPos, cell);
            }
        }

        //Grid oluştuğu için kamerayı ortala
        if (cameraController != null)
        {
            Vector3 center = GetGridExactCenterWorld();
            cameraController.InitFromGridCenter(center);
        }
    }

    //Kaybetme kontrolü için
    public bool GridTamamenDoluMu()
    {
        foreach (var cell in cells.Values)
        {
            //Eğer tek bir tane bile boş hücre varsa grid dolmamıştır
            if (cell.IsEmpty()) return false;
        }
        //Hiç boş yer bulunamadı, demek ki dolu
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
            if (cell.IsEmpty())
            {
                return cell;
            }
        }
        return null;
    }

    public List<GridCell> GetAllCells()
    {
        return new List<GridCell>(cells.Values);
    }

    //Level reset için
    private void TemizleVeYokEt()
    {
        foreach (var cell in cells.Values)
        {
            if (cell.currentObject != null) Destroy(cell.currentObject.gameObject);
            Destroy(cell.gameObject);
        }
        cells.Clear();
    }

    //Kamera merkezleme kodu
    public Vector3 GetGridExactCenterWorld()
    {
        Bounds bounds = new Bounds();
        bool first = true;

        foreach (var cell in cells.Values)
        {
            if (first)
            {
                bounds = new Bounds(cell.transform.position, Vector3.zero);
                first = false;
            }
            else
            {
                bounds.Encapsulate(cell.transform.position);
            }
        }
        return bounds.center;
    }
}