using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Text için
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
    public Text instructionText; // Ekranda görevi yazacak Text

    [Header("Hedef Objeler (Kontrol İçin)")]
    public ObjeVerisi kolonVerisi;       // Sayılacak obje 1
    public ObjeVerisi insaatAlaniVerisi; // Sayılacak obje 2
    public ObjeVerisi evVerisi;          // Final ödülü

    [Header("Sahne Kurulumu")]
    public List<TutorialSetupItem> baslangicObjeleri;

    private bool tutorialBitti = false;

    void Awake()
    {
        Instance = this;
    }

    // GridManager start'ta gridi oluşturduktan sonra bunu çağırabilir 
    // Veya GameManager'dan çağrılabilir.
    // Biz garanti olsun diye Start'ta biraz bekleyip başlatacağız.
    void Start()
    {
        StartCoroutine(BaslangicKurulumu());
    }

    IEnumerator BaslangicKurulumu()
    {
        // Grid'in ve diğer sistemlerin oturması için güvenli bekleme
        yield return new WaitForSeconds(4.0f);

        // 1. PlacementManager'ı KİLİTLE (Preview Yok, Spawn Yok)
        if (PlacementManager.Instance != null)
        {
            PlacementManager.Instance.tutorialModuAktif = true; // Kilit açıldı
            PlacementManager.Instance.SetInputLock(false); // Tıklamaya izin ver ama spawn yapma
        }

        // 2. Objeleri Sahneye Diz
        SetupScene();

        // 3. İlk Durumu Kontrol Et ve Yazıyı Yaz
        CheckTutorialStatus();
    }

    void SetupScene()
    {
        Debug.Log($"Toplam {baslangicObjeleri.Count} adet obje yerleştirilmeye çalışılacak.");

        foreach (var item in baslangicObjeleri)
        {
            Vector3Int pos = new Vector3Int(item.x, 0, item.z);
            GridCell cell = gridManager.GetCell(pos);

            // KONTROL 1: Hücre Var mı?
            if (cell == null)
            {
                Debug.LogError($"BAŞARISIZ: '{item.obje.name}' objesi ({item.x}, {item.z}) konumuna koyulamadı. SEBEP: Bu koordinatta Grid Hücresi YOK! (Grid sınırları dışında)");
                continue; 
            }

            // KONTROL 2: Hücre Boş mu?
            if (!cell.IsEmpty())
            {
                Debug.LogError($"BAŞARISIZ: '{item.obje.name}' objesi ({item.x}, {item.z}) konumuna koyulamadı. SEBEP: Hücre DOLU! (Başka bir obje var)");
                continue;
            }

            // BAŞARILI
            Vector3 worldPos = gridManager.grid.GetCellCenterWorld(pos);
            Quaternion rot = item.obje.objePrefab.transform.rotation;

            float yOffset = 0.5f;
            PlaceableObject prefabScript = item.obje.objePrefab.GetComponent<PlaceableObject>();
            if (prefabScript != null) yOffset = prefabScript.heightOffset;

            Vector3 spawnPos = worldPos + Vector3.up * yOffset;

            GameObject obj = Instantiate(item.obje.objePrefab, spawnPos, rot);
            PlaceableObject po = obj.GetComponent<PlaceableObject>();

            po.verisi = item.obje;
            po.currentCell = cell;
            po.hareketHakki = 1; 
            po.SetPreviewMode(false); 
            
            cell.currentObject = po;
            po.PlaySpawnAnimation();
            
            Debug.Log($"BAŞARILI: {item.obje.name} -> ({item.x}, {item.z})");
        }
    }

    // Bu fonksiyon her "Merge" (Birleşme) işleminden sonra çağrılmalı!
    public void CheckTutorialStatus()
    {
        if (tutorialBitti) return;

        int kolonSayisi = 0;
        int insaatAlaniSayisi = 0;

        // Sahayı Tara
        foreach (var cell in gridManager.GetAllCells())
        {
            if (!cell.IsEmpty() && cell.currentObject != null)
            {
                if (cell.currentObject.verisi == kolonVerisi) kolonSayisi++;
                else if (cell.currentObject.verisi == insaatAlaniVerisi) insaatAlaniSayisi++;
            }
        }

        // --- GÖREV MANTIĞI ---

        // Hedef: 2 Kolon, 1 İnşaat Alanı
        bool kolonTamam = kolonSayisi >= 2;
        bool insaatTamam = insaatAlaniSayisi >= 1;

        if (!kolonTamam)
        {
            UpdateText($"Görev: Demirleri birleştir ve {2 - kolonSayisi} tane daha Kolon yap.");
        }
        else if (!insaatTamam)
        {
            UpdateText("Görev: Tuğlaları birleştir ve İnşaat Alanı oluştur.");
        }
        else
        {
            // HEPSİ TAMAM!
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
        UpdateText("Tebrikler! Ev inşa ediliyor...");

        yield return new WaitForSeconds(1.0f);

        // 1. Sahayı Temizle
        gridManager.TemizleVeYokEtPublic();

        yield return new WaitForSeconds(0.5f);

        // 2. Evi (0,0) noktasına koy
        SpawnFinalHouse();

        yield return new WaitForSeconds(2.0f);

        // 3. UI Sahnesine Dön
        // Çıkmadan önce kilidi açalım ki normal oyun bozulmasın
        if (PlacementManager.Instance != null)
            PlacementManager.Instance.tutorialModuAktif = false;

        SceneManager.LoadScene("UI");
    }

    void SpawnFinalHouse()
    {
        Vector3Int centerPos = new Vector3Int(0, 0, 0); // İsteğe göre (1,1) veya (2,2) yapabilirsin
        GridCell cell = gridManager.GetCell(centerPos);

        if (cell != null)
        {
            Vector3 worldPos = gridManager.grid.GetCellCenterWorld(centerPos);
            GameObject ev = Instantiate(evVerisi.objePrefab, worldPos, evVerisi.objePrefab.transform.rotation);
            
            // Yükseklik ayarı
            PlaceableObject po = ev.GetComponent<PlaceableObject>();
            ev.transform.position += Vector3.up * po.heightOffset;
            
            po.PlaySpawnAnimation();
        }
    }
}