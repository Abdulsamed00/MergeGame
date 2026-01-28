using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BirlestirmeYoneticisi : MonoBehaviour
{
    public static BirlestirmeYoneticisi Instance;
    public List<BirlestirmeVerisi> tumTarifler;
    public GridManager gridManager;

    [Header("Animasyon Zamanlaması")]
    public float mergeBeklemeSuresi = 0.5f; // Merge ile Spawn arasındaki minik boşluk

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
        if (GameManager.Instance == null || GameManager.Instance.SuankiLevelData == null) return false;
        
        ObjeVerisi sinir = GameManager.Instance.SuankiLevelData.izinVerilenEnUstObje;
        if (sinir != null && sonucObjesi.objeSeviyesi > sinir.objeSeviyesi)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayMergeError();
            return true;
        }
        return false;
    }

    // Gerçek İşlem: Birleştirmeyi Uygula
    public bool ManuelBirlestirme(PlaceableObject elimizdeki, PlaceableObject yerdeki)
    {
        if (SeviyeSiniriAsiliyorMu(yerdeki.verisi)) return false;

        List<ObjeVerisi> toplamMalzemeler = new List<ObjeVerisi>();
        toplamMalzemeler.AddRange(elimizdeki.icindekiMalzemeler);
        toplamMalzemeler.AddRange(yerdeki.icindekiMalzemeler);

        BirlestirmeVerisi uygunTarif = TarifAra(toplamMalzemeler);
        GridCell hedefHucre = yerdeki.currentCell;

        // A) TARİF İLE BİRLEŞME (Bina Oluşumu)
        if (uygunTarif != null)
        {
            if (SeviyeSiniriAsiliyorMu(uygunTarif.sonucObjesi)) return false;

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayMerge(); 

            // Eski objeleri yok et
            if (yerdeki != null) Destroy(yerdeki.gameObject);
            
            BinaOlustur(hedefHucre, uygunTarif.sonucObjesi);
            return true;
        }

        // B) AYNI TÜR YIĞINLAMA (Stack)
        if (elimizdeki.verisi == yerdeki.verisi)
        {
            yerdeki.icindekiMalzemeler.AddRange(elimizdeki.icindekiMalzemeler);
            
            yerdeki.BoyutuGuncelle();
            yerdeki.hareketHakki = 1; 
            
            // Stack Animasyonu
            if (yerdeki != null) yerdeki.PlayStackAnimation(); 

            if (AudioManager.Instance != null) 
                AudioManager.Instance.PlayButtonClick();

            return true; 
        }

        return false;
    }

    BirlestirmeVerisi TarifAra(List<ObjeVerisi> malzemeler)
    {
        foreach (var tarif in tumTarifler)
        {
            if (tarif.gerekenMalzemeler.Count != malzemeler.Count) continue;

            List<ObjeVerisi> kopyaMalzemeler = new List<ObjeVerisi>(malzemeler);
            bool tarifUygun = true;

            foreach (var gereken in tarif.gerekenMalzemeler)
            {
                if (kopyaMalzemeler.Contains(gereken))
                {
                    kopyaMalzemeler.Remove(gereken);
                }
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

        // --- KRİTİK DÜZELTME: GÖRÜNMEZLİK ---
        // Obje oluşur oluşmaz boyutunu sıfırla ki ekranda "Pat" diye belirip durmasın.
        // Animasyon onu büyütecek.
        po.transform.localScale = Vector3.zero; 
        // -------------------------------------

        po.verisi = binaVerisi;
        po.currentCell = hedefHucre;
        hedefHucre.currentObject = po;

        po.hareketHakki = 1; 
        po.transform.position = pos + Vector3.up * po.heightOffset;
        
        po.icindekiMalzemeler.Clear(); 
        
        // Burada BoyutuGuncelle çağırmıyoruz çünkü scale'i 0 yaptık, bozmasın.
        po.SetPreviewMode(false);

        // --- SIRALI ANİMASYON ---
        StartCoroutine(MergeVeSpawnSirasi(po));
        // ------------------------
        
        if (CollectionManager.Instance != null)
            CollectionManager.Instance.ObjeAcildi(binaVerisi.collectionID);
        else
        {
            string key = "Collection_" + binaVerisi.collectionID;
            if (PlayerPrefs.GetInt(key, 0) == 0)
            {
                PlayerPrefs.SetInt(key, 1);
                PlayerPrefs.Save();
            }
        }

        if(GameManager.Instance != null)
            GameManager.Instance.UretimYapildi(binaVerisi, pos);
    }

    IEnumerator MergeVeSpawnSirasi(PlaceableObject po)
    {
        // 1. Bekle (Eski objelerin yok oluşunu sindirmek için)
        yield return new WaitForSeconds(mergeBeklemeSuresi);

        // 2. Spawn Animasyonu (Görünür Olma)
        // Obje şu an Scale 0 (Görünmez). Spawn animasyonu onu 0 -> 1 yapacak.
        po.PlaySpawnAnimation();
        
        // Not: Merge animasyonunu burada çağırmıyoruz çünkü Merge zıplama efektidir.
        // Görünmeyen objeyi zıplatmak işe yaramaz. Önce doğsun (Spawn), sonra gerekirse zıplar.
        // Ama senin "Spawn en son olsun" isteğini görsel olarak en iyi böyle karşılıyoruz:
        // Önceki objeler birleşti (yok oldu) -> Kısa sessizlik -> Yeni bina doğdu (Spawn).
    }
}