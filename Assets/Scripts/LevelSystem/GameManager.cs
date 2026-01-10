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
    // public int suankiBinaSayisi = 0; // BUNU SİLDİK, ARTIK SAYMAYACAĞIZ, KONTROL EDECEĞİZ
    public int suankiPopulasyon = 0;
    public bool oyunBittiMi = false;

    // Şu anki level verisine dışarıdan (BirlestirmeYoneticisi'nden) erişebilmek için public property yapabiliriz
    public LevelData SuankiLevelData => suankiLevelData; 

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
        
        // suankiBinaSayisi = 0; // SİLDİK
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
            // suankiBinaSayisi++; // ARTIK GEREK YOK

            int kazanilanPop = Random.Range(uretilenObjeVerisi.minPopulasyon, uretilenObjeVerisi.maxPopulasyon + 1);
            suankiPopulasyon += kazanilanPop;
            
            UpdatePopulasyonUI();
            ShowFloatingText(worldPos, "+" + kazanilanPop);
            
            if (CollectionManager.Instance != null)
            {
                // Objenin ID'sini gönderip kaydettiriyoruz
                CollectionManager.Instance.ObjeAcildi(uretilenObjeVerisi.collectionID);
                Debug.Log("Koleksiyon Kaydı Gönderildi: " + uretilenObjeVerisi.objeAdi);
            }
            else
            {
                // Eğer hata alırsan bunu görmek için:
                Debug.LogWarning("CollectionManager bulunamadı! Sahneye ekledin mi?");
                
                // GEÇİCİ ÇÖZÜM (Eğer CollectionManager sadece menüdeyse):
                // Direkt PlayerPrefs'e buradan da yazabiliriz garanti olsun diye:
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

    // --- YENİ KAZANMA KONTROLÜ ---
    // Griddeki tüm binaları tarar ve hedeflerle karşılaştırır.
    private bool HedeflerTamamlandiMi()
    {
        if (suankiLevelData.hedefler == null || suankiLevelData.hedefler.Count == 0) return true; // Hedef yoksa kazanmış say (veya false yapabilirsin)

        // Hedef listesindeki her bir madde için kontrol yap
        foreach (var hedef in suankiLevelData.hedefler)
        {
            int sahadakiAdet = 0;

            // GridManager içindeki array'e erişip sayıyoruz
            // (GridManager kodunda gridArray public olmalı veya erişim fonksiyonu olmalı)
            // Senin GridManager kodun bende yok ama genelde şöyledir:
            for (int x = 0; x < suankiLevelData.gridGenislik; x++)
            {
                for (int z = 0; z < suankiLevelData.gridYukseklik; z++)
                {
                    GridCell hucre = gridManager.GetCell(new Vector3Int(x, 0, z)); // Veya senin GridManager erişimin nasılsa
                    if (hucre != null && !hucre.IsEmpty())
                    {
                        if (hucre.currentObject.verisi == hedef.istenenObje)
                        {
                            sahadakiAdet++;
                        }
                    }
                }
            }

            // Eğer bu hedef için sayı yetersizse, henüz kazanmadık demektir.
            if (sahadakiAdet < hedef.adet)
            {
                return false;
            }
        }

        // Döngü bitti ve hiç 'return false' olmadıysa tüm hedefler tamamdır.
        return true;
    }

    public void HamleBittiKontrolu()
    {
        if (oyunBittiMi) return;

        if (gridManager.GridTamamenDoluMu())
        {
            // YENİ FONKSİYONU ÇAĞIRIYORUZ
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
            int kazanilanYildiz = YildizHesapla();
            KaydetYildiz(suankiLevelIndex, kazanilanYildiz);

            int acilacakLevelIndex = suankiLevelIndex + 1;
            int enYuksekLevel = PlayerPrefs.GetInt("HighestUnlockedLevel", 0);
            
            if (acilacakLevelIndex > enYuksekLevel)
            {
                PlayerPrefs.SetInt("HighestUnlockedLevel", acilacakLevelIndex);
                PlayerPrefs.Save();
            }

            winBaslikText.text = suankiLevelData.levelAdi + " Tamamlandı!";
            
            if (suankiLevelIndex + 1 < tumLeveller.Count)
                winSonrakiLevelText.text = tumLeveller[suankiLevelIndex + 1].levelAdi;
            else
                winSonrakiLevelText.text = "Oyun Bitti!";

            winPanel.SetActive(true);
        }
        else
        {
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