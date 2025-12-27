using UnityEngine;
using System.Collections.Generic;

public class PlacementManager : MonoBehaviour
{
    public Grid grid;
    public GridManager gridManager;
    public BirlestirmeYoneticisi birlestirmeYoneticisi;

    [Header("Spawnlanacak Başlangıç Objeleri")]
    public List<ObjeVerisi> spawnlanabilirObjeler;

    private GameObject currentPrefab;
    private GameObject previewObject;
    private GridCell selectedCell;
    private bool isDragging = false;
    private bool hasDragged = false;
    private bool pressedOnPreview = false;
    private Vector2 touchStartPos;
    private const float dragThreshold = 5f;

    private GridCell kaynakHucre;
    private PlaceableObject yerdekiGercekObje;
    private bool yerdenMiAldik = false;
    private ObjeVerisi siradakiSpawnVerisi;
    private GridCell spawnOriginCell; 

    void Start() { } 

    void Update()
    {
        HandleInput();
    }

    public void SpawnYeniObje()
    {
        HazirlaYeniSpawn();
    }

    void HazirlaYeniSpawn()
    {
        if (spawnlanabilirObjeler.Count == 0) return;

        yerdenMiAldik = false;
        kaynakHucre = null;
        spawnOriginCell = null;

        int rastgeleSayi = Random.Range(0, spawnlanabilirObjeler.Count);
        siradakiSpawnVerisi = spawnlanabilirObjeler[rastgeleSayi];
        currentPrefab = siradakiSpawnVerisi.objePrefab;

        CreatePreview();
        SelectFirstEmptyCell();
    }

    void CreatePreview()
    {
        if (previewObject != null) Destroy(previewObject);

        previewObject = Instantiate(currentPrefab);

        foreach (var col in previewObject.GetComponentsInChildren<Collider>()) col.enabled = false;

        var po = previewObject.GetComponent<PlaceableObject>();
        if (po != null) 
        {
            po.BoyutuGuncelle(); 
            po.SetPreviewMode(true);
        }
    }

    void Place()
    {
        if (selectedCell == null) return;

        // A. İPTAL
        if (yerdenMiAldik && selectedCell == kaynakHucre)
        {
            IptalEt();
            return;
        }
        
        // Bu hamlede üretilen yeni bir sabit obje var mı? (İnşaat alanı vb.)
        PlaceableObject yeniSabitObje = null;

        // B. BOŞ YERE KOYMA
        if (selectedCell.IsEmpty())
        {
            if (yerdenMiAldik)
            {
                // Yerdeki objeyi taşı
                kaynakHucre.currentObject = null;
                
                yerdekiGercekObje.gameObject.SetActive(true);
                yerdekiGercekObje.transform.position = previewObject.transform.position;
                yerdekiGercekObje.currentCell = selectedCell;
                selectedCell.currentObject = yerdekiGercekObje;
                
                var previewPO = previewObject.GetComponent<PlaceableObject>();
                if (previewPO != null) yerdekiGercekObje.icindekiMalzemeler = new List<ObjeVerisi>(previewPO.icindekiMalzemeler);
                
                yerdekiGercekObje.hareketHakki = 0;
                yerdekiGercekObje.BoyutuGuncelle();
                yerdekiGercekObje.SetPreviewMode(false); 

                Destroy(previewObject);
                YerdenOynamaBitti();
            }
            else
            {
                // Yeni Spawn
                GameObject obj = Instantiate(currentPrefab, previewObject.transform.position, Quaternion.identity);
                PlaceableObject po = obj.GetComponent<PlaceableObject>();

                var previewPO = previewObject.GetComponent<PlaceableObject>();
                if (previewPO != null) 
                {
                    po.verisi = previewPO.verisi;
                    po.icindekiMalzemeler = new List<ObjeVerisi>(previewPO.icindekiMalzemeler);
                }

                po.hareketHakki = 0; 
                po.BoyutuGuncelle(); 
                po.SetPreviewMode(false);
                
                po.currentCell = selectedCell;
                selectedCell.currentObject = po;

                Destroy(previewObject);
                selectedCell = null;
                previewObject = null;
                HazirlaYeniSpawn();
            }
        }
        // C. DOLU YERE KOYMA (BİRLEŞTİRME)
        else
        {
            PlaceableObject yerdekiObje = selectedCell.currentObject;
            PlaceableObject elimizdekiObje = previewObject.GetComponent<PlaceableObject>();

            if (yerdekiObje.kilitliMi) { IptalEt(); return; }

            // Kural: Yeni spawn, yığınla birleşemez
            if (!yerdenMiAldik && yerdekiObje.icindekiMalzemeler.Count >= 2)
            {
                IptalEt();
                return;
            }

            int sonuc = birlestirmeYoneticisi.YiginlamaKontrol(elimizdekiObje, yerdekiObje);

            if (sonuc > 0)
            {
                Destroy(previewObject);
                if (yerdenMiAldik)
                {
                    kaynakHucre.currentObject = null;
                    Destroy(yerdekiGercekObje.gameObject);
                }

                if (sonuc == 2) 
                {
                    // --- BURADA İNŞAAT ALANI OLUŞTU ---
                    if (selectedCell.currentObject != null && selectedCell.currentObject != birlestirmeYoneticisi.sonUretilenObje)
                        Destroy(selectedCell.currentObject.gameObject);
                        
                    selectedCell.currentObject = birlestirmeYoneticisi.sonUretilenObje;
                    
                    // Otomatik kontrol için işaretle
                    yeniSabitObje = birlestirmeYoneticisi.sonUretilenObje;
                }

                if (yerdenMiAldik) YerdenOynamaBitti();
                else { selectedCell = null; previewObject = null; HazirlaYeniSpawn(); }
            }
            else
            {
                IptalEt();
            }
        }
        
        // --- İŞTE O EKLEME BURADA ---
        // Eğer bu hamle sonucunda bir İnşaat Alanı (veya benzeri sabit obje) oluştuysa,
        // etrafına bakıp "Ben bina olabilir miyim?" diye kontrol etsin.
        if (yeniSabitObje != null)
        {
            birlestirmeYoneticisi.OtomatikKomsulukKontrolu(yeniSabitObje);
        }
    }

