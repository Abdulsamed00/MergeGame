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
    
    // Input Değişkenleri
    private bool isDragging = false;
    private bool hasDragged = false;
    private Vector2 touchStartPos;
    private const float dragThreshold = 10f; 

    // Hareket / Taşıma Değişkenleri
    private GridCell kaynakHucre;
    private PlaceableObject yerdekiGercekObje;
    private bool yerdenMiAldik = false;
    private GridCell spawnOriginCell;
    
    private bool isInputLocked = false;

    private void Awake()
    {
        Instance = this;
    }

    public void SetInputLock(bool locked)
    {
        isInputLocked = locked;
        if (previewObject != null) previewObject.SetActive(!locked);
    }

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

    public void BeginPlacementAfterInitialSpawn()
    {
        SpawnYeniObje();
    }

    public void SpawnYeniObje()
    {
        HazirlaYeniSpawn();
    }

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

    private void UpdateNextUI()
    {
        if (OnNextObjectChanged != null && sonrakiObjeVerisi != null)
        {
            OnNextObjectChanged.Invoke(sonrakiObjeVerisi.uiIkonu);
        }
    }

    private ObjeVerisi GetWeightedRandomObject()
    {
        float toplamSans = 0;
        foreach (var item in mevcutLevelObjeleri) toplamSans += item.spawnYuzdesi;
        float rastgeleDeger = Random.Range(0, toplamSans);
        float suankiToplam = 0;
        foreach (var item in mevcutLevelObjeleri)
        {
            suankiToplam += item.spawnYuzdesi;
            if (rastgeleDeger <= suankiToplam) return item.obje;
        }
        return mevcutLevelObjeleri[0].obje;
    }

    void CreatePreview()
    {
        if (previewObject != null) Destroy(previewObject);
        if (currentPrefab == null) return;

        previewObject = Instantiate(currentPrefab);
        
        // Preview objesinin colliderlarını AÇIK bırakıyoruz ki tıklayabilelim!
        // Ama Raycast'i karıştırmaması için Layer ayarı yapılabilir, 
        // şimdilik basit Raycast mantığıyla devam ediyoruz.
        foreach (var col in previewObject.GetComponentsInChildren<Collider>()) col.enabled = false; 

        var po = previewObject.GetComponent<PlaceableObject>();
        if (po != null)
        {
            po.verisi = siradakiObjeVerisi;
            po.BoyutuGuncelle();
            po.SetPreviewMode(true); 
        }
    }

    void HandleInput()
    {
        // 1. MOUSE DOWN (TIKLAMA BAŞLANGICI)
        if (Input.GetMouseButtonDown(0))
        {
            // A) Eğer zaten seçili bir Preview varsa ve MOUSE ONUN ÜZERİNDEYSE -> ONAYLA (PLACE)
            if (previewObject != null && previewObject.activeSelf && IsMouseOverPreview())
            {
                Place();
                return; // Yerleştirme yapıldı, başka işlem yapma
            }

            // B) Değilse, yeni bir seçim veya sürükleme başlat
            isDragging = true;
            hasDragged = false;
            touchStartPos = Input.mousePosition;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            
            // Raycast ile hedef belirleme
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                PlaceableObject hitObject = hit.collider.GetComponentInParent<PlaceableObject>();
                GridCell cell = null;

                if (hitObject != null && hitObject.currentCell != null)
                {
                    cell = hitObject.currentCell;
                }
                else
                {
                    Vector3Int cellPos = grid.WorldToCell(hit.point);
                    cell = gridManager.GetCell(cellPos);
                }

                // Dolu bir hücreye tıkladıysak ve hareket hakkı varsa -> TAŞIMA BAŞLAT
                if (cell != null && !cell.IsEmpty() && cell.currentObject.hareketHakki > 0)
                {
                    // Eski preview varsa sil (Yeni bir şey seçiyoruz)
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
                }
                // Boş hücreye tıkladıysak -> Sadece Preview'ı oraya taşı (Yeni spawn için)
                else if (cell != null && cell.IsEmpty() && !yerdenMiAldik)
                {
                     SelectCell(cell);
                }
            }
        }

        // 2. MOUSE DRAG (SÜRÜKLEME)
        if (Input.GetMouseButton(0) && isDragging)
        {
            if (!hasDragged && Vector2.Distance(Input.mousePosition, touchStartPos) >= dragThreshold) hasDragged = true;
            
            // Sürüklerken Preview'ı güncelle
            UpdatePreviewPosition();
        }

        // 3. MOUSE UP (BIRAKMA)
        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            
            // BURASI DEĞİŞTİ: Artık MouseUp olduğunda Place() ÇAĞIRMIYORUZ.
            // Obje son bırakılan yerde "Preview" modunda bekliyor.
            
            // Sadece iptal durumu (Sürüklemeden tıkladık ama preview üstüne değil, boşluğa)
            // Bu durumda bir şey yapmamıza gerek yok, preview yeni yerde beklesin.
        }
    }

    void UpdatePreviewPosition()
    {
        if (yerdenMiAldik && !hasDragged && kaynakHucre != null)
        {
            SelectCell(kaynakHucre);
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        GridCell targetCell = null;

        // Önce objelere çarpıyor mu?
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            PlaceableObject hitObj = hit.collider.GetComponentInParent<PlaceableObject>();
            if (hitObj != null && hitObj.currentCell != null)
            {
                targetCell = hitObj.currentCell;
            }
        }

        // Çarpmadıysa zemine çarpıyor mu?
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
            bool secilebilir = false;
            
            // Menzil Kontrolü
            if (yerdenMiAldik && kaynakHucre != null)
            {
                int mesafeX = Mathf.Abs(targetCell.cellPosition.x - kaynakHucre.cellPosition.x);
                int mesafeZ = Mathf.Abs(targetCell.cellPosition.z - kaynakHucre.cellPosition.z);
                
                if (mesafeX + mesafeZ > 1) return; 
            }

            if (targetCell.IsEmpty()) 
            {
                secilebilir = true;
            }
            else if (previewObject != null)
            {
                if (yerdenMiAldik)
                {
                    var yerdeki = targetCell.currentObject;
                    if (yerdeki != null)
                    {
                        if (yerdeki.verisi == yerdekiGercekObje.verisi) secilebilir = true;
                    }
                }
                
                if (yerdenMiAldik && targetCell == kaynakHucre) secilebilir = true;
            }

            if (secilebilir) SelectCell(targetCell);
        }
    }

    void SelectCell(GridCell cell)
    {
        if (previewObject == null) return;
        selectedCell = cell;
        
        if (!isInputLocked) previewObject.SetActive(true);

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

    private bool IsMouseOverPreview()
    {
        if (previewObject == null) return false;
        
        // Preview objesinin colliderları kapalı olduğu için Renderer üzerinden Bound kontrolü yapıyoruz
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        foreach (Renderer r in previewObject.GetComponentsInChildren<Renderer>())
        {
            if (r.bounds.IntersectRay(ray)) return true;
        }
        return false;
    }

    public void ForceUpdatePreview()
    {
        if (previewObject != null) Destroy(previewObject);
        currentPrefab = siradakiObjeVerisi.objePrefab;
        CreatePreview();
        SelectFirstEmptyCell();
    }
    
    void Place()
    {
        if (selectedCell == null) return;

        bool hamleGecerliMi = true; 
        if (hamleGecerliMi && UndoManager.Instance != null)
        {
            UndoManager.Instance.SaveState();
        }

        // İptal (Kendi yerine bırakma)
        if (yerdenMiAldik && selectedCell == kaynakHucre)
        {
            if (UndoManager.Instance != null) UndoManager.Instance.RemoveLastState();
            IptalEt();
            return;
        }
        
        if (yerdenMiAldik && kaynakHucre != null)
        {
            kaynakHucre.currentObject = null;
        }

        PlaceableObject islemGorenObje = null;

        // A. BOŞ HÜCREYE YERLEŞTİRME
        if (selectedCell.IsEmpty())
        {
            if (yerdenMiAldik)
            {
                // Taşıma
                yerdekiGercekObje.gameObject.SetActive(true);
                yerdekiGercekObje.transform.position = previewObject.transform.position;
                yerdekiGercekObje.currentCell = selectedCell;
                selectedCell.currentObject = yerdekiGercekObje;
                
                yerdekiGercekObje.hareketHakki = 0; 
                yerdekiGercekObje.BoyutuGuncelle();
                yerdekiGercekObje.SetPreviewMode(false); // Normal moda dön
                
                islemGorenObje = yerdekiGercekObje;
                Destroy(previewObject);
                IslemTamamlandi(true); 
            }
            else
            {
                // Yeni Spawn
                GameObject obj = Instantiate(currentPrefab, previewObject.transform.position, Quaternion.identity);
                PlaceableObject po = obj.GetComponent<PlaceableObject>();
                po.verisi = siradakiObjeVerisi;
                po.hareketHakki = 1; 

                var previewPO = previewObject.GetComponent<PlaceableObject>();
                if (previewPO != null) po.icindekiMalzemeler = new List<ObjeVerisi>(previewPO.icindekiMalzemeler);

                po.BoyutuGuncelle();
                po.SetPreviewMode(false); // Normal moda dön
                po.currentCell = selectedCell;
                selectedCell.currentObject = po;
                islemGorenObje = po;

                Destroy(previewObject);
                IslemTamamlandi(true); 
            }
        }
        // B. DOLU HÜCREYE YERLEŞTİRME (MERGE)
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

            bool islemBasladi = birlestirmeYoneticisi.ManuelBirlestirme(elimizdekiObje, yerdekiObje);

            if (islemBasladi)
            {
                islemGorenObje = yerdekiObje;
                Destroy(previewObject);
                IslemTamamlandi(true); 
            }
            else
            {
                if (UndoManager.Instance != null) UndoManager.Instance.RemoveLastState();
                GeriAl();
            }
        }

        if (islemGorenObje != null && selectedCell.currentObject == islemGorenObje)
        {
            birlestirmeYoneticisi.OtomatikTarifKontrolu(islemGorenObje);
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

        if (yeniSpawnGerekli)
        {
            SpawnYeniObje();
        }
    }

    void GeriAl()
    {
        if (yerdenMiAldik && kaynakHucre != null)
        {
            kaynakHucre.currentObject = yerdekiGercekObje; 
            yerdekiGercekObje.gameObject.SetActive(true);
            if(previewObject != null) Destroy(previewObject);
            
            yerdenMiAldik = false;
            yerdekiGercekObje = null;
            kaynakHucre = null;
        }
        else
        {
            IptalEt();
        }
    }

    void IptalEt()
    {
        if (yerdenMiAldik)
        {
            //1. Yerdeki objeyi eski yerine geri koy
            if (kaynakHucre != null && kaynakHucre.currentObject == null) 
                kaynakHucre.currentObject = yerdekiGercekObje;

            //2. Objenin kendisini tekrar görünür yap
            if (yerdekiGercekObje != null) 
                yerdekiGercekObje.gameObject.SetActive(true);

            //3. Elimizdeki (Mouse ucundaki) preview'ı yok et
            if(previewObject != null) Destroy(previewObject);

            //4. Değişkenleri sıfırla
            yerdenMiAldik = false;
            yerdekiGercekObje = null;
            kaynakHucre = null;
            currentPrefab = siradakiObjeVerisi.objePrefab;
            CreatePreview();
            SelectFirstEmptyCell(); 
        }
        else
        {
            if (spawnOriginCell != null) SelectCell(spawnOriginCell);
        }
    }
}

[System.Serializable]
public class SpriteEvent : UnityEvent<Sprite> { }