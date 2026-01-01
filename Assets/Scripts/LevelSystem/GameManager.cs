using System;
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
    public int suankiLevelIndex = 0; // 1. Bölüm
    public int suankiBinaSayisi = 0;
    public bool oyunBittiMi = false;

    [Header("Referanslar")] 
    public GridManager gridManager;
    public PlacementManager placementManager;

    public GameObject winPanel;
    public GameObject losePanel;
    
    public Text winBaslikText;    // "Tebrikler Level 1 Bitti" yazacak yer
    public Text winSonrakiLevelText; // "Sıradaki: Level 2" yazacak yer
    public Text loseBaslikText;   // "Level 1 Başarısız" yazacak yer

    private LevelData suankiLevelData;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        LeveliBaslat(suankiLevelIndex);
    }

    public void LeveliBaslat(int index)
    {
        if (index >= tumLeveller.Count)
        {
            Debug.Log("Oyun bitti");
            return;
        }

        suankiLevelIndex = index;
        suankiLevelData = tumLeveller[index];
        
        //değişkenleri sıfırlıyoruz
        suankiBinaSayisi = 0;
        oyunBittiMi = false;
        winPanel.SetActive(false);
        losePanel.SetActive(false);

        gridManager.GridiOlustur(suankiLevelData.gridGenislik, suankiLevelData.gridYukseklik);
        placementManager.SetupSpawnList(suankiLevelData.levelObjeleri);
        
        placementManager.SpawnYeniObje();
    }
        
        // Bu fonksiyonu BirlestirmeYoneticisi çağıracak (Bina oluşunca)
        public void UretimYapildi(ObjeVerisi uretilenObjeVerisi)
        {
            if (oyunBittiMi) return;

            // EĞER ÜRETİLEN ŞEY BİR "BİNA" İSE SAYACI ARTIR
            if (uretilenObjeVerisi.tur == ObjeTuru.Bina)
            {
                suankiBinaSayisi++;
                Debug.Log("Yeni bir bina inşa edildi! Toplam: " + suankiBinaSayisi);
            }
            else
            {
                Debug.Log("Ara malzeme üretildi (Puan artmadı): " + uretilenObjeVerisi.objeAdi);
            }
        }

        public void HamleBittiKontrolu()
        {
            if (oyunBittiMi)
            {
                return;
            }

            if (gridManager.GridTamamenDoluMu())
            {
                OyunBittiKararVer();
            }
        }

        public void OyunBittiKararVer()
        {
            oyunBittiMi = true;

            // Hedefe ulaşıldı mı?
            if (suankiBinaSayisi >= suankiLevelData.hedeflenenBinaSayisi)
            {
                // --- KAZANMA DURUMU ---
                Debug.Log("Kazandın");
            
                // WİN BAŞLIK
                winBaslikText.text = suankiLevelData.levelAdi + " Tamamlandı!";

                // 2. Bir sonraki levelin adını bul ve yaz
                if (suankiLevelIndex + 1 < tumLeveller.Count)
                {
                    string sonrakiLevelAdi = tumLeveller[suankiLevelIndex + 1].levelAdi;
                    winSonrakiLevelText.text = sonrakiLevelAdi;
                }
                else
                {
                    winSonrakiLevelText.text = "Tebrikler!";
                }

                winPanel.SetActive(true);
            }
            else
            {
                // --- KAYBETME DURUMU ---
                Debug.Log("Kaybettin");
            
                // LOSE BAŞLIK
                loseBaslikText.text = suankiLevelData.levelAdi + "Kaybettin!";
            
                losePanel.SetActive(true);
            }
        }
        public void SonrakiLevelButonu()
        {
            // Bir sonraki levele geç
            // (Arkadaşın Save Sistemi yapınca buraya 'Save(suankiLevelIndex + 1)' kodunu ekleyecek)
            LeveliBaslat(suankiLevelIndex + 1);
        }

        public void YenidenOynaButonu()
        {
            // Aynı leveli baştan başlat
            LeveliBaslat(suankiLevelIndex);
        }
    
        public void AnaMenuButonu()
        {
            // Ana menü sahnesine dön (Şimdilik boş bırakabilirsin)
            Debug.Log("Ana Menüye Dönüldü");
        }
        
    
    
}
