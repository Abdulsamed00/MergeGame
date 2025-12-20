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
        previewObject = Instantiate(placeablePrefab);

        foreach (var col in previewObject.GetComponentsInChildren<Collider>())
            col.enabled = false;

        previewObject.GetComponent<PlaceableObject>().SetPreviewMode(true);
        previewObject.SetActive(false);
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
            return;

        Vector3Int cellPos = grid.WorldToCell(hit.point);
        GridCell cell = gridManager.GetCell(cellPos);

        if (cell == null || !cell.IsEmpty())
            return;

        //Eğer henüz hücre seçilmediyse hücre seçilir
        if (selectedCell == null)
        {
            SelectCell(cell);
            return;
        }

        //Aynı hücreye ikinci kez tıklandıysa yerleştir
        if (cell == selectedCell)
        {
            Place();
        }
        else
        {
            //Farklı boş hücreye tıklandıysa preview oraya taşınır
            SelectCell(cell);
        }
    }

    void SelectCell(GridCell cell)
    {
        selectedCell = cell;
        previewObject.SetActive(true);

        float offset = previewObject.GetComponent<PlaceableObject>().heightOffset;

        previewObject.transform.position =
            grid.GetCellCenterWorld(cell.cellPosition) + Vector3.up * offset;
    }

    void Place()
    {
        if (selectedCell == null || !selectedCell.IsEmpty())
            return;

        PlaceableObject prefabPO = placeablePrefab.GetComponent<PlaceableObject>();

        Vector3 spawnPos =
            grid.GetCellCenterWorld(selectedCell.cellPosition)
            + Vector3.up * prefabPO.heightOffset;

        GameObject obj = Instantiate(placeablePrefab, spawnPos, Quaternion.identity);

        PlaceableObject po = obj.GetComponent<PlaceableObject>();
        po.SetPreviewMode(false);

        po.currentCell = selectedCell;
        selectedCell.currentObject = po;

        //Yerleştirdikten sonra preview sıfırlanır
        selectedCell = null;
        previewObject.SetActive(false);
    }
}
