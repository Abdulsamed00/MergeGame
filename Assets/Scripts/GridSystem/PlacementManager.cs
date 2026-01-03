using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public class PlacementManager : MonoBehaviour
{
    public Grid grid;
    public GridManager gridManager;
    public BirlestirmeYoneticisi birlestirmeYoneticisi;

    [Header("Spawn Sistemi")]
    public List<LevelSpawnVerisi> mevcutLevelObjeleri;

    // Kuyruk
    public ObjeVerisi siradakiObjeVerisi;
    public ObjeVerisi sonrakiObjeVerisi;

    [Header("UI Event")]
    public SpriteEvent OnNextObjectChanged;

    // --- DEĞİŞKENLER ---
    private GameObject currentPrefab;
    private GameObject previewObject; 
    
    // Toplama Sistemi (Gathering)
    private List<PlaceableObject> suruklenenObjeler = new List<PlaceableObject>();
    private bool isGatheringMode = false; 
    
    private Vector2 touchStartPos;
    
    // Sürükleme Hassasiyeti (Piksel cinsinden)
    private const float dragThreshold = 30f; 
    
    private bool isDragging = false;
    private bool hasDragged = false;
    
    // Onay Bekleyen (Havada asılı kalan) durum
    private bool isPendingConfirmation = false; 

    private GridCell selectedCell;
    private GridCell spawnOriginCell;

    void Update()
    {
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

    public void SpawnYeniObje()
    {
        if (mevcutLevelObjeleri == null || mevcutLevelObjeleri.Count == 0) return;

        siradakiObjeVerisi = sonrakiObjeVerisi;
        sonrakiObjeVerisi = GetWeightedRandomObject();
        UpdateNextUI();

        currentPrefab = siradakiObjeVerisi.objePrefab;
        CreateQueuePreview();
        SelectFirstEmptyCell();
        
        isPendingConfirmation = false;
    }

    // --- INPUT SİSTEMİ ---
    void HandleInput()
    {
        Vector3 mousePos = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        
        // Raycast Hedefleri
        GridCell targetCell = null;
        PlaceableObject hitObj = null;

        if (Physics.Raycast(ray, out RaycastHit hitObjInfo))
        {
            hitObj = hitObjInfo.collider.GetComponentInParent<PlaceableObject>();
        }

        Plane zemin = new Plane(Vector3.up, Vector3.zero);
        float enter;
        if (zemin.Raycast(ray, out enter))
        {
            Vector3 worldPoint = ray.GetPoint(enter);
            Vector3Int cellPos = grid.WorldToCell(worldPoint);
            targetCell = gridManager.GetCell(cellPos);
        }

        // ---------------------------------------------------------
        // 1. INPUT DOWN (DOKUNMA ANI)
        // ---------------------------------------------------------
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            hasDragged = false;
            touchStartPos = mousePos;

            // Gathering modunda değilsek
            if (!isGatheringMode)
            {
                // A) Yerdeki bir objeye mi dokunduk?
                if (hitObj != null && hitObj.currentCell != null && hitObj.gameObject != previewObject)
                {
                    BaslatGathering(hitObj);
                }
                // B) Boş bir hücreye mi dokunduk?
                else if (targetCell != null && targetCell.IsEmpty())
                {
                    if (!isPendingConfirmation)
                    {
                        // Sadece ilk defa sahneye koyuyorsak ışınla
                        SelectCell(targetCell);
                        isPendingConfirmation = true;
                    }
                }
            }
        }

        // ---------------------------------------------------------
        // 2. DRAGGING (SÜRÜKLEME)
        // ---------------------------------------------------------
        if (Input.GetMouseButton(0) && isDragging)
        {
            // Hassasiyet kontrolü
            if (!hasDragged && Vector2.Distance(mousePos, touchStartPos) >= dragThreshold) 
            {
                hasDragged = true;
                // Sürükleme kesinleştiği an onay modundan çıkabiliriz, artık pozisyon değişecek
                isPendingConfirmation = false; 
            }

            if (hasDragged || isGatheringMode) 
            {
                Vector3 worldPoint = Vector3.zero;
                if (zemin.Raycast(ray, out enter)) worldPoint = ray.GetPoint(enter);

                if (isGatheringMode)
                {
                    UpdateGatheringVisuals(worldPoint);
                    CheckForMoreGathering(ray); 
                }
                else
                {
                    // Normal Preview sadece sürükleme kesinleşince hareket eder
                    if (targetCell != null && targetCell.IsEmpty())
                    {
                        SelectCell(targetCell);
                    }
                }
            }
        }

        // ---------------------------------------------------------
        // 3. INPUT UP (BIRAKMA / ONAYLAMA)
        // ---------------------------------------------------------
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            
            if (isGatheringMode)
            {
                BitirGathering();
            }
            else
            {
                // --- NORMAL MOD ---
                
                if (hasDragged)
                {
                    // Sürükleyip bıraktıysak -> Beklemeye geç (Onaylama)
                    isPendingConfirmation = true; 
                }
                else
                {
                    // Sürüklemeden tıkladıysak (CLICK):
                    
                    if (isPendingConfirmation)
                    {
                        if (targetCell == selectedCell || IsMouseOverPreview())
                        {
                            PlaceNewObject();
                            isPendingConfirmation = false;
                        }
                        else
                        {
                            // Başka bir yere tıkladı, objeyi oraya ışınla ama yerleştirme
                            if (targetCell != null && targetCell.IsEmpty())
                            {
                                SelectCell(targetCell);
                            }
                        }
                    }
                    else
                    {
                        isPendingConfirmation = true;
                    }
                }
            }
        }
    }

    // --- TOPLAMA MANTIĞI ---

    void BaslatGathering(PlaceableObject ilkObje)
    {
        if (ilkObje.hareketHakki <= 0) return; 

        isGatheringMode = true;
        if (previewObject != null) previewObject.SetActive(false);
        AddToDraggingList(ilkObje);
        
        hasDragged = true; 
    }

    void CheckForMoreGathering(Ray ray)
    {
        if (suruklenenObjeler.Count >= 2) return;

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            PlaceableObject hitObj = hit.collider.GetComponentInParent<PlaceableObject>();
            
            if (hitObj != null && !suruklenenObjeler.Contains(hitObj) && hitObj.currentCell != null)
            {
                if (suruklenenObjeler.Count > 0 && birlestirmeYoneticisi.UygunMu(suruklenenObjeler[0], hitObj))
                {
                    AddToDraggingList(hitObj);
                }
            }
        }
    }

    void AddToDraggingList(PlaceableObject obj)
    {
        suruklenenObjeler.Add(obj);
        obj.SetPreviewMode(true);
        foreach(var col in obj.GetComponentsInChildren<Collider>()) col.enabled = false;
    }

    void UpdateGatheringVisuals(Vector3 targetPos)
    {
        for (int i = 0; i < suruklenenObjeler.Count; i++)
        {
            Vector3 offset = new Vector3(0, 0.5f + (i * 0.3f), 0);
            suruklenenObjeler[i].transform.position = Vector3.Lerp(suruklenenObjeler[i].transform.position, targetPos + offset, Time.deltaTime * 15f);
        }
    }

    void BitirGathering()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        PlaceableObject targetObj = null;

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            targetObj = hit.collider.GetComponentInParent<PlaceableObject>();
        }

        bool islemBasarili = false;

        if (targetObj != null && !suruklenenObjeler.Contains(targetObj) && targetObj.currentCell != null)
        {
            if (birlestirmeYoneticisi.CokluBirlestirme(suruklenenObjeler, targetObj))
            {
                islemBasarili = true;
            }
        }
        
        if (!islemBasarili) CancelGathering(); 
        else suruklenenObjeler.Clear(); 

        isGatheringMode = false;
        isPendingConfirmation = false;

        if (previewObject != null)
        {
            SelectFirstEmptyCell(); 
            previewObject.SetActive(true);
        }
    }

    void CancelGathering()
    {
        foreach (var obj in suruklenenObjeler)
        {
            if (obj == null) continue;
            
            // 1. Animator'ı kapat (Şu anki scale değerinde donar)
            obj.SetPreviewMode(false);
            
            // 2. Colliderları geri aç
            foreach(var col in obj.GetComponentsInChildren<Collider>()) col.enabled = true;

            // 3. Eski yerine geri ışınla
            if (obj.currentCell != null)
            {
                Vector3 cellPos = grid.GetCellCenterWorld(obj.currentCell.cellPosition);
                obj.transform.position = cellPos + Vector3.up * obj.heightOffset;
            }
            
            // 4. --- DÜZELTME BURADA ---
            // Animator kapandıktan sonra objenin boyutu bozuk kalmasın diye
            // kodla zorla doğru boyutu (normalScale veya buyukScale) geri yüklüyoruz.
            obj.BoyutuGuncelle(); 
        }
        suruklenenObjeler.Clear();
    }

    // --- NORMAL SPAWN FONKSİYONLARI ---

    void PlaceNewObject()
    {
        if (selectedCell == null) return;
        if (UndoManager.Instance != null) UndoManager.Instance.SaveState();

        GameObject obj = Instantiate(currentPrefab, previewObject.transform.position, Quaternion.identity);
        PlaceableObject po = obj.GetComponent<PlaceableObject>();
        
        po.verisi = siradakiObjeVerisi;
        po.hareketHakki = 1;
        po.currentCell = selectedCell;
        po.BoyutuGuncelle();
        po.SetPreviewMode(false);
        
        selectedCell.currentObject = po;
        birlestirmeYoneticisi.OtomatikTarifKontrolu(po);
        SpawnYeniObje(); 
    }

    void CreateQueuePreview()
    {
        if (previewObject != null) Destroy(previewObject);
        if (currentPrefab == null) return;

        previewObject = Instantiate(currentPrefab);
        foreach (var col in previewObject.GetComponentsInChildren<Collider>()) col.enabled = false;

        var po = previewObject.GetComponent<PlaceableObject>();
        if (po != null)
        {
            po.verisi = siradakiObjeVerisi;
            po.BoyutuGuncelle();
            po.SetPreviewMode(true);
        }
    }

    void SelectCell(GridCell cell)
    {
        selectedCell = cell;
        if (previewObject != null)
        {
            float offset = 0.5f;
            var po = previewObject.GetComponent<PlaceableObject>();
            if (po != null) offset = po.heightOffset;

            previewObject.transform.position = grid.GetCellCenterWorld(cell.cellPosition) + Vector3.up * offset;
            previewObject.SetActive(true);
        }
    }
    
    void SelectFirstEmptyCell()
    {
        GridCell firstEmpty = gridManager.GetFirstEmptyCell();
        if (firstEmpty != null) SelectCell(firstEmpty);
    }

    private ObjeVerisi GetWeightedRandomObject()
    {
        float toplamSans = 0;
        foreach (var item in mevcutLevelObjeleri) toplamSans += item.spawnYuzdesi;
        float rnd = Random.Range(0, toplamSans);
        float current = 0;
        foreach (var item in mevcutLevelObjeleri)
        {
            current += item.spawnYuzdesi;
            if (rnd <= current) return item.obje;
        }
        return mevcutLevelObjeleri[0].obje;
    }

    private void UpdateNextUI()
    {
        if (OnNextObjectChanged != null && sonrakiObjeVerisi != null)
            OnNextObjectChanged.Invoke(sonrakiObjeVerisi.uiIkonu);
    }
    
    private bool IsMouseOverPreview()
    {
        if (previewObject == null || !previewObject.activeSelf) return false;
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
        CreateQueuePreview();
        SelectFirstEmptyCell();
        isPendingConfirmation = false;
    }
}

[System.Serializable]
public class SpriteEvent : UnityEvent<Sprite> { }