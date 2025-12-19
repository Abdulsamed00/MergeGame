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

        float offset = previewObject.GetComponent<PlaceableObject>().heightOffset;

        previewObject.transform.position =
            grid.GetCellCenterWorld(cellPos) + Vector3.up * offset;
    }


    void Place()
    {
        if (currentHoverCell == null || !currentHoverCell.IsEmpty())
            return;

        PlaceableObject prefabPO = placeablePrefab.GetComponent<PlaceableObject>();

        Vector3 spawnPos =
            grid.GetCellCenterWorld(currentHoverCell.cellPosition)
            + Vector3.up * prefabPO.heightOffset;

        GameObject obj = Instantiate(placeablePrefab, spawnPos, Quaternion.identity);

        PlaceableObject po = obj.GetComponent<PlaceableObject>();
        po.SetPreviewMode(false);

        po.currentCell = currentHoverCell;
        currentHoverCell.currentObject = po;
    }



}