using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class CreativeGridManager : MonoBehaviour
{
    [Header("Grid Ayarları")]
    public Grid grid;
    public CreativeGridCell cellPrefab;
    public int width = 9;
    public int height = 9;

    private Dictionary<Vector3Int, CreativeGridCell> cells = new();

    void Start()
    {
        CreateGrid();
    }

    void CreateGrid()
    {
        cells.Clear();

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Vector3Int cellPos = new Vector3Int(x, 0, z);
                Vector3 worldPos = grid.GetCellCenterWorld(cellPos);

                CreativeGridCell cell = Instantiate(
                    cellPrefab,
                    worldPos,
                    Quaternion.identity,
                    transform
                );

                cell.Init(cellPos);
                cells.Add(cellPos, cell);
            }
        }
    }

    // ---------- CREATIVE API ----------

    public CreativeGridCell GetCell(Vector3Int pos)
    {
        cells.TryGetValue(pos, out CreativeGridCell cell);
        return cell;
    }

    public bool IsCellEmpty(Vector3Int pos)
    {
        CreativeGridCell cell = GetCell(pos);
        return cell != null && cell.IsEmpty();
    }

    public Vector3 GetCellWorldPosition(Vector3Int pos)
    {
        return grid.GetCellCenterWorld(pos);
    }

    public Vector3Int WorldToCell(Vector3 worldPos)
    {
        return grid.WorldToCell(worldPos);
    }

    public bool PlaceObject(Vector3Int pos, GameObject obj)
    {
        CreativeGridCell cell = GetCell(pos);

        if (cell == null || !cell.IsEmpty())
            return false;

        cell.PlaceObject(obj);
        return true;
    }

    public bool RemoveObject(Vector3Int pos)
    {
        CreativeGridCell cell = GetCell(pos);

        if (cell == null || cell.IsEmpty())
            return false;

        cell.Clear();
        return true;
    }
    
    public void ClearAllGrid()
    {
        foreach (var cell in cells.Values)
        {
            cell.Clear(); 
        }
    }
}
