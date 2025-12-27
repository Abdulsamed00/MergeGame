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

        placementManager.spawnlanabilirObjeler = suankiLevelData.spawnlanablirObjeler;
        placementManager.SpawnYeniObje();
    }
        
        // Bu fonksiyonu BirlestirmeYoneticisi çağıracak (Bina oluşunca)
        public void BinaYapildi()
        {
            if (oyunBittiMi)
            {
                return;
            }

            suankiBinaSayisi++;
            Debug.Log("Bina yapıldı Toplam bina;" + suankiBinaSayisi);
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
            if (suankiBinaSayisi >= suankiLevelData.hedeflenenBinaSayisi)
            {
                Debug.Log("Kazandın");
                winPanel.SetActive(true);
            }
            else
            {
                Debug.Log("Kaybettin");
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