    void IptalEt()
    {
        if (yerdenMiAldik)
        {
            yerdekiGercekObje.gameObject.SetActive(true);
            Destroy(previewObject);
            YerdenOynamaBitti();
        }
        else
        {
            if (spawnOriginCell != null) SelectCell(spawnOriginCell);
        }
    }

    void YerdenOynamaBitti()
    {
        yerdenMiAldik = false;
        yerdekiGercekObje = null;
        kaynakHucre = null;
        if(previewObject!=null) Destroy(previewObject);

        currentPrefab = siradakiSpawnVerisi.objePrefab;
        CreatePreview();
        SelectFirstEmptyCell();
    }

    void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            hasDragged = false;
            pressedOnPreview = false;
            touchStartPos = Input.mousePosition;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Vector3Int cellPos = grid.WorldToCell(hit.point);
                GridCell cell = gridManager.GetCell(cellPos);

                if (cell != null && cell == selectedCell && !yerdenMiAldik)
                {
                    pressedOnPreview = true;
                }
                else if (cell != null && !cell.IsEmpty() && !cell.currentObject.kilitliMi)
                {
                    if (cell.currentObject.hareketHakki > 0)
                    {
                        if (previewObject != null) Destroy(previewObject);

                        yerdenMiAldik = true;
                        kaynakHucre = cell;
                        yerdekiGercekObje = cell.currentObject;
                        currentPrefab = yerdekiGercekObje.verisi.objePrefab;

                        yerdekiGercekObje.gameObject.SetActive(false);
                        CreatePreview(); 
                        
                        var po = previewObject.GetComponent<PlaceableObject>();
                        po.icindekiMalzemeler = new List<ObjeVerisi>(yerdekiGercekObje.icindekiMalzemeler);
                        po.BoyutuGuncelle(); 

                        SelectCell(cell);
                        pressedOnPreview = true;
                    }
                }
            }
            UpdatePreviewPosition();
        }

        if (Input.GetMouseButton(0) && isDragging)
        {
            if (!hasDragged && Vector2.Distance(Input.mousePosition, touchStartPos) >= dragThreshold)
            {
                hasDragged = true;
            }
            UpdatePreviewPosition();
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            if (selectedCell != null) Place();
            else IptalEt();
        }
    }

    void UpdatePreviewPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane zemin = new Plane(Vector3.up, Vector3.zero);
        float enter;

        if (zemin.Raycast(ray, out enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            Vector3Int cellPos = grid.WorldToCell(hitPoint);
            GridCell cell = gridManager.GetCell(cellPos);
            
            if (cell != null)
            {
                bool secilebilir = false;

                GridCell referansHucre = yerdenMiAldik ? kaynakHucre : spawnOriginCell;
                if (referansHucre != null)
                {
                    int mesafeX = Mathf.Abs(cell.cellPosition.x - referansHucre.cellPosition.x);
                    int mesafeY = Mathf.Abs(cell.cellPosition.z - referansHucre.cellPosition.z);
                    if (mesafeX + mesafeY > 1) return; 
                }

                if (cell.IsEmpty()) 
                {
                    secilebilir = true;
                }
                else if (previewObject != null)
                {
                    var yerdeki = cell.currentObject;
                    var elimizdeki = previewObject.GetComponent<PlaceableObject>();
                    
                    if (yerdeki != null && elimizdeki != null && 
                        yerdeki.verisi == elimizdeki.verisi && !yerdeki.kilitliMi)
                    {
                        if (!yerdenMiAldik && yerdeki.icindekiMalzemeler.Count >= 2) secilebilir = false;
                        else secilebilir = true;
                    }
                    if (yerdenMiAldik && cell == kaynakHucre) secilebilir = true;
                    if (!yerdenMiAldik && cell == spawnOriginCell) secilebilir = true;
                }

                if (secilebilir) SelectCell(cell);
            }
        }
    }

    void SelectCell(GridCell cell)
    {
        if (previewObject == null) return;

        selectedCell = cell;
        previewObject.SetActive(true);

        float offset = 0.5f;
        var po = previewObject.GetComponent<PlaceableObject>();
        if (po != null) offset = po.heightOffset;

        previewObject.transform.position =
            grid.GetCellCenterWorld(cell.cellPosition) + Vector3.up * offset;
    }

    void SelectFirstEmptyCell()
    {
        GridCell firstEmpty = gridManager.GetFirstEmptyCell();
        if (firstEmpty != null) 
        {
            spawnOriginCell = firstEmpty; 
            SelectCell(firstEmpty);
        }
    }

    public void BeginPlacementAfterInitialSpawn()
    {
        SpawnYeniObje();
    }
}