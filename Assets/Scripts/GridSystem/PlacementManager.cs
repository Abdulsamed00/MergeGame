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
            // Preview'a veri atamayı unutmuyoruz
            po.verisi = siradakiObjeVerisi;
            po.BoyutuGuncelle();
            po.SetPreviewMode(true);
        }
    }

    void Place()
    {
        if (selectedCell == null) return;

        // --- UNDO SİSTEMİ İÇİN EKLEME ---
        // Eğer geçerli bir hamle yapıyorsak durumu kaydedelim.
        // (Basit kontrol: ya boş yere koyuyoruzdur ya da dolu yerle etkileşime giriyoruzdur)
        bool hamleGecerliMi = false;
    
        if (selectedCell.IsEmpty()) hamleGecerliMi = true;
        else 
        {
            // Dolu hücre kontrolü (senin kodundaki mantığın aynısı)
            var yerdeki = selectedCell.currentObject;
            var eldeki = siradakiObjeVerisi; // Basit referans
            if (yerdenMiAldik) eldeki = yerdekiGercekObje.verisi; // Yerden aldıysak farklı

            // Burada detaylı "merge olabilir mi" kontrolü yapmak yerine
            // En basit yöntem: Şimdilik kaydet, hamle başarısız olursa geri sileriz
            // Ama snapshot ucuz olduğu için direkt kaydedelim:
            hamleGecerliMi = true; 
        }

        if (hamleGecerliMi)
        {
            // Eğer sahnede UndoManager varsa kaydet
            if (UndoManager.Instance != null) UndoManager.Instance.SaveState();
        }
        // -------------------------------

        // ... Senin mevcut kodların buradan devam ediyor ...
        if (yerdenMiAldik && selectedCell == kaynakHucre)
        {
            // İPTAL DURUMU: Eğer oyuncu taşı kaldırıp aynı yere geri koyduysa
            // bu bir hamle sayılmaz. Stack'ten son kaydı silelim.
            if (UndoManager.Instance != null) UndoManager.Instance.RemoveLastState(); // (Bu fonksiyonu aşağıda veriyorum)
        
            IptalEt();
            return;
        }
        
        if (selectedCell == null) return;
        PlaceableObject islemGorenObje = null; // Hangi objeyi koyduk/hareket ettirdik?

        if (yerdenMiAldik && selectedCell == kaynakHucre)
        {
            IptalEt();
            return;
        }

        // Eski yeri temizle
        if (yerdenMiAldik && kaynakHucre != null)
        {
            kaynakHucre.currentObject = null;
        }

        if (selectedCell.IsEmpty())
        {
            if (yerdenMiAldik)
            {
                // TAŞIMA
                yerdekiGercekObje.gameObject.SetActive(true);
                yerdekiGercekObje.transform.position = previewObject.transform.position;
                
                yerdekiGercekObje.currentCell = selectedCell;
                selectedCell.currentObject = yerdekiGercekObje;
                yerdekiGercekObje.hareketHakki = 0; 

                yerdekiGercekObje.BoyutuGuncelle();
                yerdekiGercekObje.SetPreviewMode(false);
                
                islemGorenObje = yerdekiGercekObje; // Referansı tut
                
                Destroy(previewObject);
                IslemTamamlandi(true); 
            }
            else
            {
                // YENİ SPAWN
                GameObject obj = Instantiate(currentPrefab, previewObject.transform.position, Quaternion.identity);
                PlaceableObject po = obj.GetComponent<PlaceableObject>();

                // Veriyi aktar
                po.verisi = siradakiObjeVerisi;
                po.hareketHakki = 1; 

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

                islemGorenObje = po; // Referansı tut

                Destroy(previewObject);
                IslemTamamlandi(true); 
            }
        }
        // B. DOLU YERE KOYMA (MANUEL BİRLEŞTİRME)
        else
        {
            if (!yerdenMiAldik) 
            {
                IptalEt(); 
                return;
            }

            PlaceableObject yerdekiObje = selectedCell.currentObject; 
            PlaceableObject elimizdekiObje = yerdekiGercekObje;       

            bool birlestiMi = birlestirmeYoneticisi.ManuelBirlestirme(elimizdekiObje, yerdekiObje);

            if (birlestiMi)
            {
                // Manuel birleşme olduysa, son oluşan obje "yerdekiObje"dir (çünkü elimizdekini onun içine ekledik)
                islemGorenObje = yerdekiObje;

                Destroy(yerdekiGercekObje.gameObject);
                Destroy(previewObject);

                IslemTamamlandi(true); 
            }
            else
            {
                GeriAl();
                return; // Geri alındıysa otomatik kontrol yapma
            }
        }

        // --- İŞTE SİHİRLİ DOKUNUŞ BURADA ---
        // Hamle bitti, şimdi etrafı kontrol et: "Yan yana gelenlerle bir şey oluşuyor mu?"
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
                // Önce objeyi kontrol et
                PlaceableObject hitObject = hit.collider.GetComponentInParent<PlaceableObject>();
                GridCell cell = null;

                if (hitObject != null && hitObject.currentCell != null)
                {
                    cell = hitObject.currentCell;
                }
                else
                {
                    // Obje yoksa zemini kontrol et
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

        // 1. Raycast
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

        // 2. Kontroller ve Hareket Kısıtlaması
        if (targetCell != null)
        {
            bool secilebilir = false;
            
            // --- BURASI DEĞİŞTİ: 1 BİRİM HAREKET KISITLAMASI ---
            if (yerdenMiAldik && kaynakHucre != null)
            {
                int mesafeX = Mathf.Abs(targetCell.cellPosition.x - kaynakHucre.cellPosition.x);
                int mesafeZ = Mathf.Abs(targetCell.cellPosition.z - kaynakHucre.cellPosition.z);
                
                // Manhattan Mesafesi: Sadece sağ-sol-ön-arka (Toplam 1 birim)
                if (mesafeX + mesafeZ > 1) 
                {
                    return; // 1 birimden uzağa gidemez
                }
            }

            if (targetCell.IsEmpty()) 
            {
                secilebilir = true;
            }
            else if (previewObject != null)
            {
                // Eğer doluysa, üzerine gelip birleştirebiliyor muyuz?
                if (yerdenMiAldik)
                {
                    var yerdeki = targetCell.currentObject;
                    if (yerdeki != null)
                    {
                        // Sadece aynı türler üst üste gelebilir (Birleşme ihtimali için)
                        if (yerdeki.verisi == yerdekiGercekObje.verisi)
                        {
                            secilebilir = true;
                        }
                    }
                }
                
                // Kendi yerimiz ise seçilebilir
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