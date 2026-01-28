using UnityEngine;
using System.Collections;
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

    // Kuyruk
    public ObjeVerisi siradakiObjeVerisi;
    public ObjeVerisi sonrakiObjeVerisi;

    [Header("Animasyon Zamanlaması")]
    public float spawnGecikmesi = 0.4f;

    [Header("UI Event")]
    public SpriteEvent OnNextObjectChanged;

    [Header("Tutorial Ayarı")]
    public bool tutorialModuAktif = false; // SimpleTutorialManager bunu TRUE yapar

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
        // Oyun bittiyse veya kilitliyse dur
        if (isInputLocked || GameManager.Instance.oyunBittiMi)
        {
            return;
        }
        
        // Normal modda elin boşsa (preview yoksa) tıklayamazsın.
        // AMA Tutorial modundaysak elin boş olsa bile tıklayabilmelisin (yerdekileri taşımak için).
        if (!tutorialModuAktif && previewObject == null) 
        {
            return;
        }

        HandleInput();
    }

    // --- SETUP VE BAŞLANGIÇ ---
    public void SetupSpawnList(List<LevelSpawnVerisi> gelenListe)
    {
        mevcutLevelObjeleri = gelenListe;
        if (siradakiObjeVerisi == null)
        {
            siradakiObjeVerisi = GetWeightedRandomObject();
            sonrakiObjeVerisi = GetWeightedRandomObject();
            UpdateNextUI();
        }
    }

    public void LoadSpawnState(ObjeVerisi current, ObjeVerisi next)
    {
        siradakiObjeVerisi = current;
        sonrakiObjeVerisi = next;
        
        UpdateNextUI();
        
        if (previewObject != null) Destroy(previewObject);
        
        currentPrefab = siradakiObjeVerisi.objePrefab;
        CreatePreview();
        SelectFirstEmptyCell();
    }

    public void BeginPlacementAfterInitialSpawn()
    {
        SpawnYeniObje();
    }

    public void SpawnYeniObje()
    {
        if (currentPrefab != null)
        {
             if (previewObject == null) CreatePreview();
             SelectFirstEmptyCell();
             return; 
        }
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
        // Tutorial aktifse Preview/Hayalet oluşturma
        if (tutorialModuAktif) return; 

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
        if (UndoManager.Instance != null) UndoManager.Instance.SaveState();

        if (yerdenMiAldik && selectedCell == kaynakHucre)
        {
            if (UndoManager.Instance != null) UndoManager.Instance.RemoveLastState();
            IptalEt();
            return;
        }
        
        if (yerdenMiAldik && kaynakHucre != null) kaynakHucre.currentObject = null;

        // A. BOŞ YERE KOYMA
        if (selectedCell.IsEmpty())
        {
            if (yerdenMiAldik)
            {
                yerdekiGercekObje.gameObject.SetActive(true);
                
                // --- POZİSYON HESAPLAMA (Null Check Eklendi) ---
                if (previewObject != null)
                {
                    yerdekiGercekObje.transform.position = previewObject.transform.position;
                }
                else
                {
                    // Preview yoksa (Tutorial modu) direkt hücre merkezine koy
                    yerdekiGercekObje.transform.position = grid.GetCellCenterWorld(selectedCell.cellPosition) + Vector3.up * yerdekiGercekObje.heightOffset;
                }
                // -----------------------------------------------

                yerdekiGercekObje.currentCell = selectedCell;
                selectedCell.currentObject = yerdekiGercekObje;
                
                // Tutorial modundaysak hareket hakkını yeme (tekrar taşıyabilsin)
                if (!tutorialModuAktif) yerdekiGercekObje.hareketHakki = 0; 

                yerdekiGercekObje.BoyutuGuncelle();
                yerdekiGercekObje.SetPreviewMode(false);
                
                if(previewObject != null) Destroy(previewObject);
                IslemTamamlandi(false); 
                
                // Tutorial kontrolü
                if (SimpleTutorialManager.Instance != null) SimpleTutorialManager.Instance.CheckTutorialStatus();
                else GameManager.Instance.HamleBittiKontrolu();
            }
            else
            {
                // Normal oyun (Spawn)
                GameObject obj = Instantiate(currentPrefab, previewObject.transform.position, currentPrefab.transform.rotation);
                PlaceableObject po = obj.GetComponent<PlaceableObject>();

                po.verisi = siradakiObjeVerisi;
                po.hareketHakki = 1; 

                var previewPO = previewObject.GetComponent<PlaceableObject>();
                if (previewPO != null) po.icindekiMalzemeler = new List<ObjeVerisi>(previewPO.icindekiMalzemeler);

                po.BoyutuGuncelle();
                po.SetPreviewMode(false); 
                
                po.currentCell = selectedCell;
                selectedCell.currentObject = po;

                if (po.verisi != null)
                {
                    if (CollectionManager.Instance != null) CollectionManager.Instance.ObjeAcildi(po.verisi.collectionID);
                    else { string key = "Collection_" + po.verisi.collectionID; if (PlayerPrefs.GetInt(key, 0) == 0) { PlayerPrefs.SetInt(key, 1); PlayerPrefs.Save(); } }
                }

                Destroy(previewObject);
                IslemTamamlandi(true); 
                GameManager.Instance.HamleBittiKontrolu();
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
                if(previewObject != null) Destroy(previewObject);
                
                IslemTamamlandi(true);
                
                // Tutorial kontrolü
                if (SimpleTutorialManager.Instance != null) SimpleTutorialManager.Instance.CheckTutorialStatus();
                else GameManager.Instance.HamleBittiKontrolu();
            }
            else
            {
                if (UndoManager.Instance != null) UndoManager.Instance.RemoveLastState();
                GeriAl();
            }
        }
    }

    void IslemTamamlandi(bool yeniSpawnGerekli)
    {
        yerdenMiAldik = false;
        yerdekiGercekObje = null;
        kaynakHucre = null;
        selectedCell = null;
        previewObject = null;
        StartCoroutine(GecikmeliSpawnRoutine(yeniSpawnGerekli));
    }

    IEnumerator GecikmeliSpawnRoutine(bool yeniSpawnGerekli)
    {
        yield return new WaitForSeconds(spawnGecikmesi);

        // Tutorial modundaysak yeni taş verme döngüsünü kır
        if (tutorialModuAktif) yield break; 

        if (yeniSpawnGerekli)
        {
            currentPrefab = null; 
            SpawnYeniObje();
        }
        else
        {
            if(currentPrefab == null && siradakiObjeVerisi != null)
            {
                currentPrefab = siradakiObjeVerisi.objePrefab;
            }
            
            if (currentPrefab != null)
            {
                CreatePreview();
                SelectFirstEmptyCell();
            }
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
            
            if (!tutorialModuAktif) ForceUpdatePreview();
        }
        else IptalEt();
    }

    void IptalEt()
    {
        if (yerdenMiAldik)
        {
            if (kaynakHucre != null && kaynakHucre.currentObject == null) kaynakHucre.currentObject = yerdekiGercekObje;
            yerdekiGercekObje.gameObject.SetActive(true);
            if(previewObject != null) Destroy(previewObject);
            yerdenMiAldik = false;
            yerdekiGercekObje = null;
            kaynakHucre = null;
            if (!tutorialModuAktif) ForceUpdatePreview(); 
        }
        else if (spawnOriginCell != null) SelectCell(spawnOriginCell);
    }

    void HandleInput()
    {
        // --- GÜVENLİK KONTROLLERİ ---
        if (Camera.main == null) return; 
        if (grid == null) return;
        if (gridManager == null) return;
        // ----------------------------

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
                    // Hücre dolu görünüyor ama obje yoksa çık
                    if (cell.currentObject == null) return; 

                    // Tutorial'da hareket hakkı kontrolü (genelde 1 olur)
                    if (cell.currentObject.hareketHakki > 0)
                    {
                        if (previewObject != null) Destroy(previewObject);

                        yerdenMiAldik = true;
                        kaynakHucre = cell;
                        yerdekiGercekObje = cell.currentObject;
                        currentPrefab = yerdekiGercekObje.verisi.objePrefab;
                        
                        kaynakHucre.currentObject = null;
                        
                        // --- TUTORIAL GÖRÜNÜRLÜK AYARI ---
                        // Eğer tutorial modundaysak ve preview oluşturmuyorsak, gerçek objeyi gizleme!
                        // Yoksa elimizdeki obje kaybolur.
                        if (!tutorialModuAktif)
                        {
                            yerdekiGercekObje.gameObject.SetActive(false);
                            CreatePreview(); // Normal modda preview oluştur
                        }
                        else
                        {
                            // Tutorial modunda preview oluşturmuyoruz ve objeyi açık bırakıyoruz
                            // ki nereye gittiğini görelim (sürükleme efekti yoksa bile)
                        }
                        // ----------------------------------

                        // Preview varsa onun verilerini ayarla
                        if (previewObject != null)
                        {
                            var po = previewObject.GetComponent<PlaceableObject>();
                            po.verisi = yerdekiGercekObje.verisi;
                            po.icindekiMalzemeler = new List<ObjeVerisi>(yerdekiGercekObje.icindekiMalzemeler);
                            po.BoyutuGuncelle();
                        }

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

            if (targetCell.IsEmpty()) secilebilir = true;
            else if (previewObject != null)
            {
                if (yerdenMiAldik && targetCell == kaynakHucre) secilebilir = true;
                else if (yerdenMiAldik)
                {
                    var yerdeki = targetCell.currentObject;
                    var elimizdeki = previewObject.GetComponent<PlaceableObject>();
                    if (yerdeki != null && elimizdeki != null)
                    {
                        if (BirlestirmeYoneticisi.Instance.CanMerge(elimizdeki, yerdeki)) secilebilir = true;
                        else secilebilir = false; 
                    }
                }
                else secilebilir = false;
            }
            // Tutorial modunda preview yoksa sadece boş yerleri veya birleşmeleri mantıken kontrol et
            else if (tutorialModuAktif && yerdenMiAldik)
            {
                 if (targetCell.IsEmpty()) secilebilir = true;
                 else if (targetCell.currentObject != null && yerdekiGercekObje != null)
                 {
                     // Preview yok ama gerçek objeler üzerinden kontrol
                     if (BirlestirmeYoneticisi.Instance.CanMerge(yerdekiGercekObje, targetCell.currentObject))
                        secilebilir = true;
                     else
                        secilebilir = false;
                 }
            }

            if (secilebilir) SelectCell(targetCell);
        }
    }

    void SelectCell(GridCell cell)
    {
        selectedCell = cell;
        
        // Preview varsa onu taşı, yoksa sadece selectedCell'i güncelle
        if (previewObject != null)
        {
            previewObject.SetActive(true);
            float offset = 0.5f;
            var po = previewObject.GetComponent<PlaceableObject>();
            if (po != null) offset = po.heightOffset;
            previewObject.transform.position = grid.GetCellCenterWorld(cell.cellPosition) + Vector3.up * offset;
        }
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