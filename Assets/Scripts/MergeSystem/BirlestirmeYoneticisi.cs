using System.Collections.Generic;
using UnityEngine;

public class BirlestirmeYoneticisi : MonoBehaviour
{
    public List<BirlestirmeVerisi> tumTarifler;
    public GridManager gridManager;
    
    public PlaceableObject sonUretilenObje; 

    // --- MANUEL YIĞINLAMA ---
    public int YiginlamaKontrol(PlaceableObject elimizdeki, PlaceableObject yerdeki)
    {
        sonUretilenObje = null;

        if (elimizdeki.verisi != yerdeki.verisi) return 0;

        BirlestirmeVerisi gecerliTarif = null;
        foreach (var tarif in tumTarifler)
        {
            if (tarif.gerekenMalzemeler.Count > 0 && tarif.gerekenMalzemeler[0] == elimizdeki.verisi)
            {
                gecerliTarif = tarif;
                break;
            }
        }

        if (gecerliTarif == null) return 0;

        int gereken = gecerliTarif.gerekenMalzemeler.Count;
        int toplam = elimizdeki.icindekiMalzemeler.Count + yerdeki.icindekiMalzemeler.Count;

        if (toplam < gereken)
        {
            // Yığınlama (Henüz bina olmadı, malzeme birleşti)
            yerdeki.icindekiMalzemeler.AddRange(elimizdeki.icindekiMalzemeler);
            yerdeki.hareketHakki = 1; 
            yerdeki.BoyutuGuncelle();
            yerdeki.SetPreviewMode(false); 
            return 1; 
        }
        else if (toplam >= gereken)
        {
            // Dönüşüm (Bina oluştu!)
            GridCell hedefHucre = yerdeki.currentCell;
            Vector3 pos = gridManager.grid.GetCellCenterWorld(hedefHucre.cellPosition);

            GameObject bina = Instantiate(gecerliTarif.sonucObjesi.objePrefab, pos, Quaternion.identity);
            PlaceableObject po = bina.GetComponent<PlaceableObject>();
            
            po.verisi = gecerliTarif.sonucObjesi;
            po.currentCell = hedefHucre;
            po.kilitliMi = true; 
            po.hareketHakki = 0; 
            
            po.transform.position = pos + Vector3.up * po.heightOffset;
            po.SetPreviewMode(false); 
            
            sonUretilenObje = po;
            
            // --- DÜZELTME 1 BURADA ---
            // 'yeniOlusanObje' yerine 'po' kullandık.
            GameManager.Instance.UretimYapildi(po.verisi);
            
            return 2;
        }

        return 0;
    }

    // --- OTOMATİK ZİNCİRLEME KONTROL ---
    public void OtomatikKomsulukKontrolu(PlaceableObject merkezObje)
    {
        if (merkezObje == null) return;

        if (!merkezObje.kilitliMi) return;

        foreach (var tarif in tumTarifler)
        {
            if (!tarif.gerekenMalzemeler.Contains(merkezObje.verisi)) continue;

            List<PlaceableObject> baglantiliObjeler = BaglantiliKumeBul(merkezObje, tarif);

            if (HavuzTarifiKarsiliyorMu(baglantiliObjeler, tarif))
            {
                Debug.Log("OTOMATİK BİNA OLUŞUYOR: " + tarif.sonucObjesi.objeAdi);

                List<PlaceableObject> silinecekler = MalzemeleriSec(baglantiliObjeler, tarif);
                
                if (!silinecekler.Contains(merkezObje)) silinecekler.Add(merkezObje);

                foreach (var sil in silinecekler)
                {
                    sil.currentCell.currentObject = null;
                    Destroy(sil.gameObject);
                }

                GridCell merkezHucre = merkezObje.currentCell;
                Vector3 pos = gridManager.grid.GetCellCenterWorld(merkezHucre.cellPosition);

                GameObject bina = Instantiate(tarif.sonucObjesi.objePrefab, pos, Quaternion.identity);
                PlaceableObject po = bina.GetComponent<PlaceableObject>();

                po.verisi = tarif.sonucObjesi;
                po.currentCell = merkezHucre;
                po.kilitliMi = true; 
                po.hareketHakki = 0;
                
                merkezHucre.currentObject = po;
                po.transform.position = pos + Vector3.up * po.heightOffset;
                po.BoyutuGuncelle();
                po.SetPreviewMode(false);
 
                // --- DÜZELTME 2 BURADA ---
                // Burada da 'yeniOlusanObje' yerine 'po' kullandık.
                GameManager.Instance.UretimYapildi(po.verisi);
                
                return;
            }
        }
    }

    // --- YARDIMCI FONKSİYONLAR ---
    private List<PlaceableObject> BaglantiliKumeBul(PlaceableObject baslangic, BirlestirmeVerisi tarif)
    {
        List<PlaceableObject> kume = new List<PlaceableObject>();
        Queue<PlaceableObject> gezilecekler = new Queue<PlaceableObject>();
        HashSet<PlaceableObject> ziyaretEdilenler = new HashSet<PlaceableObject>();

        gezilecekler.Enqueue(baslangic);
        ziyaretEdilenler.Add(baslangic);

        Vector3Int[] yonler = { Vector3Int.right, Vector3Int.left, Vector3Int.forward, Vector3Int.back };

        while (gezilecekler.Count > 0)
        {
            PlaceableObject suanki = gezilecekler.Dequeue();
            
            if (tarif.gerekenMalzemeler.Contains(suanki.verisi))
            {
                kume.Add(suanki);
            }
            
            foreach (var yon in yonler)
            {
                GridCell k = gridManager.GetCell(suanki.currentCell.cellPosition + yon);
                if (k != null && !k.IsEmpty()) 
                {
                    PlaceableObject komsu = k.currentObject;
                    if (!ziyaretEdilenler.Contains(komsu) && tarif.gerekenMalzemeler.Contains(komsu.verisi))
                    {
                        ziyaretEdilenler.Add(komsu);
                        gezilecekler.Enqueue(komsu);
                    }
                }
            }
        }
        return kume;
    }

    private bool HavuzTarifiKarsiliyorMu(List<PlaceableObject> objeler, BirlestirmeVerisi tarif)
    {
        List<ObjeVerisi> havuz = new List<ObjeVerisi>();
        foreach(var obj in objeler) havuz.Add(obj.verisi);

        List<ObjeVerisi> gerekenler = new List<ObjeVerisi>(tarif.gerekenMalzemeler);

        foreach (var malzeme in havuz)
        {
            if (gerekenler.Contains(malzeme))
            {
                gerekenler.Remove(malzeme);
            }
        }
        return gerekenler.Count == 0;
    }

    private List<PlaceableObject> MalzemeleriSec(List<PlaceableObject> adaylar, BirlestirmeVerisi tarif)
    {
        List<PlaceableObject> silinecekler = new List<PlaceableObject>();
        List<ObjeVerisi> gerekenler = new List<ObjeVerisi>(tarif.gerekenMalzemeler);

        foreach (var aday in adaylar)
        {
            if (gerekenler.Contains(aday.verisi))
            {
                gerekenler.Remove(aday.verisi);
                silinecekler.Add(aday);
            }
            if (gerekenler.Count == 0) break;
        }
        return silinecekler;
    }
}