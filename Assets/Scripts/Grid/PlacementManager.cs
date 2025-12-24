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

    void Start()
    {
        // Oyuna başlarken rastgele bir obje ile başla
        SpawnYeniObje();
    }

    void Update()
    {
        HandleInput();
    }

    // --- RANDOM OBJE SEÇME SİSTEMİ ---
    void SpawnYeniObje()
    {
        if (spawnlanabilirObjeler.Count == 0)
        {
            Debug.LogError("Komutanım, Spawn listesi boş! Lütfen Inspector'dan obje ekleyin.");
            return;
        }

        // Listeden rastgele bir sayı seç (0 ile Liste sayısı arası)
        int rastgeleSayi = Random.Range(0, spawnlanabilirObjeler.Count);
        ObjeVerisi secilenVeri = spawnlanabilirObjeler[rastgeleSayi];

        // Elimizdeki prefabı bu seçilen yap
        currentPrefab = secilenVeri.objePrefab;

        // Preview oluştur ve sol alta veya ilk boş yere koy
        CreatePreview();
        SelectFirstEmptyCell();
    }

    void CreatePreview()
    {
        // Eski preview varsa temizle
        if (previewObject != null) Destroy(previewObject);

        previewObject = Instantiate(currentPrefab);
        
        // Raycast çarpmasın diye colliderları kapat
        foreach (var col in previewObject.GetComponentsInChildren<Collider>()) col.enabled = false;
        
        // Varsa animasyon modunu aç
        var po = previewObject.GetComponent<PlaceableObject>();
        if (po != null) po.SetPreviewMode(true);
    }

    // --- YERLEŞTİRME VE DÖNGÜ KONTROLÜ ---
    void Place()
    {
        if (selectedCell == null || !selectedCell.IsEmpty()) return;

        //Obje sahneye kalıcı olarak konur
        GameObject obj = Instantiate(currentPrefab, previewObject.transform.position, Quaternion.identity);
        PlaceableObject po = obj.GetComponent<PlaceableObject>();

        // preview üzerindeki veriyi gerçek objeye aktar
        var previewPO = previewObject.GetComponent<PlaceableObject>();
        if (previewPO != null) po.verisi = previewPO.verisi;

        po.SetPreviewMode(false);
        po.currentCell = selectedCell;
        selectedCell.currentObject = po;

        // Birleşme Kontrolü Yapılır
        birlestirmeYoneticisi.BirlestirmeKontrol(selectedCell.cellPosition.x, selectedCell.cellPosition.z, po);

        // DÖNGÜ KARARI
        if (birlestirmeYoneticisi.sonUretilenObje != null)
        {
            // --- BİRLEŞME OLDU ---
            // İnşaat alanı oluştu şimdi onu kontrol edip bir yere koymamız gerekiyo
            
            // Eski previewı imleci sil
            Destroy(previewObject);

            // Yeni oluşan objeyi İnşaat Alanı elimize alıyoruz
            previewObject = birlestirmeYoneticisi.sonUretilenObje.gameObject;
            
            // Bir sonraki tıkta bu objeyi koyması için prefabı güncelle
            currentPrefab = birlestirmeYoneticisi.sonUretilenObje.verisi.objePrefab;

            // Seçili hücrede oyuncunun hareket ettirmesini bekle
            SelectCell(selectedCell);
        }
        else
        {
           // birleşme olmadı - Normal Hamle
            selectedCell = null;
            previewObject.SetActive(false);
            
            SpawnYeniObje();
        }
    }
    
    void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            UpdatePreviewPosition();
        }
        if (Input.GetMouseButton(0) && isDragging)
        {
            UpdatePreviewPosition();
        }
        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            Place();
        }
    }

    void UpdatePreviewPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3Int cellPos = grid.WorldToCell(hit.point);
            GridCell cell = gridManager.GetCell(cellPos);
            if (cell != null && cell.IsEmpty()) SelectCell(cell);
        }
    }

    void SelectCell(GridCell cell)
    {
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
        if (firstEmpty != null) SelectCell(firstEmpty);
    }
}
