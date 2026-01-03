using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BirlestirmeYoneticisi : MonoBehaviour
{
    public List<BirlestirmeVerisi> tumTarifler;
    public GridManager gridManager;
    
    // --- YENİ EKLENEN YARDIMCI FONKSİYON ---
    // Üretilecek obje, şu anki levelin sınırını aşıyor mu?
    private bool SeviyeSiniriAsiliyorMu(ObjeVerisi sonucObjesi)
    {
        LevelData currentLevel = GameManager.Instance.SuankiLevelData;
        
        // Eğer levelde bir sınır belirlenmemişse (null) her şeye izin ver
        if (currentLevel.izinVerilenEnUstObje == null) return false;

        // Eğer sonuç objesinin seviyesi, izin verilenin seviyesinden büyükse -> SINIR AŞILDI (TRUE)
        if (sonucObjesi.objeSeviyesi > currentLevel.izinVerilenEnUstObje.objeSeviyesi)
        {
            Debug.Log("Bu levelde bu binayı yapamazsınız! Sınır: " + currentLevel.izinVerilenEnUstObje.objeAdi);
            // Burada istersen oyuncuya "Henüz bu teknoloji yok" gibi bir UI uyarısı gösterebilirsin.
            return true;
        }

        return false;
    }

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

        // --- YENİ KONTROL ---
        // Eğer birleşme sonucunda bir bina oluşacaksa ve bu bina sınıra takılıyorsa iptal et
        if (gecerliTarif.sonucObjesi != null)
        {
             // Eğer tarif aynı türden yığınlama değilse (yani yeni bir bina üretiyorsa) kontrole gir
             // (Basit yığınlamada zaten sonuç objesi tariften gelmiyor, kendi büyüyor)
        }
        
        // *Dikkat: Senin yığınlama mantığın tariften bağımsız boyutu büyütüyor.
        // Ama "Bina Yap" kısmında tarif.sonucObjesi kullanıyorsun. Kontrolü oraya koyacağız.

        int toplam = elimizdeki.icindekiMalzemeler.Count + yerdeki.icindekiMalzemeler.Count;
        int gereken = gecerliTarif.gerekenMalzemeler.Count;

        if (toplam < gereken)
        {
            // Yığınlama (Burada sınır kontrolüne gerek yok, henüz dönüşmüyor)
            yerdeki.icindekiMalzemeler.AddRange(elimizdeki.icindekiMalzemeler);
            yerdeki.BoyutuGuncelle();
            yerdeki.hareketHakki = 1; 
            return true; 
        }
        else if (toplam >= gereken)
        {
            // --- BİNA OLUŞUYOR! SINIR KONTROLÜ BURADA ---
            if (SeviyeSiniriAsiliyorMu(gecerliTarif.sonucObjesi)) 
            {
                return false; // Sınırı aşıyor, birleştirme yapma!
            }

            GridCell hedefHucre = yerdeki.currentCell;
            hedefHucre.currentObject = null; 
            Destroy(yerdeki.gameObject);
            BinaOlustur(hedefHucre, gecerliTarif.sonucObjesi);
            return true;
        }

        return false;
    }

    public void OtomatikTarifKontrolu(PlaceableObject merkezObje)
    {
        if (merkezObje == null) return;

        foreach (var tarif in tumTarifler)
        {
            if (!tarif.gerekenMalzemeler.Contains(merkezObje.verisi)) continue;
            if (TarifSadeceAyniTurdenMi(tarif)) continue;

            // --- YENİ KONTROL ---
            if (SeviyeSiniriAsiliyorMu(tarif.sonucObjesi)) continue; // Sınır aşılıyorsa bu tarifi pas geç

            List<PlaceableObject> silinecekler = TarifeUygunParcalariTopla(merkezObje, tarif);

            if (silinecekler != null)
            {
                Debug.Log("OTOMATİK BİRLEŞME: " + tarif.sonucObjesi.objeAdi);
                GridCell insaAlani = merkezObje.currentCell; 

                foreach (var parca in silinecekler)
                {
                    if (parca.currentCell != null) parca.currentCell.currentObject = null;
                    Destroy(parca.gameObject);
                }

                BinaOlustur(insaAlani, tarif.sonucObjesi);
                return; 
            }
        }
    }

    // --- BU FONKSİYON HEM ARIYOR, HEM SEÇİYOR, HEM DE FAZLALIKLARI GÖRMEZDEN GELİYOR ---
    private List<PlaceableObject> TarifeUygunParcalariTopla(PlaceableObject merkez, BirlestirmeVerisi tarif)
    {
        // 1. İhtiyaç listesini oluştur
        List<ObjeVerisi> kalanIhtiyac = new List<ObjeVerisi>(tarif.gerekenMalzemeler);
        List<PlaceableObject> toplananlar = new List<PlaceableObject>();
        
        Queue<PlaceableObject> gezilecekler = new Queue<PlaceableObject>();
        HashSet<PlaceableObject> ziyaretEdilenler = new HashSet<PlaceableObject>();

        // 2. ÖNCE MERKEZ OBJEYİ İŞLE
        List<ObjeVerisi> merkezIcerik = new List<ObjeVerisi>(merkez.icindekiMalzemeler);
        foreach(var item in merkezIcerik)
        {
            if(kalanIhtiyac.Contains(item))
            {
                kalanIhtiyac.Remove(item);
            }
            else
            {
                return null; // Merkezde fazlalık var, iptal.
            }
        }
        
        toplananlar.Add(merkez);
        ziyaretEdilenler.Add(merkez);
        
        if (kalanIhtiyac.Count == 0) return toplananlar;

        gezilecekler.Enqueue(merkez);

        Vector3Int[] yonler = { Vector3Int.right, Vector3Int.left, Vector3Int.forward, Vector3Int.back };

        // 3. ARAMAYA BAŞLA
        while (gezilecekler.Count > 0)
        {
            PlaceableObject suanki = gezilecekler.Dequeue();
            if (kalanIhtiyac.Count == 0) break;

            foreach (var yon in yonler)
            {
                GridCell k = gridManager.GetCell(suanki.currentCell.cellPosition + yon);

                if (k != null && !k.IsEmpty())
                {
                    PlaceableObject komsu = k.currentObject;

                    if (!ziyaretEdilenler.Contains(komsu))
                    {
                        // Komşuyu sadece İŞİMİZE YARIYORSA alıyoruz
                        List<ObjeVerisi> komsuIcerik = new List<ObjeVerisi>(komsu.icindekiMalzemeler);
                        
                        List<ObjeVerisi> testIhtiyac = new List<ObjeVerisi>(kalanIhtiyac);
                        bool komsuTamUygun = true;

                        foreach(var mal in komsuIcerik)
                        {
                            if(testIhtiyac.Contains(mal))
                            {
                                testIhtiyac.Remove(mal);
                            }
                            else
                            {
                                komsuTamUygun = false;
                                break;
                            }
                        }

                        if (komsuTamUygun)
                        {
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
        po.BoyutuGuncelle();
        po.SetPreviewMode(false);
        
        GameManager.Instance.UretimYapildi(po.verisi, po.transform.position);
        
        OtomatikTarifKontrolu(po);
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