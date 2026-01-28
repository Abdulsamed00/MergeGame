using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Referanslar")]
    public GridManager gridManager;
    public PlacementManager placementManager;
    public UndoManager undoManager; 
    public InitialSpawnManager initialSpawnManager; 
    public GameObject floatingTextPrefab;
    public BirlestirmeYoneticisi birlestirmeYoneticisi;

    [Header("UI Panelleri")]
    public GameObject winPanel;
    public GameObject losePanel;
    
    [Header("UI Textler")]
    public Text winBaslikText;
    public Text winSonrakiLevelText;
    public Text loseBaslikText;
    public Text populasyonText; 

    [Header("Bölüm Listesi")]
    public List<LevelData> tumLeveller;
    
    [Header("Mevcut Durumlar")]
    public int suankiLevelIndex = 0;
    public int suankiPopulasyon = 0;
    public bool oyunBittiMi = false;

    private LevelData suankiLevelData;
    public LevelData SuankiLevelData => suankiLevelData; 

    private Dictionary<string, ObjeVerisi> objeSozlugu = new Dictionary<string, ObjeVerisi>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        int gelenLevel = DataTransfer.secilenLevelIndex;
        if (gelenLevel >= tumLeveller.Count) gelenLevel = 0;

        suankiLevelIndex = gelenLevel;
        suankiLevelData = tumLeveller[suankiLevelIndex];

        SozlukOlustur(suankiLevelData);

        // Coroutine başlatıyoruz
        StartCoroutine(LevelAkisi());
    }

    void SozlukOlustur(LevelData data)
    {
        objeSozlugu.Clear();

        foreach(var spawnItem in data.levelObjeleri) EkleSozluge(spawnItem.obje);
        foreach(var hedef in data.hedefler) EkleSozluge(hedef.istenenObje);
        if(data.izinVerilenEnUstObje != null) EkleSozluge(data.izinVerilenEnUstObje);

        if (birlestirmeYoneticisi != null && birlestirmeYoneticisi.tumTarifler != null)
        {
            foreach (var tarif in birlestirmeYoneticisi.tumTarifler)
            {
                if (tarif.sonucObjesi != null) EkleSozluge(tarif.sonucObjesi);
            }
        }
    }

    void EkleSozluge(ObjeVerisi veri)
    {
        if(veri != null && !string.IsNullOrEmpty(veri.saveID))
        {
            if(!objeSozlugu.ContainsKey(veri.saveID))
                objeSozlugu.Add(veri.saveID, veri);
        }
    }

    // --- BURASI DEĞİŞTİ ---
    IEnumerator LevelAkisi()
    {
        suankiPopulasyon = 0;
        oyunBittiMi = false;
        
        UpdatePopulasyonUI();
        if(winPanel) winPanel.SetActive(false);
        if(losePanel) losePanel.SetActive(false);
        
        placementManager.SetupSpawnList(suankiLevelData.levelObjeleri);

        // Grid oluşturma fonksiyonuna "Bitince Ne Yapayım?" (Action) parametresi gönderiyoruz.
        // Böylece Grid tamamen çizilmeden Load işlemi çalışmayacak.
        yield return StartCoroutine(gridManager.GridiAnimasyonluOlustur(
            suankiLevelData.gridGenislik, 
            suankiLevelData.gridYukseklik, 
            suankiLevelData.zeminPrefabi,
            () => {
                // Grid bitti, şimdi kayıt kontrolü yapabiliriz
                KayitKontrolVeBaslat();
            }
        ));
    }

    // Grid bittikten sonra çalışacak fonksiyon
    void KayitKontrolVeBaslat()
    {
        if (SaveManager.HasSaveFile(suankiLevelIndex))
        {
            Debug.Log($"Level {suankiLevelIndex} için kayıt bulundu, yükleniyor...");
            LoadGameIslemi();
        }
        else
        {
            Debug.Log("Kayıt yok, sıfırdan başlanıyor.");
            if (initialSpawnManager != null)
            {
                initialSpawnManager.SpawnBaslat();
            }
            else
            {
                placementManager.BeginPlacementAfterInitialSpawn();
            }
        }
    }

    public void OyunuKaydet()
    {
        if (oyunBittiMi) return;

        SaveData data = new SaveData();
        data.levelIndex = suankiLevelIndex;
        data.currentPopulation = suankiPopulasyon;

        if (undoManager != null) data.undoRights = undoManager.KalanHak;

        if (placementManager.siradakiObjeVerisi != null)
            data.currentSpawnObjectID = placementManager.siradakiObjeVerisi.saveID;
            
        if (placementManager.sonrakiObjeVerisi != null)
            data.nextSpawnObjectID = placementManager.sonrakiObjeVerisi.saveID;

        List<GridCell> allCells = gridManager.GetAllCells();
        foreach (var cell in allCells)
        {
            if (!cell.IsEmpty() && cell.currentObject != null)
            {
                PlaceableObject objScript = cell.currentObject;
                if (objScript.verisi != null)
                {
                    GridObjectData objData = new GridObjectData();
                    
                    objData.objectID = objScript.verisi.saveID;
                    objData.x = cell.cellPosition.x;
                    objData.z = cell.cellPosition.z;
                    objData.movementRights = objScript.hareketHakki;
                    objData.isLocked = objScript.kilitliMi;

                    foreach(var icMalzeme in objScript.icindekiMalzemeler)
                    {
                        if(icMalzeme != null) objData.stackedItemIDs.Add(icMalzeme.saveID);
                    }
                    data.placedObjects.Add(objData);
                }
            }
        }
        SaveManager.Save(data, suankiLevelIndex);
    }

    // --- BURASI DEĞİŞTİ (POZİSYON HESABI İÇİN) ---
    // GameManager.cs içindeki LoadGameIslemi fonksiyonu

    void LoadGameIslemi()
    {
        SaveData data = SaveManager.Load(suankiLevelIndex);
        if (data == null) return;

        suankiPopulasyon = data.currentPopulation;
        UpdatePopulasyonUI();

        if (undoManager != null)
        {
            undoManager.ResetHistory(); 
            undoManager.LoadRights(data.undoRights); 
        }

        foreach (var savedObj in data.placedObjects)
        {
            if (objeSozlugu.TryGetValue(savedObj.objectID, out ObjeVerisi anaVeri))
            {
                Vector3Int cellPos = new Vector3Int(savedObj.x, 0, savedObj.z);
                GridCell cell = gridManager.GetCell(cellPos);
                
                if (cell != null)
                {
                    Vector3 spawnPos = gridManager.grid.GetCellCenterWorld(cellPos);

                    PlaceableObject tempPO = anaVeri.objePrefab.GetComponent<PlaceableObject>();
                    float offset = tempPO != null ? tempPO.heightOffset : 0.5f;
                    spawnPos.y += offset;

                    // --- DÜZELTME BURADA ---
                    // ESKİSİ: Quaternion.identity (Sıfır rotasyon)
                    // YENİSİ: anaVeri.objePrefab.transform.rotation (Prefabın orijinal duruşu)
                    
                    GameObject go = Instantiate(anaVeri.objePrefab, spawnPos, anaVeri.objePrefab.transform.rotation);
                    
                    // -----------------------

                    PlaceableObject po = go.GetComponent<PlaceableObject>();

                    po.verisi = anaVeri;
                    po.hareketHakki = savedObj.movementRights;
                    po.kilitliMi = savedObj.isLocked;
                    
                    po.icindekiMalzemeler.Clear();
                    foreach(string stackID in savedObj.stackedItemIDs)
                    {
                        if(objeSozlugu.TryGetValue(stackID, out ObjeVerisi stackVeri))
                        {
                            po.icindekiMalzemeler.Add(stackVeri);
                        }
                    }
                    
                    po.BoyutuGuncelle(); 
                    
                    po.currentCell = cell;
                    cell.currentObject = po;
                }
            }
        }
        
        // ... (Geri kalan kodlar aynı) ...
        ObjeVerisi siradaki = null;
        ObjeVerisi sonraki = null;

        if (!string.IsNullOrEmpty(data.currentSpawnObjectID)) 
            objeSozlugu.TryGetValue(data.currentSpawnObjectID, out siradaki);
            
        if (!string.IsNullOrEmpty(data.nextSpawnObjectID)) 
            objeSozlugu.TryGetValue(data.nextSpawnObjectID, out sonraki);

        if(siradaki == null && suankiLevelData.levelObjeleri.Count > 0) 
            siradaki = suankiLevelData.levelObjeleri[0].obje; 
            
        if(sonraki == null && suankiLevelData.levelObjeleri.Count > 0) 
            sonraki = suankiLevelData.levelObjeleri[0].obje;

        placementManager.LoadSpawnState(siradaki, sonraki);
    }
        
    public void UretimYapildi(ObjeVerisi uretilenObjeVerisi, Vector3 worldPos)
    {
        if (oyunBittiMi) return;

        if (uretilenObjeVerisi.tur == ObjeTuru.Bina)
        {
            int kazanilanPop = Random.Range(uretilenObjeVerisi.minPopulasyon, uretilenObjeVerisi.maxPopulasyon + 1);
            suankiPopulasyon += kazanilanPop;
            
            UpdatePopulasyonUI();
            ShowFloatingText(worldPos, "+" + kazanilanPop);
            
            if (CollectionManager.Instance != null)
            {
                CollectionManager.Instance.ObjeAcildi(uretilenObjeVerisi.collectionID);
            }
            else
            {
                string key = "Collection_" + uretilenObjeVerisi.collectionID;
                PlayerPrefs.SetInt(key, 1);
                PlayerPrefs.Save();
            }
        }
    }

    private void UpdatePopulasyonUI()
    {
        if(populasyonText != null) populasyonText.text = "Nüfus: " + suankiPopulasyon;
    }

    private void ShowFloatingText(Vector3 pos, string text)
    {
        if (floatingTextPrefab != null)
        {
            Vector3 spawnPos = pos + Vector3.up * 1.5f; 
            GameObject go = Instantiate(floatingTextPrefab, spawnPos, Quaternion.identity);
            go.GetComponent<FloatingText>().SetText(text, Color.green);
        }
    }

    private bool HedeflerTamamlandiMi()
    {
        if (suankiLevelData.hedefler == null || suankiLevelData.hedefler.Count == 0) return true;

        foreach (var hedef in suankiLevelData.hedefler)
        {
            int sahadakiAdet = 0;
            foreach(var hucre in gridManager.GetAllCells())
            {
                if (hucre != null && !hucre.IsEmpty())
                {
                    if (hucre.currentObject.verisi == hedef.istenenObje)
                    {
                        sahadakiAdet++;
                    }
                }
            }
            if (sahadakiAdet < hedef.adet) return false;
        }
        return true;
    }

    public void HamleBittiKontrolu()
    {
        if (oyunBittiMi) return;
        
        OyunuKaydet();

        if (gridManager.GridTamamenDoluMu())
        {
            if (HedeflerTamamlandiMi())
            {
                OyunBittiKararVer(true);
            }
            else
            {
                OyunBittiKararVer(false);
            }
        }
    }

    public void OyunBittiKararVer(bool kazandiMi)
    {
        if (oyunBittiMi) return;
        oyunBittiMi = true;

        if (kazandiMi)
        {
            Debug.Log("KAZANDIN!");
            
            SaveManager.DeleteSave(suankiLevelIndex);

            int kazanilanYildiz = YildizHesapla();
            KaydetYildiz(suankiLevelIndex, kazanilanYildiz);

            int acilacakLevelIndex = suankiLevelIndex + 1;
            int enYuksekLevel = PlayerPrefs.GetInt("HighestUnlockedLevel", 0);
            
            if (acilacakLevelIndex > enYuksekLevel)
            {
                PlayerPrefs.SetInt("HighestUnlockedLevel", acilacakLevelIndex);
                PlayerPrefs.Save();
            }

            if(winBaslikText) winBaslikText.text = suankiLevelData.levelAdi + " Tamamlandı!";
            
            if(winSonrakiLevelText)
            {
                if (suankiLevelIndex + 1 < tumLeveller.Count)
                    winSonrakiLevelText.text = tumLeveller[suankiLevelIndex + 1].levelAdi;
                else
                    winSonrakiLevelText.text = "Oyun Bitti!";
            }

            if(winPanel) winPanel.SetActive(true);
        }
        else
        {
            if(loseBaslikText) loseBaslikText.text = "Başarısız!";
            if(losePanel) losePanel.SetActive(true);
        }
    }
    
    int YildizHesapla()
    {
        if (suankiPopulasyon >= suankiLevelData.yildiz3Puani) return 3;
        if (suankiPopulasyon >= suankiLevelData.yildiz2Puani) return 2;
        if (suankiPopulasyon >= suankiLevelData.yildiz1Puani) return 1;
        return 0;
    }

    void KaydetYildiz(int levelIndex, int yildizSayisi)
    {
        string saveKey = "Level_" + levelIndex + "_Stars";
        int eskiYildiz = PlayerPrefs.GetInt(saveKey, 0);
        if (yildizSayisi > eskiYildiz)
        {
            PlayerPrefs.SetInt(saveKey, yildizSayisi);
            PlayerPrefs.Save();
        }
    }
    
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus) OyunuKaydet();
    }

    private void OnApplicationQuit()
    {
        OyunuKaydet();
    }

    public void AnaMenuButonu()
    {
        OyunuKaydet(); 
        SceneManager.LoadScene("UI"); 
    }

    public void SonrakiLevelButonu()
    {
        DataTransfer.secilenLevelIndex = suankiLevelIndex + 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void YenidenOynaButonu()
    {
        SaveManager.DeleteSave(suankiLevelIndex);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void AnaMenuLosePaneldenDon()
    {
        SceneManager.LoadScene("UI");
    }
    
    public void AnaMenuWinPaneldenDon()
    {
        SceneManager.LoadScene("UI");
    }
}