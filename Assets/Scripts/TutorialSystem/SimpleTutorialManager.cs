using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
using UnityEngine.SceneManagement;

[System.Serializable]
public class TutorialSetupItem
{
    public ObjeVerisi obje;
    public int x;
    public int z;
}

public class SimpleTutorialManager : MonoBehaviour
{
    public static SimpleTutorialManager Instance;

    [Header("Referanslar")]
    public GridManager gridManager;
    public Text instructionText; 

    [Header("Grid Ayarları")]
    public int gridWidth = 4;
    public int gridHeight = 4;
    public GridCell zeminPrefabi; // İsteğe bağlı, boş bırakılabilir

    [Header("Hedef Objeler")]
    public ObjeVerisi kolonVerisi;      
    public ObjeVerisi insaatAlaniVerisi; 
    public ObjeVerisi evVerisi;          

    [Header("Sahne Kurulumu")]
    public List<TutorialSetupItem> baslangicObjeleri;

    private bool tutorialBitti = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartCoroutine(BaslangicAkisi());
    }

    IEnumerator BaslangicAkisi()
    {
        // 1. PlacementManager'ı Hemen Kilitle (Oyuncu dokunamasın)
        if (PlacementManager.Instance != null)
        {
            PlacementManager.Instance.tutorialModuAktif = true; 
            PlacementManager.Instance.SetInputLock(true); // Grid oluşurken dokunmayı engelle
        }

        // 2. GRID'İ OLUŞTUR VE ANİMASYONU BEKLE
        // GameManager devre dışı olduğu için bunu biz çağırıyoruz.
        yield return StartCoroutine(gridManager.GridiAnimasyonluOlustur(
            gridWidth, 
            gridHeight, 
            zeminPrefabi, 
            null // Callback kullanmıyoruz, Coroutine bitişini bekleyeceğiz
        ));

        // 3. Grid oturduktan sonra input kilidini aç (ama tutorial modu kalsın)
        if (PlacementManager.Instance != null)
        {
            PlacementManager.Instance.SetInputLock(false);
        }

        // 4. Objeleri Sahneye Diz
        SetupScene();

        // 5. İlk Görevi Kontrol Et
        CheckTutorialStatus();
    }

    void SetupScene()
    {
        foreach (var item in baslangicObjeleri)
        {
            Vector3Int pos = new Vector3Int(item.x, 0, item.z);
            GridCell cell = gridManager.GetCell(pos);

            if (cell != null && cell.IsEmpty())
            {
                Vector3 worldPos = gridManager.grid.GetCellCenterWorld(pos);
                Quaternion rot = item.obje.objePrefab.transform.rotation;

                // Yükseklik ayarı
                float yOffset = 0.5f;
                PlaceableObject prefabScript = item.obje.objePrefab.GetComponent<PlaceableObject>();
                if (prefabScript != null) yOffset = prefabScript.heightOffset;

                Vector3 spawnPos = worldPos + Vector3.up * yOffset;

                // Objeyi oluştur
                GameObject obj = Instantiate(item.obje.objePrefab, spawnPos, rot);
                PlaceableObject po = obj.GetComponent<PlaceableObject>();

                po.verisi = item.obje;
                po.currentCell = cell;
                po.hareketHakki = 1; 
                po.SetPreviewMode(false); 
                
                cell.currentObject = po;
                po.PlaySpawnAnimation();
            }
        }
    }

    public void CheckTutorialStatus()
    {
        if (tutorialBitti) return;

        int kolonSayisi = 0;
        int insaatAlaniSayisi = 0;

        foreach (var cell in gridManager.GetAllCells())
        {
            if (!cell.IsEmpty() && cell.currentObject != null)
            {
                if (cell.currentObject.verisi == kolonVerisi) kolonSayisi++;
                else if (cell.currentObject.verisi == insaatAlaniVerisi) insaatAlaniSayisi++;
            }
        }

        bool kolonTamam = kolonSayisi >= 1;
        bool insaatTamam = insaatAlaniSayisi >= 2;

        if (!kolonTamam)
        {
            UpdateText($"Demirleri birlestir\n ve {1 - kolonSayisi} tane \nKolon yap.");
        }
        else if (!insaatTamam)
        {
            UpdateText($"Tuğlaları birlestir \nve {2 - insaatAlaniSayisi} adet daha \nİnşaat Alanı oluştur.");
        }
        else
        {
            StartCoroutine(FinalSequence());
        }
    }

    void UpdateText(string mesaj)
    {
        if (instructionText != null)
            instructionText.text = mesaj;
    }

    IEnumerator FinalSequence()
    {
        tutorialBitti = true;
        UpdateText("Tebrikler! Öğretici\n tamamlandı.Ana \nmenüye dönülüyor...");

        yield return new WaitForSeconds(1.0f);

        gridManager.TemizleVeYokEtPublic();

        yield return new WaitForSeconds(0.5f);

        SpawnFinalHouse();

        yield return new WaitForSeconds(3.0f);

        // Tutorial bitti, normal oyuna dön
        PlayerPrefs.SetInt("TutorialCompleted", 1);
        PlayerPrefs.Save();
        
        DataTransfer.secilenLevelIndex = 0; 

        if (PlacementManager.Instance != null)
            PlacementManager.Instance.tutorialModuAktif = false;

        SceneManager.LoadScene("UI");
    }

    void SpawnFinalHouse()
    {
        // Evi Grid'in ortasına koyalım (örn: 1,1 veya 2,2)
        Vector3Int centerPos = new Vector3Int(gridWidth / 2, 0, gridHeight / 2);
        GridCell cell = gridManager.GetCell(centerPos);

        if (cell != null)
        {
            Vector3 worldPos = gridManager.grid.GetCellCenterWorld(centerPos);
            GameObject ev = Instantiate(evVerisi.objePrefab, worldPos, evVerisi.objePrefab.transform.rotation);
            
            PlaceableObject po = ev.GetComponent<PlaceableObject>();
            ev.transform.position += Vector3.up * po.heightOffset;
            
            po.PlaySpawnAnimation();
        }
    }
}