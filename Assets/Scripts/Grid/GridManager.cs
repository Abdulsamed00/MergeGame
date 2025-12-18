using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public Grid grid;
    public GridCell cellPrefab;
    public int width = 4;
    public int height = 4;

    private Dictionary<Vector3Int, GridCell> cells = new();

    void Start()
    {
        CreateGrid();
    }

    void CreateGrid()
    {
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
    }

    public GridCell GetCell(Vector3Int pos)
    {
        cells.TryGetValue(pos, out GridCell cell);
        return cell;
    }
}