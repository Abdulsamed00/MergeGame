using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BirlestirmeYoneticisi : MonoBehaviour
{
    public static BirlestirmeYoneticisi Instance;
    public List<BirlestirmeVerisi> tumTarifler;
    public GridManager gridManager;

    private void Awake()
    {
        Instance = this;
    }
    
    // Simülasyon: Birleşme olur mu?
    public bool CanMerge(PlaceableObject elimizdeki, PlaceableObject yerdeki)
    {
        if (elimizdeki == null || yerdeki == null) return false;

        List<ObjeVerisi> toplamMalzemeler = new List<ObjeVerisi>();
        toplamMalzemeler.AddRange(elimizdeki.icindekiMalzemeler);
        toplamMalzemeler.AddRange(yerdeki.icindekiMalzemeler);

        BirlestirmeVerisi uygunTarif = TarifAra(toplamMalzemeler);
        if (uygunTarif != null)
        {
            if (SeviyeSiniriAsiliyorMu(uygunTarif.sonucObjesi)) return false;
            return true; 
        }

        if (elimizdeki.verisi == yerdeki.verisi)
        {
            return true; 
        }

        return false;
    }

    private bool SeviyeSiniriAsiliyorMu(ObjeVerisi sonucObjesi)
    {
        if (GameManager.Instance == null) return false;
        LevelData currentLevel = GameManager.Instance.SuankiLevelData;
        if (currentLevel == null || currentLevel.izinVerilenEnUstObje == null) return false;
        
        if (sonucObjesi.objeSeviyesi > currentLevel.izinVerilenEnUstObje.objeSeviyesi)
        {
            return true;
        }
        return false;
    }

    public bool ManuelBirlestirme(PlaceableObject elimizdeki, PlaceableObject yerdeki)
    {
        List<ObjeVerisi> toplamMalzemeler = new List<ObjeVerisi>();
        toplamMalzemeler.AddRange(elimizdeki.icindekiMalzemeler);
        toplamMalzemeler.AddRange(yerdeki.icindekiMalzemeler);

        BirlestirmeVerisi uygunTarif = TarifAra(toplamMalzemeler);

        // A) TARİF İLE BİRLEŞME (Bina Oluşumu)
        if (uygunTarif != null)
        {
            if (SeviyeSiniriAsiliyorMu(uygunTarif.sonucObjesi)) return false;

            GridCell hedefHucre = yerdeki.currentCell;
            hedefHucre.currentObject = null; 
            Destroy(yerdeki.gameObject);
            
            BinaOlustur(hedefHucre, uygunTarif.sonucObjesi);
            return true;
        }

        // B) AYNI TÜR YIĞINLAMA (Stack)
        if (elimizdeki.verisi == yerdeki.verisi)
        {
            yerdeki.icindekiMalzemeler.AddRange(elimizdeki.icindekiMalzemeler);
            yerdeki.BoyutuGuncelle();
            
            yerdeki.hareketHakki = 1; 
            
            // Eğer stack yapılırken de animasyon istemiyorsan burayı da silebilirsin.
            // yerdeki.PlayStackAnimation(); 
            return true;
        }

        return false;
    }

    private BirlestirmeVerisi TarifAra(List<ObjeVerisi> elimizdekiMalzemeler)
    {
        foreach (var tarif in tumTarifler)
        {
            if (tarif.gerekenMalzemeler.Count != elimizdekiMalzemeler.Count) continue;

            List<ObjeVerisi> tempElimizdekiler = new List<ObjeVerisi>(elimizdekiMalzemeler);
            bool tarifUygun = true;

            foreach (var gereken in tarif.gerekenMalzemeler)
            {
                if (tempElimizdekiler.Contains(gereken)) tempElimizdekiler.Remove(gereken);
                else { tarifUygun = false; break; }
            }
            if (tarifUygun) return tarif;
        }
        return null;
    }

    public void OtomatikTarifKontrolu(PlaceableObject merkezObje) { }

    private void BinaOlustur(GridCell hedefHucre, ObjeVerisi binaVerisi)
    {
        Vector3 pos = gridManager.grid.GetCellCenterWorld(hedefHucre.cellPosition);
        GameObject yeniBina = Instantiate(binaVerisi.objePrefab, pos, binaVerisi.objePrefab.transform.rotation);
        PlaceableObject po = yeniBina.GetComponent<PlaceableObject>();

        po.verisi = binaVerisi;
        po.currentCell = hedefHucre;
        hedefHucre.currentObject = po;

        // 1 hareket hakkı veriyoruz
        po.hareketHakki = 1; 
        
        po.transform.position = pos + Vector3.up * po.heightOffset;
        po.BoyutuGuncelle();
        po.SetPreviewMode(false);

        // --- DEĞİŞİKLİK BURADA ---
        // po.PlayMergeAnimation();  <-- BU SATIR SİLİNDİ
        // Artık bina oluşurken animasyon oynamayacak, direkt duracak.
        
        if (CollectionManager.Instance != null)
            CollectionManager.Instance.ObjeAcildi(binaVerisi.collectionID);
        else
        {
            string key = "Collection_" + binaVerisi.collectionID;
            if (PlayerPrefs.GetInt(key, 0) == 0) { PlayerPrefs.SetInt(key, 1); PlayerPrefs.Save(); }
        }
        
        GameManager.Instance.UretimYapildi(po.verisi, po.transform.position);
    }
}