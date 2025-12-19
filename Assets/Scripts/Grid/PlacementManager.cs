using UnityEngine;

public class PlacementManager : MonoBehaviour
{
    public Grid grid;
    public GridManager gridManager;
    public GameObject placeablePrefab;

    private GameObject previewObject;
    private GridCell currentHoverCell;

    void Start()
    {
        previewObject = Instantiate(placeablePrefab);

        foreach (var col in previewObject.GetComponentsInChildren<Collider>())
            col.enabled = false;

        previewObject.GetComponent<PlaceableObject>()
            .SetPreviewMode(true);

        previewObject.SetActive(false);
    }


    void Update()
    {
        UpdatePreview();

        if (Input.GetMouseButtonDown(0))
        {
            Place();
        }
    }

    void UpdatePreview()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit))
        {
            previewObject.SetActive(false);
            currentHoverCell = null;
            return;
        }

        Vector3Int cellPos = grid.WorldToCell(hit.point);
        GridCell cell = gridManager.GetCell(cellPos);

        if (cell == null)
        {
            previewObject.SetActive(false);
            currentHoverCell = null;
            return;
        }

        currentHoverCell = cell;
        previewObject.SetActive(true);
        previewObject.transform.position = grid.GetCellCenterWorld(cellPos);
    }

    void Place()
    {
        if (currentHoverCell == null || !currentHoverCell.IsEmpty())
            return;

        Vector3 spawnPos = grid.GetCellCenterWorld(currentHoverCell.cellPosition);

        GameObject obj = Instantiate(placeablePrefab, spawnPos, Quaternion.identity);

        PlaceableObject po = obj.GetComponent<PlaceableObject>();

        po.SetPreviewMode(false);

        po.currentCell = currentHoverCell;
        currentHoverCell.currentObject = po;
    }


}