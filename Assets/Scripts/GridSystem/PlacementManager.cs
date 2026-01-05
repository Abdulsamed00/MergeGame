using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public class PlacementManager : MonoBehaviour
{
    public Grid grid;
    public GridManager gridManager;
    public BirlestirmeYoneticisi birlestirmeYoneticisi;

    [Header("Spawn Sistemi")]
    public List<LevelSpawnVerisi> mevcutLevelObjeleri; // Yeni sistemin (LevelSpawnVerisi)

    // Kuyruk
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

    void Update()
    {
        HandleInput();
    }

    // --- SETUP VE BAŞLANGIÇ ---
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
            if (rastgeleDeger <= suankiToplam) 
            {
                return item.obje;
            }
        }
        return mevcutLevelObjeleri[0].obje;
    }

    void CreatePreview()
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

    void Place()
    {
        if (selectedCell == null) return;

        // --- UNDO SİSTEMİ ---
        bool hamleGecerliMi = false;
        if (selectedCell.IsEmpty()) hamleGecerliMi = true;
        else hamleGecerliMi = true; 

        if (hamleGecerliMi)
        {
            if (UndoManager.Instance != null) UndoManager.Instance.SaveState();
        }
        // --------------------

        // İPTAL DURUMU: Yerden aldığımızı aynı yere koyduysak
        if (yerdenMiAldik && selectedCell == kaynakHucre)
        {
            if (UndoManager.Instance != null) UndoManager.Instance.RemoveLastState();
            IptalEt();
            return;
        }
        
        // Eski yeri temizle
        if (yerdenMiAldik && kaynakHucre != null)
        {
            kaynakHucre.currentObject = null;
        }

        PlaceableObject islemGorenObje = null;

        // A. BOŞ YERE KOYMA
        if (selectedCell.IsEmpty())
        {
            if (yerdenMiAldik)
            {
                // --- TAŞIMA ---
                yerdekiGercekObje.gameObject.SetActive(true);
                yerdekiGercekObje.transform.position = previewObject.transform.position;
                
                yerdekiGercekObje.currentCell = selectedCell;
                selectedCell.currentObject = yerdekiGercekObje;
                
                // Hareket ettirdiğimiz için hakkı bitti (0)
                yerdekiGercekObje.hareketHakki = 0; 

                yerdekiGercekObje.BoyutuGuncelle();
                yerdekiGercekObje.SetPreviewMode(false);
                
                islemGorenObje = yerdekiGercekObje;
                
                Destroy(previewObject);
                IslemTamamlandi(true); 
            }
            else
            {
                // --- YENİ SPAWN KOYMA ---
                GameObject obj = Instantiate(currentPrefab, previewObject.transform.position, Quaternion.identity);
                PlaceableObject po = obj.GetComponent<PlaceableObject>();

                po.verisi = siradakiObjeVerisi;

                // --- DÜZELTME BURADA ---
                // Yeni doğan objenin 1 hareket hakkı olsun istiyoruz.
                po.hareketHakki = 1; 

                var previewPO = previewObject.GetComponent<PlaceableObject>();
                if (previewPO != null)
                {
                    po.icindekiMalzemeler = new List<ObjeVerisi>(previewPO.icindekiMalzemeler);
                }

                // HATA BURADAYDI: po.hareketHakki = 0; satırını SİLDİM.
                // Artık hakkı 1 olarak kalacak.

                po.BoyutuGuncelle();
                po.SetPreviewMode(false);
                
                po.currentCell = selectedCell;
                selectedCell.currentObject = po;

                islemGorenObje = po;

                Destroy(previewObject);
                IslemTamamlandi(true); 
            }
        }
        // B. DOLU YERE KOYMA (MANUEL BİRLEŞTİRME)
        else
        {
            if (!yerdenMiAldik) 
            {
                // Yeni spawn edilen obje (Preview) dolu yere konamaz (veya birleşemez) kuralı varsa burası çalışır.
                // Eğer yeni spawn'ın da birleşmesini istiyorsan burayı açabiliriz ama
                // şimdilik senin attığın koda sadık kalıyorum.
                if (UndoManager.Instance != null) UndoManager.Instance.RemoveLastState();
                IptalEt(); 
                return;
            }

            PlaceableObject yerdekiObje = selectedCell.currentObject; 
            PlaceableObject elimizdekiObje = yerdekiGercekObje;       

            bool birlestiMi = birlestirmeYoneticisi.ManuelBirlestirme(elimizdekiObje, yerdekiObje);

            if (birlestiMi)
            {
                islemGorenObje = yerdekiObje;

                Destroy(yerdekiGercekObje.gameObject);
                Destroy(previewObject);

                IslemTamamlandi(true); 
            }
            else
            {
                if (UndoManager.Instance != null) UndoManager.Instance.RemoveLastState();
                GeriAl();
            }
        }

        // --- OTOMATİK KONTROL ---
        if (islemGorenObje != null)
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
            if (kaynakHucre != null && kaynakHucre.currentObject == null) 
                kaynakHucre.currentObject = yerdekiGercekObje;

            yerdekiGercekObje.gameObject.SetActive(true);
            if(previewObject != null) Destroy(previewObject);

            yerdenMiAldik = false;
            yerdekiGercekObje = null;
            kaynakHucre = null;
        }
        else
        {
            if (spawnOriginCell != null) SelectCell(spawnOriginCell);
        }
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

                if (hitObject != null && hitObject.currentCell != null)
                {
                    cell = hitObject.currentCell;
                }
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
                    // BURASI ÇOK ÖNEMLİ: Objenin hareket hakkı var mı?
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
                if (!hasDragged && pressedOnPreview) Place();
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
            if (hitObj != null && hitObj.currentCell != null)
            {
                targetCell = hitObj.currentCell;
            }
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
            bool secilebilir = false;
            
            // 1 Birim Hareket Kısıtlaması
            if (yerdenMiAldik && kaynakHucre != null)
            {
                int mesafeX = Mathf.Abs(targetCell.cellPosition.x - kaynakHucre.cellPosition.x);
                int mesafeZ = Mathf.Abs(targetCell.cellPosition.z - kaynakHucre.cellPosition.z);
                
                if (mesafeX + mesafeZ > 1) 
                {
                    return; 
                }
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
                        if (yerdeki.verisi == yerdekiGercekObje.verisi)
                        {
                            secilebilir = true;
                        }
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

    private bool IsMouseOverPreview()
    {
        if (previewObject == null) return false;
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
    
}

[System.Serializable]
public class SpriteEvent : UnityEvent<Sprite> { }