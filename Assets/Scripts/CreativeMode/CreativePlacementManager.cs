using UnityEngine;

public class CreativePlacementManager : MonoBehaviour
{
    public static CreativePlacementManager Instance;

    public Grid grid;
    public CreativeGridManager gridManager;
    public LayerMask groundLayer;

    private ObjeVerisi seciliObje;
    private GameObject preview;
    private CreativeGridCell currentCell;
    private bool isEraserActive;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (seciliObje == null && !isEraserActive) return;

        HandleMouseInput();

        if (currentCell != null)
        {
            if (isEraserActive)
            {
                if (Input.GetMouseButton(0))
                {
                    currentCell.Clear();
                }
            }
            else
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Koy();
                }
            }
        }
    }

    public void ActivateEraser()
    {
        isEraserActive = true;
        seciliObje = null;
        Destroy(preview);
    }

    public void ObjeSec(ObjeVerisi veri)
    {
        isEraserActive = false;
        seciliObje = veri;

        Destroy(preview);
        preview = Instantiate(veri.objePrefab);
        
        foreach (var col in preview.GetComponentsInChildren<Collider>())
            col.enabled = false;
    }

    public void OnClearAllButtonClicked()
    {
        gridManager.ClearAllGrid();
        isEraserActive = false;
        seciliObje = null;
        Destroy(preview);
    }

    void HandleMouseInput()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
        {
            Vector3Int cellPos = grid.WorldToCell(hit.point);
            currentCell = gridManager.GetCell(cellPos);

            if (!isEraserActive && preview != null && currentCell != null)
                preview.transform.position = currentCell.transform.position;
        }
        else
        {
            currentCell = null;
        }
    }

    void Koy()
    {
        if (!currentCell.IsEmpty()) return;

        Quaternion rot = Quaternion.Euler(90, 0, 0); 
        GameObject obj = Instantiate(seciliObje.objePrefab, currentCell.transform.position, rot);

        currentCell.PlaceObject(obj, seciliObje);
    }

}