using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public class PlacementManager : MonoBehaviour
{
    public static PlacementManager Instance;

    [Header("Referanslar")]
    public Grid grid;
    public GridManager gridManager;
    public BirlestirmeYoneticisi birlestirmeYoneticisi;

    [Header("Spawn Sistemi")]
    public List<LevelSpawnVerisi> mevcutLevelObjeleri;

    public ObjeVerisi siradakiObjeVerisi;
    public ObjeVerisi sonrakiObjeVerisi;

    [Header("UI Event")]
    public SpriteEvent OnNextObjectChanged;

    private GameObject currentPrefab;
    private GameObject previewObject;
    private GridCell selectedCell;
    private bool pressedOnPreview = false;
    private bool isDragging = false;
    private bool hasDragged = false;
    private Vector2 touchStartPos;
    private const float dragThreshold = 10f;

    private GridCell kaynakHucre;
    private PlaceableObject yerdekiGercekObje;
    private bool yerdenMiAldik = false;
    private GridCell spawnOriginCell;
    
    private bool isInputLocked = false;

    private void Awake() { Instance = this; }
    public void SetInputLock(bool locked) { isInputLocked = locked; if (previewObject != null) previewObject.SetActive(!locked); }

    void Update()
    {
        if (isInputLocked || GameManager.Instance.oyunBittiMi) return;
        HandleInput();
    }

    public void SetupSpawnList(List<LevelSpawnVerisi> gelenListe)
    {
        mevcutLevelObjeleri = gelenListe;
        siradakiObjeVerisi = GetWeightedRandomObject();
        sonrakiObjeVerisi = GetWeightedRandomObject();
        UpdateNextUI();
    }
    public void BeginPlacementAfterInitialSpawn() { SpawnYeniObje(); }
    public void SpawnYeniObje() { HazirlaYeniSpawn(); }

    void HazirlaYeniSpawn()
    {
        if (mevcutLevelObjeleri == null || mevcutLevelObjeleri.Count == 0) return;
        siradakiObjeVerisi = sonrakiObjeVerisi;
        sonrakiObjeVerisi = GetWeightedRandomObject();
        UpdateNextUI();

        currentPrefab = siradakiObjeVerisi.objePrefab;
        CreatePreview();
        SelectFirstEmptyCell();
    }
    private void UpdateNextUI() { if (OnNextObjectChanged != null && sonrakiObjeVerisi != null) OnNextObjectChanged.Invoke(sonrakiObjeVerisi.uiIkonu); }
    
    private ObjeVerisi GetWeightedRandomObject()
    {
        float toplamSans = 0;
        foreach (var item in mevcutLevelObjeleri) toplamSans += item.spawnYuzdesi;
        float rastgeleDeger = Random.Range(0, toplamSans);
        float suankiToplam = 0;
        foreach (var item in mevcutLevelObjeleri) { suankiToplam += item.spawnYuzdesi; if (rastgeleDeger <= suankiToplam) return item.obje; }
        return mevcutLevelObjeleri[0].obje;
    }

    void CreatePreview()
    {
        if (previewObject != null) Destroy(previewObject);
        if (currentPrefab == null) return;
        previewObject = Instantiate(currentPrefab);
        foreach (var col in previewObject.GetComponentsInChildren<Collider>()) col.enabled = false;
        var po = previewObject.GetComponent<PlaceableObject>();
        if (po != null) { po.verisi = siradakiObjeVerisi; po.BoyutuGuncelle(); po.SetPreviewMode(true); }
    }

    void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            hasDragged = false;
            pressedOnPreview = false;
            touchStartPos = Input.mousePosition;

            if (IsMouseOverPreview()) pressedOnPreview = true; 

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                PlaceableObject hitObject = hit.collider.GetComponentInParent<PlaceableObject>();
                GridCell cell = null;

                if (hitObject != null && hitObject.currentCell != null) cell = hitObject.currentCell;
                else
                {
                    Vector3Int cellPos = grid.WorldToCell(hit.point);
                    cell = gridManager.GetCell(cellPos);
                }

                if (!pressedOnPreview && cell != null && cell == selectedCell && !yerdenMiAldik)
                {
                    pressedOnPreview = true;
                }
                else if (!pressedOnPreview && cell != null && !cell.IsEmpty())
                {
                    if (cell.currentObject.hareketHakki > 0)
                    {
                        if (previewObject != null) Destroy(previewObject);

                        yerdenMiAldik = true;
                        kaynakHucre = cell;
                        yerdekiGercekObje = cell.currentObject;
                        currentPrefab = yerdekiGercekObje.verisi.objePrefab;
                        
                        kaynakHucre.currentObject = null;
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
            if (!pressedOnPreview) UpdatePreviewPosition();
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
                if (yerdenMiAldik || (hasDragged && pressedOnPreview) || (!hasDragged && pressedOnPreview)) 
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
        GridCell targetCell = null;

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            PlaceableObject hitObj = hit.collider.GetComponentInParent<PlaceableObject>();
            if (hitObj != null && hitObj.currentCell != null) targetCell = hitObj.currentCell;
        }

        if (targetCell == null)
        {
            Plane zemin = new Plane(Vector3.up, Vector3.zero);
            float enter;
            if (zemin.Raycast(ray, out enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                Vector3Int cellPos = grid.WorldToCell(hitPoint);
                targetCell = gridManager.GetCell(cellPos);
            }
        }

        if (targetCell != null)
        {
            if (yerdenMiAldik && kaynakHucre != null)
            {
                int mesafeX = Mathf.Abs(targetCell.cellPosition.x - kaynakHucre.cellPosition.x);
                int mesafeZ = Mathf.Abs(targetCell.cellPosition.z - kaynakHucre.cellPosition.z);
                if (mesafeX + mesafeZ > 1) return; 
            }

            bool secilebilir = false;

            if (targetCell.IsEmpty()) 
            {
                secilebilir = true;
            }
            else if (previewObject != null)
            {
                if (yerdenMiAldik && targetCell == kaynakHucre) 
                {
                    secilebilir = true;
                }
                else if (yerdenMiAldik)
                {
                    var yerdeki = targetCell.currentObject;
                    var elimizdeki = previewObject.GetComponent<PlaceableObject>();

                    if (yerdeki != null && elimizdeki != null)
                    {
                        if (BirlestirmeYoneticisi.Instance.CanMerge(elimizdeki, yerdeki))
                        {
                            secilebilir = true;
                        }
                        else
                        {
                            secilebilir = false; 
                        }
                    }
                }
                else
                {
                    secilebilir = false;
                }
            }

            if (secilebilir) SelectCell(targetCell);
        }
    }

    void Place()
    {
        if (selectedCell == null) return;
        if (UndoManager.Instance != null) UndoManager.Instance.SaveState();

        // --- DÜZELTME 1: AYNI YERE KOYMA (İPTAL) DURUMU ---
        if (yerdenMiAldik && selectedCell == kaynakHucre)
        {
            if (UndoManager.Instance != null) UndoManager.Instance.RemoveLastState();
            IptalEt(); // Burası artık Spawn'ı geri getirecek
            return;
        }
        
        if (yerdenMiAldik && kaynakHucre != null) kaynakHucre.currentObject = null;

        // A. BOŞ YERE KOYMA
        if (selectedCell.IsEmpty())
        {
            if (yerdenMiAldik)
            {
                // TAŞIMA İŞLEMİ (MOVE)
                yerdekiGercekObje.gameObject.SetActive(true);
                yerdekiGercekObje.transform.position = previewObject.transform.position;
                yerdekiGercekObje.currentCell = selectedCell;
                selectedCell.currentObject = yerdekiGercekObje;
                
                yerdekiGercekObje.hareketHakki = 0; 
                yerdekiGercekObje.BoyutuGuncelle();
                yerdekiGercekObje.SetPreviewMode(false);
                
                Destroy(previewObject);
                IslemTamamlandi(true); // Sıradaki Spawn Gelsin
            }
            else
            {
                // YENİ SPAWN İŞLEMİ
                GameObject obj = Instantiate(currentPrefab, previewObject.transform.position, Quaternion.identity);
                PlaceableObject po = obj.GetComponent<PlaceableObject>();
                po.verisi = siradakiObjeVerisi;
                po.hareketHakki = 1; 

                var previewPO = previewObject.GetComponent<PlaceableObject>();
                if (previewPO != null) po.icindekiMalzemeler = new List<ObjeVerisi>(previewPO.icindekiMalzemeler);

                po.BoyutuGuncelle();
                po.SetPreviewMode(false);
                po.currentCell = selectedCell;
                selectedCell.currentObject = po;
                po.PlaySpawnAnimation();

                if (po.verisi != null)
                {
                    if (CollectionManager.Instance != null) CollectionManager.Instance.ObjeAcildi(po.verisi.collectionID);
                    else { string key = "Collection_" + po.verisi.collectionID; if (PlayerPrefs.GetInt(key, 0) == 0) { PlayerPrefs.SetInt(key, 1); PlayerPrefs.Save(); } }
                }

                Destroy(previewObject);
                IslemTamamlandi(true); // Sıradaki Spawn Gelsin
            }
        }
        // B. DOLU YERE KOYMA
        else
        {
            if (!yerdenMiAldik) 
            {
                 if (UndoManager.Instance != null) UndoManager.Instance.RemoveLastState();
                 IptalEt();
                 return;
            }

            PlaceableObject yerdekiObje = selectedCell.currentObject; 
            PlaceableObject elimizdekiObje = yerdekiGercekObje; 
            
            bool birlestiMi = birlestirmeYoneticisi.ManuelBirlestirme(elimizdekiObje, yerdekiObje);

            if (birlestiMi)
            {
                Destroy(yerdekiGercekObje.gameObject);
                Destroy(previewObject);
                IslemTamamlandi(true); // Birleşti, yeni spawn gelsin
            }
            else
            {
                // Birleşme başarısızsa eski yerine dön (Bu da bir nevi iptaldir)
                if (UndoManager.Instance != null) UndoManager.Instance.RemoveLastState();
                GeriAl();
            }
        }
        GameManager.Instance.HamleBittiKontrolu();
    }

    void IslemTamamlandi(bool yeniSpawnGerekli)
    {
        yerdenMiAldik = false;
        yerdekiGercekObje = null;
        kaynakHucre = null;
        selectedCell = null;
        previewObject = null;
        
        // Eğer hareket ettiyse veya birleştiyse yeni obje spawn et
        if (yeniSpawnGerekli) SpawnYeniObje();
    }

    void GeriAl()
    {
        // Eski yerine geri koyma işlemi
        if (yerdenMiAldik && kaynakHucre != null)
        {
            kaynakHucre.currentObject = yerdekiGercekObje; 
            yerdekiGercekObje.gameObject.SetActive(true);
            if(previewObject != null) Destroy(previewObject);
            
            yerdenMiAldik = false;
            yerdekiGercekObje = null;
            kaynakHucre = null;
            
            // --- DÜZELTME: GERİ ALININCA SPAWN TEKRAR GÖZÜKMELİ ---
            // Oyuncu başarısız bir hamle yaptı ve obje eski yerine döndü.
            // Bu durumda elindeki "Sıradaki Obje" (Spawn Preview) geri gelmeli.
            ForceUpdatePreview();
        }
        else 
        {
            IptalEt();
        }
    }

    void IptalEt()
    {
        // 1. Yerden aldığımız bir şey varsa yerine bırak
        if (yerdenMiAldik)
        {
            if (kaynakHucre != null && kaynakHucre.currentObject == null) kaynakHucre.currentObject = yerdekiGercekObje;
            yerdekiGercekObje.gameObject.SetActive(true);
            if(previewObject != null) Destroy(previewObject);

            yerdenMiAldik = false;
            yerdekiGercekObje = null;
            kaynakHucre = null;
            
            // --- DÜZELTME: İPTAL EDİLİNCE SPAWN TEKRAR GÖZÜKMELİ ---
            // Oyuncu taşıdığı objeyi aynı yere bıraktı (vazgeçti).
            // O zaman oyun "Spawn Modu"na geri dönmeli ve sıradaki objeyi göstermeli.
            ForceUpdatePreview(); 
        }
        // 2. Yeni spawn edilecek objeyi sürüklüyorsak ve boşluğa bıraktıysak
        else 
        {
            if (spawnOriginCell != null) SelectCell(spawnOriginCell);
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
        if (firstEmpty != null) { spawnOriginCell = firstEmpty; SelectCell(firstEmpty); }
    }
    
    private bool IsMouseOverPreview()
    {
        if (previewObject == null) return false;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        foreach (Renderer r in previewObject.GetComponentsInChildren<Renderer>()) if (r.bounds.IntersectRay(ray)) return true;
        return false;
    }
    
    // Preview'ı zorla yenileme (İptal durumları için)
    public void ForceUpdatePreview() 
    { 
        if (previewObject != null) Destroy(previewObject); 
        currentPrefab = siradakiObjeVerisi.objePrefab; 
        CreatePreview(); 
        SelectFirstEmptyCell(); 
    }
}

[System.Serializable]
public class SpriteEvent : UnityEvent<Sprite> { }