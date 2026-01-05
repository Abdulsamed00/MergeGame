using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BirlestirmeYoneticisi : MonoBehaviour
{
    public List<BirlestirmeVerisi> tumTarifler;
    public GridManager gridManager;
    
    [Header("Animasyon Ayarları")]
    public float mergeAnimDuration = 0.5f; // Objelerin küçülerek yok olma süresi

    // Seviye Sınırı Kontrolü
    private bool SeviyeSiniriAsiliyorMu(ObjeVerisi sonucObjesi)
    {
        LevelData currentLevel = GameManager.Instance.SuankiLevelData;
        if (currentLevel.izinVerilenEnUstObje == null) return false;
        if (sonucObjesi.objeSeviyesi > currentLevel.izinVerilenEnUstObje.objeSeviyesi) return true;
        return false;
    }

    // --- MANUEL BİRLEŞTİRME ---
    public bool ManuelBirlestirme(PlaceableObject elimizdeki, PlaceableObject yerdeki)
    {
        if (elimizdeki.verisi != yerdeki.verisi) return false;

        BirlestirmeVerisi gecerliTarif = null;
        foreach (var tarif in tumTarifler)
        {
            if (tarif.gerekenMalzemeler.Contains(elimizdeki.verisi))
            {
                gecerliTarif = tarif;
                break;
            }
        }
        if (gecerliTarif == null) return false; 

        if (gecerliTarif.sonucObjesi != null)
        {
            if (SeviyeSiniriAsiliyorMu(gecerliTarif.sonucObjesi)) return false; 
        }

        StartCoroutine(BirlestirmeSureci(elimizdeki, yerdeki, gecerliTarif));
        return true;
    }

    // --- ANİMASYONLU SÜREÇ ---
    private IEnumerator BirlestirmeSureci(PlaceableObject elimizdeki, PlaceableObject yerdeki, BirlestirmeVerisi tarif)
    {
        if(PlacementManager.Instance != null) PlacementManager.Instance.SetInputLock(true);

        int toplamMalzeme = elimizdeki.icindekiMalzemeler.Count + yerdeki.icindekiMalzemeler.Count;
        int gerekenMalzeme = tarif.gerekenMalzemeler.Count; 

        // 1. Elimizdeki objeyi hemen yok et (Görsel olarak diğerinin içine girmiş gibi)
        Destroy(elimizdeki.gameObject); 

        // --- SENARYO A: YIĞINLAMA (STACK - 2. Obje) ---
        if (toplamMalzeme < gerekenMalzeme)
        {
            yerdeki.icindekiMalzemeler.AddRange(elimizdeki.icindekiMalzemeler);
            
            // DÜZELTME: Artık Spawn DEĞİL, Stack animasyonu çağırıyoruz.
            // Bu animasyon objeyi şişirip (Pop efekti) yerine oturtacak.
            yerdeki.PlayStackAnimation();

            // Animasyonun "şişme" kısmı görünmesi için biraz bekle
            yield return new WaitForSeconds(0.15f);

            // Sonra boyutu kod tarafında da güncelle
            yerdeki.BoyutuGuncelle();
            yerdeki.hareketHakki = 1; 
        }
        // --- SENARYO B: DÖNÜŞÜM (MERGE - 3. Obje) ---
        else
        {
            // 1. Yerdeki obje "Merge" animasyonunu oynatsın (Küçülerek yok olma)
            // Not: Animasyon klibinizde Scale'i 1 -> 0 yapmalısınız.
            yerdeki.PlayMergeAnimation();
            
            // 2. Animasyonun bitmesini bekle
            yield return new WaitForSeconds(mergeAnimDuration);

            // 3. Eski objeyi tamamen yok et
            GridCell hedefHucre = yerdeki.currentCell;
            hedefHucre.currentObject = null; 
            Destroy(yerdeki.gameObject);
            
            // 4. Yeni binayı oluştur (Spawn animasyonu otomatik çalışacak)
            BinaOlustur(hedefHucre, tarif.sonucObjesi);
        }

        if(PlacementManager.Instance != null) PlacementManager.Instance.SetInputLock(false);
        GameManager.Instance.HamleBittiKontrolu();
    }

    private void BinaOlustur(GridCell hedefHucre, ObjeVerisi binaVerisi)
    {
        Vector3 pos = gridManager.grid.GetCellCenterWorld(hedefHucre.cellPosition);
        
        // Yeni obje doğar doğmaz Animator'daki "Entry -> Spawn" sayesinde büyüme animasyonuna başlar.
        GameObject yeniBina = Instantiate(binaVerisi.objePrefab, pos, Quaternion.identity);
        PlaceableObject po = yeniBina.GetComponent<PlaceableObject>();
        
        // Obje doğduğunda boyutunu 0 yapıyoruz ki "Spawn" animasyonu 0'dan büyütsün.
        po.transform.localScale = Vector3.zero;

        po.verisi = binaVerisi;
        po.currentCell = hedefHucre;
        hedefHucre.currentObject = po;
        po.hareketHakki = 1; 
        
        po.transform.position = pos + Vector3.up * po.heightOffset;
        po.SetPreviewMode(false);
        
        // Spawn trigger'ını tetikle (Eğer Entry->Spawn yapmadıysanız bu garanti olur)
        po.PlaySpawnAnimation();
        
        GameManager.Instance.UretimYapildi(po.verisi, po.transform.position);
        
        // Zincirleme reaksiyon kontrolü
        OtomatikTarifKontrolu(po);
    }
    
    // --- OTOMATİK KONTROL ---
    public void OtomatikTarifKontrolu(PlaceableObject merkezObje)
    {
        if (merkezObje == null) return;
        StartCoroutine(OtomatikKontrolRoutine(merkezObje));
    }

    private IEnumerator OtomatikKontrolRoutine(PlaceableObject merkezObje)
    {
        yield return new WaitForSeconds(0.1f);

        foreach (var tarif in tumTarifler)
        {
            if (!tarif.gerekenMalzemeler.Contains(merkezObje.verisi)) continue;
            if (TarifSadeceAyniTurdenMi(tarif)) continue; 
            if (SeviyeSiniriAsiliyorMu(tarif.sonucObjesi)) continue; 

            List<PlaceableObject> silinecekler = TarifeUygunParcalariTopla(merkezObje, tarif);

            if (silinecekler != null)
            {
                if(PlacementManager.Instance != null) PlacementManager.Instance.SetInputLock(true);

                foreach (var parca in silinecekler)
                {
                    parca.PlayMergeAnimation();
                }

                yield return new WaitForSeconds(mergeAnimDuration);

                GridCell insaAlani = merkezObje.currentCell; 
                foreach (var parca in silinecekler)
                {
                    if (parca.currentCell != null) parca.currentCell.currentObject = null;
                    Destroy(parca.gameObject);
                }

                BinaOlustur(insaAlani, tarif.sonucObjesi);
                
                if(PlacementManager.Instance != null) PlacementManager.Instance.SetInputLock(false);
                yield break; 
            }
        }
    }

    // --- YARDIMCI FONKSİYONLAR (DEĞİŞMEDİ) ---
    private List<PlaceableObject> TarifeUygunParcalariTopla(PlaceableObject merkez, BirlestirmeVerisi tarif)
    {
        List<ObjeVerisi> kalanIhtiyac = new List<ObjeVerisi>(tarif.gerekenMalzemeler);
        List<PlaceableObject> toplananlar = new List<PlaceableObject>();
        Queue<PlaceableObject> gezilecekler = new Queue<PlaceableObject>();
        HashSet<PlaceableObject> ziyaretEdilenler = new HashSet<PlaceableObject>();

        List<ObjeVerisi> merkezIcerik = new List<ObjeVerisi>(merkez.icindekiMalzemeler);
        foreach(var item in merkezIcerik) {
            if(kalanIhtiyac.Contains(item)) kalanIhtiyac.Remove(item);
            else return null; 
        }
        toplananlar.Add(merkez);
        ziyaretEdilenler.Add(merkez);
        if (kalanIhtiyac.Count == 0) return toplananlar;
        gezilecekler.Enqueue(merkez);
        Vector3Int[] yonler = { Vector3Int.right, Vector3Int.left, Vector3Int.forward, Vector3Int.back };

        while (gezilecekler.Count > 0) {
            PlaceableObject suanki = gezilecekler.Dequeue();
            if (kalanIhtiyac.Count == 0) break;
            foreach (var yon in yonler) {
                GridCell k = gridManager.GetCell(suanki.currentCell.cellPosition + yon);
                if (k != null && !k.IsEmpty()) {
                    PlaceableObject komsu = k.currentObject;
                    if (!ziyaretEdilenler.Contains(komsu)) {
                        List<ObjeVerisi> komsuIcerik = new List<ObjeVerisi>(komsu.icindekiMalzemeler);
                        List<ObjeVerisi> testIhtiyac = new List<ObjeVerisi>(kalanIhtiyac);
                        bool komsuTamUygun = true;
                        foreach(var mal in komsuIcerik) {
                            if(testIhtiyac.Contains(mal)) testIhtiyac.Remove(mal);
                            else { komsuTamUygun = false; break; }
                        }
                        if (komsuTamUygun) {
                            ziyaretEdilenler.Add(komsu);
                            toplananlar.Add(komsu);
                            gezilecekler.Enqueue(komsu); 
                            kalanIhtiyac = testIhtiyac; 
                        }
                        if (kalanIhtiyac.Count == 0) goto AramaBitti;
                    }
                }
            }
        }
        AramaBitti:
        if (kalanIhtiyac.Count == 0) return toplananlar;
        else return null;
    }

    private bool TarifSadeceAyniTurdenMi(BirlestirmeVerisi tarif)
    {
        if (tarif.gerekenMalzemeler.Count == 0) return false;
        ObjeVerisi ilkMalzeme = tarif.gerekenMalzemeler[0];
        foreach (var malzeme in tarif.gerekenMalzemeler)
        {
            if (malzeme != ilkMalzeme) return false;
        }
        return true;
    }
}