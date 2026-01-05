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

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (seciliObje == null || preview == null)
            return;

        PreviewHareket();

        if (Input.GetMouseButtonDown(0))
            Koy();
    }


    // -----------------------------
    // BUTON → OBJE SEÇİMİ
    // -----------------------------
    public void ObjeSec(ObjeVerisi veri)
    {
        seciliObje = veri;

        if (preview != null)
            Destroy(preview);

        preview = Instantiate(veri.objePrefab);

        // Preview SADECE görsel
        foreach (var col in preview.GetComponentsInChildren<Collider>())
            col.enabled = false;
    }

    // -----------------------------
    // PREVIEW GRID'E YAPIŞIR
    // -----------------------------
    void PreviewHareket()
    {
        if (preview == null) return;
        
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
        {
            Vector3Int cellPos = grid.WorldToCell(hit.point);
            CreativeGridCell cell = gridManager.GetCell(cellPos);

            if (cell == null)
            {
                currentCell = null;
                return;
            }

            currentCell = cell;
            preview.transform.position = cell.transform.position;
        }
    }

    // -----------------------------
    // TIK → OBJE YERLEŞTİR
    // -----------------------------
    void Koy()
    {
        if (seciliObje == null)
        {
            Debug.LogWarning("Henüz obje seçilmedi!");
            return;
        }

        if (currentCell == null)
        {
            Debug.LogWarning("Geçerli hücre yok!");
            return;
        }

        if (!currentCell.IsEmpty())
            return;

        GameObject obj = Instantiate(
            seciliObje.objePrefab,
            currentCell.transform.position,
            Quaternion.identity
        );

        currentCell.PlaceObject(obj);
    }
}
