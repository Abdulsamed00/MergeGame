using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

public class UndoManager : MonoBehaviour
{
    public static UndoManager Instance;

    [Header("Referanslar")]
    public GridManager gridManager;
    public PlacementManager placementManager;

    [Header("Undo Hakkı ve UI Ayarları")]
    public int baslangicHakki = 3; 
    public Text hakText; 
    
    // GameManager'ın erişip kaydedebilmesi için Property yaptık
    public int KalanHak { get; private set; }

    // Oyun içi anlık durumları (Snapshots) tutan yığın
    private Stack<GameState> history = new Stack<GameState>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Eğer dışarıdan (Load işleminden) bir hak yüklenmediyse varsayılanı kullan
        if (KalanHak == 0) KalanHak = baslangicHakki;
        UpdateUI();
    }

    // --- SAVE/LOAD SİSTEMİ İÇİN EKLENEN FONKSİYONLAR ---
    
    // Oyun yüklenirken kayıtlı hakkı geri koymak için
    public void LoadRights(int rights)
    {
        KalanHak = rights;
        UpdateUI();
    }

    // Yeni level veya kayıt yüklendiğinde eski geçmişi silmek için
    public void ResetHistory()
    {
        history.Clear();
        // Eğer LoadRights çağrılmazsa (yeni oyunsa) hakkı resetle
        KalanHak = baslangicHakki; 
        UpdateUI();
    }
    // ----------------------------------------------------

    // Hamle yapmadan hemen önce çağrılır
    public void SaveState()
    {
        GameState state = new GameState();
        
        // Sıradaki objelerin verilerini sakla
        state.siradakiVeri = placementManager.siradakiObjeVerisi; 
        state.sonrakiVeri = placementManager.sonrakiObjeVerisi;

        state.gridObjects = new List<ObjectState>();
        
        // Grid üzerindeki tüm objeleri kaydet
        foreach (var cell in gridManager.GetAllCells())
        {
            if (!cell.IsEmpty() && cell.currentObject != null)
            {
                ObjectState objState = new ObjectState();
                objState.position = cell.cellPosition;
                objState.data = cell.currentObject.verisi;
                objState.isLocked = cell.currentObject.kilitliMi;
                objState.hareketHakki = cell.currentObject.hareketHakki;

                // Stack (içindeki malzemeler) listesini kopyala
                if (cell.currentObject.icindekiMalzemeler != null)
                {
                    objState.materials = new List<ObjeVerisi>(cell.currentObject.icindekiMalzemeler);
                }
                else
                {
                    objState.materials = new List<ObjeVerisi>();
                }
                
                state.gridObjects.Add(objState);
            }
        }
        history.Push(state);
    }

    public void Undo()
    {
        // Geçmiş yoksa veya hak bittiyse işlem yapma
        if (history.Count == 0 || KalanHak <= 0)
        {
            return;
        }

        // 1. Son durumu çek
        GameState lastState = history.Pop();
        
        // 2. Sahneyi temizle
        gridManager.TemizleVeYokEtPublic();

        // 3. Objeleri tek tek geri yerleştir
        foreach (var objState in lastState.gridObjects)
        {
            GridCell cell = gridManager.GetCell(objState.position);
            Vector3 worldPos = gridManager.grid.GetCellCenterWorld(objState.position);

            GameObject newObj = Instantiate(objState.data.objePrefab, worldPos, Quaternion.identity);
            PlaceableObject po = newObj.GetComponent<PlaceableObject>();

            // Verileri geri yükle
            po.verisi = objState.data;
            po.currentCell = cell;
            po.kilitliMi = objState.isLocked;
            po.hareketHakki = objState.hareketHakki;
            po.icindekiMalzemeler = new List<ObjeVerisi>(objState.materials);
            
            // Görsel ayarlar
            po.transform.position = worldPos + Vector3.up * po.heightOffset;
            po.BoyutuGuncelle();
            po.SetPreviewMode(false);

            cell.currentObject = po;
        }

        // 4. ELİMİZDEKİ OBJEYİ DÜZELT (Önemli Değişiklik Burası)
        // PlacementManager'daki LoadSpawnState fonksiyonunu kullanarak
        // hem veriyi hem de eldeki 3D görseli (Preview) güncelliyoruz.
        placementManager.LoadSpawnState(lastState.siradakiVeri, lastState.sonrakiVeri);

        // 5. Hakkı düş ve UI güncelle
        KalanHak--;
        UpdateUI();
    }
    
    // Merge iptali gibi durumlarda son state'i silmek gerekebilir
    public void RemoveLastState()
    {
        if (history.Count > 0)
        {
            history.Pop();
        }
    }

    private void UpdateUI()
    {
        if (hakText != null)
        {
            hakText.text = KalanHak.ToString();
        }
    }
}

// --- YARDIMCI SINIFLAR ---

[System.Serializable]
public class GameState
{
    public ObjeVerisi siradakiVeri;
    public ObjeVerisi sonrakiVeri;
    public List<ObjectState> gridObjects;
}

[System.Serializable]
public class ObjectState
{
    public Vector3Int position;
    public ObjeVerisi data;
    public bool isLocked;
    public int hareketHakki;
    public List<ObjeVerisi> materials;
}