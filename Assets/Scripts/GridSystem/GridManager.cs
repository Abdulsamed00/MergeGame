using UnityEngine;
using UnityEngine.Tilemaps;//Grid sistemiyle uyumlu çalışabilmek için
using System.Collections.Generic; //Dictionary kullanabilmek için

public class GridManager : MonoBehaviour
{
    [Header("Grid")]
    public Grid grid;
    public GridCell cellPrefab;//Hücre için instantiate edliecek prefab için
    public int width = 4;
    public int height = 4;
    
    [Header("Camera")]
    public CameraControlTool cameraController;

    public List<GridCell> GetAllCells()
    {
        return new List<GridCell>(cells.Values);
    }

    private Dictionary<Vector3Int, GridCell> cells = new();
    //Vector3Int = Grid koordinatları için(x,y,z)
    //GridCell = O pozisyondaki hücre için
    //Dictionary key ve ona karşılık gelen value değrini hızlıca saklamak ve bulmak için kullanılır.

    void Start()
    {
        CreateGrid();

        Vector3 center = GetGridExactCenterWorld();
        cameraController.InitFromGridCenter(center);
    }



    void CreateGrid()
    {
        for (int x = 0; x < width; x++)//Gridin genişliğini kontrol eder.
        {
            for (int z = 0; z < height; z++)//Gridin yüksekliğini kontrol eder.
            {
                Vector3Int cellPos = new Vector3Int(x, 0, z); //Hücrenin grid kordinatları
                Vector3 worldPos = grid.GetCellCenterWorld(cellPos); //Grid koordinatlarını gerçek sahne pozisyonuna çevirir bu sayede hücreler tam ortalı ve hizalı olur.

                GridCell cell = Instantiate(cellPrefab, worldPos, Quaternion.identity, transform);
                cell.cellPosition = cellPos;//Hücrenin kendi pozisyonunu tanıması

                cells.Add(cellPos, cell);//Hücre Dictonary'ye kaydedilir.
            }
        }
    }

    public GridCell GetCell(Vector3Int pos)//Verilen grid koordinatlarındaki hücreyi döndürür
    {
        cells.TryGetValue(pos, out GridCell cell);//Dictionary içinde o pozisyon var mı kontrol eder. Varsa eğer cell değişkinine atar. Yoksa eğer null değer döner.
        //TryGetValue bir Dictionary fonksiyonudur. Anlamı: Key var mı varsa bana değerini ver
        //"pos" aranan anahtar, out GridCell cell ise anahtar varsa out sayesinde cell içini doldur. Yani true veya false döner
        return cell;
    }
    //Grid koordinatlarında hücreyi bulur hücre var mı yok mu kontrol eder.

    public GridCell GetFirstEmptyCell()
    {
        foreach (var cell in cells.Values)//Dictionary içindeki hücreleri toplar, tek tek gezer.
        {
            if (cell.IsEmpty())
            {
                return cell;
            }
            //Hücre boşsa o hücreyi döndürür.
        }
        return null;
    }

    public void ClearGrid()
    {
        foreach (var cell in cells.Values)
        {
            cell.currentObject = null;
        }
    }
    
    public Vector3 GetGridCenterWorld()
    {
        Vector3Int centerCell = new Vector3Int(
            (width - 1) / 2,
            0,
            (height - 1) / 2
        );

        return grid.GetCellCenterWorld(centerCell);
    }
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