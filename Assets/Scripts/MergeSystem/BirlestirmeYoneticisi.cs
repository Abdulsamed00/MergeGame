using System.Collections.Generic;
using UnityEngine;
using System.Linq; 
using System.Collections; // Coroutine için gerekli

public class BirlestirmeYoneticisi : MonoBehaviour
{
    public List<BirlestirmeVerisi> tumTarifler;
    public GridManager gridManager;

    // --- 1. UYUMLULUK KONTROLÜ ---
    public bool UygunMu(PlaceableObject eldeTutulan, PlaceableObject yerdeki)
    {
        if (eldeTutulan == null || yerdeki == null) return false;

        if (eldeTutulan.verisi == yerdeki.verisi) return true;

        foreach (var tarif in tumTarifler)
        {
            bool eldekiLazim = tarif.gerekenMalzemeler.Contains(eldeTutulan.verisi);
            bool yerdekiLazim = tarif.gerekenMalzemeler.Contains(yerdeki.verisi);
            if (eldekiLazim && yerdekiLazim) return true;
        }

        return false;
    }

    // --- 2. ÇOKLU BİRLEŞTİRME ---
    public bool CokluBirlestirme(List<PlaceableObject> gelenObjeler, PlaceableObject hedefObje)
    {
        List<PlaceableObject> tumParcalar = new List<PlaceableObject>(gelenObjeler);
        tumParcalar.Add(hedefObje);

        // 1. Bina Tarifi Kontrolü
        BirlestirmeVerisi uygunTarif = TarifBulVeDogrula(tumParcalar);

        if (uygunTarif != null)
        {
            // BİNA OLUŞTUR (Animasyonlu Süreç Başlat)
            if (UndoManager.Instance != null) UndoManager.Instance.SaveState();
            
            // Coroutine başlatıyoruz (Zamanlı işlem)
            StartCoroutine(MergeProcess(tumParcalar, hedefObje.currentCell, uygunTarif.sonucObjesi));
            return true;
        }
        else
        {
            // 2. Yığınlama (Stack) Kontrolü
            bool hepsiAyni = tumParcalar.All(x => x.verisi == tumParcalar[0].verisi);

            if (hepsiAyni)
            {
                ObjeVerisi buMalzeme = tumParcalar[0].verisi;
                int toplamAdet = 0;
                foreach(var p in tumParcalar) 
                    toplamAdet += (p.icindekiMalzemeler.Count > 0 ? p.icindekiMalzemeler.Count : 1);

                int gerekenMaksimum = GetMaxRequiredCount(buMalzeme);

                if (toplamAdet < gerekenMaksimum)
                {
                    if (UndoManager.Instance != null) UndoManager.Instance.SaveState();

                    // Stack animasyonu karmaşık olmasın, direkt birleşsin
                    foreach (var gelen in gelenObjeler)
                    {
                        hedefObje.icindekiMalzemeler.AddRange(gelen.icindekiMalzemeler);
                        if (gelen.currentCell != null) gelen.currentCell.currentObject = null;
                        Destroy(gelen.gameObject);
                    }
                    
                    hedefObje.BoyutuGuncelle();
                    hedefObje.hareketHakki = 1; 
                    return true;
                }
            }
        }
        
        return false;
    }

    // --- YENİ: ANİMASYONLU BİRLEŞTİRME SÜRECİ ---
    private IEnumerator MergeProcess(List<PlaceableObject> parcalar, GridCell hedefHucre, ObjeVerisi sonucVerisi)
    {
        // 1. Tüm parçaların "Merge" (Küçülme) animasyonunu tetikle
        foreach (var parca in parcalar)
        {
            if (parca != null)
            {
                // Colliderları kapat ki oyuncu yanlışlıkla tekrar tıklamasın
                foreach(var col in parca.GetComponentsInChildren<Collider>()) col.enabled = false;
                
                // Animator üzerinden küçülme animasyonunu oynat
                parca.PlayMergeAnimation();
            }
        }

        // 2. Animasyonun bitmesini bekle (Örneğin 0.4 saniye)
        // Animasyon klibinizin süresine göre burayı ayarlayın!
        yield return new WaitForSeconds(0.4f);

        // 3. Eski parçaları yok et
        TemizleVeYokEt(parcalar);

        // 4. Yeni binayı oluştur
        BinaOlustur(hedefHucre, sonucVerisi);
    }

    private void BinaOlustur(GridCell hedefHucre, ObjeVerisi binaVerisi)
    {
        Vector3 pos = gridManager.grid.GetCellCenterWorld(hedefHucre.cellPosition);
        GameObject yeniBina = Instantiate(binaVerisi.objePrefab, pos, Quaternion.identity);
        PlaceableObject po = yeniBina.GetComponent<PlaceableObject>();

        po.verisi = binaVerisi;
        po.currentCell = hedefHucre;
        hedefHucre.currentObject = po;
        po.hareketHakki = 1; 
        
        po.transform.position = pos + Vector3.up * po.heightOffset;
        
        // --- POP EFEKTİ ---
        // Yeni doğan objenin "Spawn" (Büyüme) animasyonunu tetikle
        po.PlaySpawnAnimation();
        po.SetPreviewMode(false);
        
        GameManager.Instance.UretimYapildi(po.verisi, po.transform.position);
    }

    // --- YARDIMCI FONKSİYONLAR ---
    private int GetMaxRequiredCount(ObjeVerisi malzeme)
    {
        int maxCount = 0;
        foreach (var tarif in tumTarifler)
        {
            int countInRecipe = tarif.gerekenMalzemeler.Count(x => x == malzeme);
            if (countInRecipe > maxCount) maxCount = countInRecipe;
        }
        return maxCount;
    }

    private BirlestirmeVerisi TarifBulVeDogrula(List<PlaceableObject> parcalar)
    {
        List<ObjeVerisi> elimizdekiMalzemeler = new List<ObjeVerisi>();
        foreach (var p in parcalar)
        {
            if (p.icindekiMalzemeler != null && p.icindekiMalzemeler.Count > 0)
                elimizdekiMalzemeler.AddRange(p.icindekiMalzemeler);
            else
                elimizdekiMalzemeler.Add(p.verisi);
        }

        foreach (var tarif in tumTarifler)
        {
            if (elimizdekiMalzemeler.Count != tarif.gerekenMalzemeler.Count) continue;
            if (IcerikBirebirAyniMi(elimizdekiMalzemeler, tarif.gerekenMalzemeler)) return tarif;
        }
        return null;
    }

    private bool IcerikBirebirAyniMi(List<ObjeVerisi> eldeki, List<ObjeVerisi> gereken)
    {
        List<ObjeVerisi> eldekiKopya = new List<ObjeVerisi>(eldeki);
        List<ObjeVerisi> gerekenKopya = new List<ObjeVerisi>(gereken);

        foreach (var item in gerekenKopya)
        {
            if (eldekiKopya.Contains(item)) eldekiKopya.Remove(item);
            else return false;
        }
        return eldekiKopya.Count == 0;
    }

    private void TemizleVeYokEt(List<PlaceableObject> objeler)
    {
        foreach(var obj in objeler)
        {
            if (obj != null)
            {
                if (obj.currentCell != null) obj.currentCell.currentObject = null;
                Destroy(obj.gameObject);
            }
        }
    }
    
    public void OtomatikTarifKontrolu(PlaceableObject merkezObje) { }
}