using UnityEngine;

public class PlacementManager : MonoBehaviour
{
    public Grid grid;
    public GridManager gridManager;
    public GameObject placeablePrefab;

    private GameObject previewObject;
    private GridCell selectedCell;

    void Start()
    {
        previewObject = Instantiate(placeablePrefab);//Yerleştirilecek objenin previewını spawn eder.

        foreach (var col in previewObject.GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }
        //Preview'ın raycast ile çarpışmasını engellemek için colliderları devredışı bırakır.

        previewObject.GetComponent<PlaceableObject>().SetPreviewMode(true);//Preview animasyon kodu

        GridCell firstEmpty = gridManager.GetFirstEmptyCell();//Grid üzerindeki ilk boş hücreyi alır
        if (firstEmpty != null)
        {
            SelectCell(firstEmpty);
        }
        //Eğer boş hücre varsa preview oraya taşınır
        
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleTouch();
        }
    }

    void HandleTouch()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit))
        {
            return;
        }
        //Raycast collidera çarpmazsa method durur.

        Vector3Int cellPos = grid.WorldToCell(hit.point);//Raycastin çarptığı dünya pozisyonunu grid koordinatlarına çevirir.
        GridCell cell = gridManager.GetCell(cellPos);//Grid pozisyondaki hücre alınır

        if (cell == null || !cell.IsEmpty())
        {
            return;
        }
        //Hücre yoksa ve doluysa hiçbir şey yapmaz.

        if (selectedCell == null)
        {
            SelectCell(cell);
            return;
        }
        //Henüz bir hücre seçili değilse tıklanan hücre seçilir ve preview oraya taşınır

        if (cell == selectedCell)
        {
            Place();
        }
        //Aynı hücreye tıklandığında obje o hcürede spawn olur
        else
        {
            SelectCell(cell);
        }
        //Farklı boş hücreye tıklandıysa preview oraya taşınır
    }

    void SelectCell(GridCell cell)
    {
        selectedCell = cell;//Yeni hücre seçili olarak atanır.
        previewObject.SetActive(true);

        float offset = previewObject.GetComponent<PlaceableObject>().heightOffset;//Objeye offset vermek için

        previewObject.transform.position = grid.GetCellCenterWorld(cell.cellPosition) + Vector3.up * offset;//Hücrenin merkezine y koodinatında objeye offset vererek yerleştirir.
    }

    void Place()//Preview objeye dönüştrmek için yazılan methodtur
    {
        if (selectedCell == null || !selectedCell.IsEmpty())
        {
            return;
        }
        //Seçili hücre yoksa yerleştirme yapılmaz return döner.

        PlaceableObject prefabPO = placeablePrefab.GetComponent<PlaceableObject>();//Prefab üzerineki ayarlar çekilir

        Vector3 spawnPos = grid.GetCellCenterWorld(selectedCell.cellPosition) + Vector3.up * prefabPO.heightOffset;
        //Hücrenin merkezine y koodinatında objeye offset vererek yerleştirir. Üstekinden farkı burada obje spawn edilir, üstekinde ise preview olarak gözükür.

        GameObject obj = Instantiate(placeablePrefab, spawnPos, Quaternion.identity);

        PlaceableObject po = obj.GetComponent<PlaceableObject>();
        po.SetPreviewMode(false);
        //Animasyon kapatılır ve bu obje artık preview değildir.

        po.currentCell = selectedCell;
        selectedCell.currentObject = po;
        //Hücre artık dolu kabul edilir.

        selectedCell = null;
        previewObject.SetActive(false);
        //Yerleştirdikten sonra preview sıfırlanır.
        
        GridCell nextEmpty = gridManager.GetFirstEmptyCell();//Grid üzerindeki bir sonraki boş hücre bulur.
        if (nextEmpty != null)
        {
            SelectCell(nextEmpty);
        }
        //Preview otomatik olarak yeni boş hücreye taşınır.
    }
}
