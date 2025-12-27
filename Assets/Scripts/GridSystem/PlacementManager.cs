using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public class PlacementManager : MonoBehaviour
{
    public Grid grid;
    public GridManager gridManager;
    public BirlestirmeYoneticisi birlestirmeYoneticisi;

    [Header("Spawn Sistemi")]
    public List<ObjeVerisi> spawnlanabilirObjeler;

    // Kuyruk
    private ObjeVerisi siradakiObjeVerisi;
    private ObjeVerisi sonrakiObjeVerisi;

    // --- ÖNEMLİ DEĞİŞİKLİK: Generic event yerine özel sınıf kullanıyoruz ---
    [Header("UI Event")]
    public SpriteEvent OnNextObjectChanged;
    // ----------------------------------------------------------------------

    private GameObject currentPrefab;
    private GameObject previewObject;
    private GridCell selectedCell;
    private bool pressedOnPreview = false;
    private bool isDragging = false;
    private bool hasDragged = false;
    private Vector2 touchStartPos;
    private const float dragThreshold = 5f;

    private GridCell kaynakHucre;
    private PlaceableObject yerdekiGercekObje;
    private bool yerdenMiAldik = false;
    private GridCell spawnOriginCell;

    void Update()
    {
        HandleInput();
    }

    public void SpawnYeniObje()
    {
        HazirlaYeniSpawn();
    }

    // --- EKSİK OLAN SETUP FONKSİYONU ---
    public void SetupSpawnList(List<ObjeVerisi> gelenListe)
    {
        spawnlanabilirObjeler = gelenListe;
        siradakiObjeVerisi = GetWeightedRandomObject();
        sonrakiObjeVerisi = GetWeightedRandomObject();
        UpdateNextUI();
    }
    // -----------------------------------

    void HazirlaYeniSpawn()
    {
        if (spawnlanabilirObjeler == null || spawnlanabilirObjeler.Count == 0) return;

        siradakiObjeVerisi = sonrakiObjeVerisi;
        sonrakiObjeVerisi = GetWeightedRandomObject();

        // UI Güncelleme çağrısı
        UpdateNextUI();

        currentPrefab = siradakiObjeVerisi.objePrefab;

        CreatePreview();
        SelectFirstEmptyCell();
    }

    // --- EKSİK OLAN UPDATE FONKSİYONU ---
    private void UpdateNextUI()
    {
        if (OnNextObjectChanged != null && sonrakiObjeVerisi != null)
        {
            OnNextObjectChanged.Invoke(sonrakiObjeVerisi.uiIkonu);
        }
    }
    // ------------------------------------

    private ObjeVerisi GetWeightedRandomObject()
    {
        float toplamSans = 0;
        foreach (var obj in spawnlanabilirObjeler) toplamSans += obj.spawnYuzdesi;

        float rastgeleDeger = Random.Range(0, toplamSans);
        float suankiToplam = 0;

        foreach (var obj in spawnlanabilirObjeler)
        {
            suankiToplam += obj.spawnYuzdesi;
            if (rastgeleDeger <= suankiToplam) return obj;
        }
        return spawnlanabilirObjeler[0];
    }

    void CreatePreview()
    {
        if (previewObject != null) Destroy(previewObject);

        previewObject = Instantiate(currentPrefab);
        foreach (var col in previewObject.GetComponentsInChildren<Collider>()) col.enabled = false;

        var po = previewObject.GetComponent<PlaceableObject>();
        if (po != null)
        {
            // Preview'a veri atamayı unutmuyoruz
            po.verisi = siradakiObjeVerisi;
            po.BoyutuGuncelle();
            po.SetPreviewMode(true);
        }
    }

    void Place()
    {
        if (selectedCell == null) return;

        if (yerdenMiAldik && selectedCell == kaynakHucre)
        {
            IptalEt();
            return;
        }

        PlaceableObject yeniSabitObje = null;

        if (selectedCell.IsEmpty())
        {
            if (yerdenMiAldik)
            {
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
                GameObject obj = Instantiate(currentPrefab, previewObject.transform.position, Quaternion.identity);
                PlaceableObject po = obj.GetComponent<PlaceableObject>();

                // Veriyi aktar
                po.verisi = siradakiObjeVerisi;

                var previewPO = previewObject.GetComponent<PlaceableObject>();
                if (previewPO != null)
                {
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
        else
        {
            PlaceableObject yerdekiObje = selectedCell.currentObject;
            PlaceableObject elimizdekiObje = previewObject.GetComponent<PlaceableObject>();
            if (elimizdekiObje.verisi == null) elimizdekiObje.verisi = siradakiObjeVerisi;

            if (yerdekiObje.kilitliMi) { IptalEt(); return; }

            if (!yerdenMiAldik && yerdekiObje.icindekiMalzemeler.Count >= 2)
            {
                IptalEt(); return;
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
                    if (selectedCell.currentObject != null && selectedCell.currentObject != birlestirmeYoneticisi.sonUretilenObje)
                        Destroy(selectedCell.currentObject.gameObject);

                    selectedCell.currentObject = birlestirmeYoneticisi.sonUretilenObje;
                    yeniSabitObje = birlestirmeYoneticisi.sonUretilenObje;
                }

                if (yerdenMiAldik) YerdenOynamaBitti();
                else { selectedCell = null; previewObject = null; HazirlaYeniSpawn(); }
            }
            else IptalEt();
        }

        if (yeniSabitObje != null) birlestirmeYoneticisi.OtomatikKomsulukKontrolu(yeniSabitObje);
        GameManager.Instance.HamleBittiKontrolu();
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
        if (previewObject != null) Destroy(previewObject);

        currentPrefab = siradakiObjeVerisi.objePrefab; // Düzeltildi
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

            if (IsMouseOverPreview())
            {
                pressedOnPreview = true; 
            }

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Vector3Int cellPos = grid.WorldToCell(hit.point);
                GridCell cell = gridManager.GetCell(cellPos);

                if (!pressedOnPreview && cell != null && cell == selectedCell && !yerdenMiAldik)
                {
                    pressedOnPreview = true;
                }
                //Yerden obje alma mantığı
                else if (!pressedOnPreview && cell != null && !cell.IsEmpty() && !cell.currentObject.kilitliMi)
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
                        po.verisi = yerdekiGercekObje.verisi;
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
            if (!hasDragged && Vector2.Distance(Input.mousePosition, touchStartPos) >= dragThreshold) hasDragged = true;
            UpdatePreviewPosition();
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            if (selectedCell != null)
            {                
                if (!hasDragged && pressedOnPreview)
                {
                    Place();
                }
            }
            else 
            {
                IptalEt();
            }
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

                if (cell.IsEmpty()) secilebilir = true;
                else if (previewObject != null)
                {
                    var yerdeki = cell.currentObject;
                    // Hata kaynağı düzeltildi: previewPO null olabilir
                    var poComp = previewObject.GetComponent<PlaceableObject>();
                    ObjeVerisi elimizdekiVeri = null;
                    if (yerdenMiAldik && poComp != null) elimizdekiVeri = poComp.verisi;
                    else elimizdekiVeri = siradakiObjeVerisi;

                    if (yerdeki != null && elimizdekiVeri != null &&
                        yerdeki.verisi == elimizdekiVeri && !yerdeki.kilitliMi)
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
        previewObject.transform.position = grid.GetCellCenterWorld(cell.cellPosition) + Vector3.up * offset;
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

    private bool IsMouseOverPreview()
    {
        if (previewObject == null) return false;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        //Preview objesinin tüm görsel parçalarını (Renderer) bul
        foreach (Renderer r in previewObject.GetComponentsInChildren<Renderer>())
        {
            //Işın bu görselin kutusuna (Bounds) değiyor mu?
            if (r.bounds.IntersectRay(ray))
            {
                return true;
            }
        }
        return false;
    }
}

[System.Serializable]
public class SpriteEvent : UnityEvent<Sprite> { }