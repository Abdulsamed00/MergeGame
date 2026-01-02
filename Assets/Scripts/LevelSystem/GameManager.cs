using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    [Header("Bölüm Listesi")]
    public List<LevelData> tumLeveller;
    
    [Header("Mevcut Durumlar")]
    public int suankiLevelIndex = 0;
    public int suankiBinaSayisi = 0;
    public int suankiPopulasyon = 0;
    public bool oyunBittiMi = false;

    [Header("Referanslar")] 
    public GridManager gridManager;
    public PlacementManager placementManager;
    public GameObject floatingTextPrefab;

    [Header("UI Panelleri")]
    public GameObject winPanel;
    public GameObject losePanel;
    
    [Header("UI Textler")]
    public Text winBaslikText;
    public Text winSonrakiLevelText;
    public Text loseBaslikText;
    public Text populasyonText; 
    
    // --- DİKKAT: YILDIZ OBJELERİNİ BURADAN SİLDİM ---
    // Çünkü Win Panel'de yıldız olmayacak dedin.
    // Hesaplama arka planda yapılacak.

    private LevelData suankiLevelData;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        int gelenLevel = DataTransfer.secilenLevelIndex;
        LeveliBaslat(gelenLevel);
    }

    public void LeveliBaslat(int index)
    {
        if (index >= tumLeveller.Count) return;

        suankiLevelIndex = index;
        suankiLevelData = tumLeveller[index];
        
        suankiBinaSayisi = 0;
        suankiPopulasyon = 0;
        oyunBittiMi = false;
        
        UpdatePopulasyonUI();
        winPanel.SetActive(false);
        losePanel.SetActive(false);

        gridManager.GridiOlustur(suankiLevelData.gridGenislik, suankiLevelData.gridYukseklik);
        placementManager.SetupSpawnList(suankiLevelData.levelObjeleri);
        placementManager.SpawnYeniObje();
    }
        
    public void UretimYapildi(ObjeVerisi uretilenObjeVerisi, Vector3 worldPos)
    {
        if (oyunBittiMi) return;

        if (uretilenObjeVerisi.tur == ObjeTuru.Bina)
        {
            suankiBinaSayisi++;

            int kazanilanPop = Random.Range(uretilenObjeVerisi.minPopulasyon, uretilenObjeVerisi.maxPopulasyon + 1);
            suankiPopulasyon += kazanilanPop;
            
            UpdatePopulasyonUI();
            ShowFloatingText(worldPos, "+" + kazanilanPop);
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

    public void HamleBittiKontrolu()
    {
        if (oyunBittiMi) return;

        if (gridManager.GridTamamenDoluMu())
        {
            if (suankiBinaSayisi >= suankiLevelData.hedeflenenBinaSayisi)
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
            // --- KAZANMA ---
            Debug.Log("KAZANDIN!");
            
            // 1. Yıldızı Hesapla ve Sessizce Kaydet
            int kazanilanYildiz = YildizHesapla();
            KaydetYildiz(suankiLevelIndex, kazanilanYildiz);

            // 2. Kilidi Aç
            int acilacakLevelIndex = suankiLevelIndex + 1;
            int enYuksekLevel = PlayerPrefs.GetInt("HighestUnlockedLevel", 0);
            
            if (acilacakLevelIndex > enYuksekLevel)
            {
                PlayerPrefs.SetInt("HighestUnlockedLevel", acilacakLevelIndex);
                PlayerPrefs.Save();
            }

            // 3. Paneli Aç (Yıldız göstermeden)
            winBaslikText.text = suankiLevelData.levelAdi + " Tamamlandı!";
            
            if (suankiLevelIndex + 1 < tumLeveller.Count)
                winSonrakiLevelText.text = tumLeveller[suankiLevelIndex + 1].levelAdi;
            else
                winSonrakiLevelText.text = "Oyun Bitti!";

            winPanel.SetActive(true);
        }
        else
        {
            // --- KAYBETME ---
            loseBaslikText.text = "Başarısız!";
            losePanel.SetActive(true);
        }
    }
    
    int YildizHesapla()
    {
        // Puan hesaplama mantığı: Büyükten küçüğe kontrol et
        if (suankiPopulasyon >= suankiLevelData.yildiz3Puani) return 3;
        if (suankiPopulasyon >= suankiLevelData.yildiz2Puani) return 2;
        if (suankiPopulasyon >= suankiLevelData.yildiz1Puani) return 1;
        return 0;
    }

    void KaydetYildiz(int levelIndex, int yildizSayisi)
    {
        string saveKey = "Level_" + levelIndex + "_Stars";
        int eskiYildiz = PlayerPrefs.GetInt(saveKey, 0);
        // Sadece daha yüksek bir skor yaptıysa kaydet
        if (yildizSayisi > eskiYildiz)
        {
            PlayerPrefs.SetInt(saveKey, yildizSayisi);
            PlayerPrefs.Save();
        }
    }
    
    public void AnaMenuButonu()
    {
        SceneManager.LoadScene("UI"); 
    }

    public void SonrakiLevelButonu()
    {
        DataTransfer.secilenLevelIndex = suankiLevelIndex + 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void YenidenOynaButonu()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void AnaMenuLosePaneldenDon()
    {
        losePanel.SetActive(false);
        SceneManager.LoadScene("UI");
    }
    
    public void AnaMenuWinPaneldenDon()
    {
        winPanel.SetActive(false);
        SceneManager.LoadScene("UI");
    }
}