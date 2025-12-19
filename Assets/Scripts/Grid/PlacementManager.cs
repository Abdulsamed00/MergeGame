using UnityEngine;

public class PlacementManager : MonoBehaviour
{
    public Grid grid;
    public GridManager gridManager;
    public GameObject placeablePrefab;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Place();
        }
    }

    void Place()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3Int cellPos = grid.WorldToCell(hit.point);
            GridCell cell = gridManager.GetCell(cellPos);

            if (cell != null && cell.IsEmpty())
            {
                Vector3 spawnPos = grid.GetCellCenterWorld(cellPos);
                GameObject obj = Instantiate(placeablePrefab, spawnPos, Quaternion.identity);

                PlaceableObject po = obj.GetComponent<PlaceableObject>();
                po.currentCell = cell;
                cell.currentObject = po;
            }
        }
    }
}