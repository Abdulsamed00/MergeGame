using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirlestirmeYoneticisi : MonoBehaviour
{
    public List<BirlestirmeVerisi> tumTarifler;
    public GridManager gridManager;

    // Sonsuz döngü engeli için liste (Arama sırasında kullanılır)
    private List<PlaceableObject> ziyaretEdilenler;
    
    public PlaceableObject sonUretilenObje; 

    public void BirlestirmeKontrol(int x, int y, PlaceableObject koyulanObjeNesnesi)
    {
        sonUretilenObje = null; // Her yeni hamlede eski üretim verisini temizle

        // Önce koyulan objenin kendi türündeki kümesini bul (Örn: 2 İnşaat Alanı)
        List<PlaceableObject> anaKume = BaglantiliObjeleriBul(x, y, koyulanObjeNesnesi.verisi);
        
        // Bu listeyi Tüm Adaylar olarak başlatalım
        List<PlaceableObject> tumAdaylar = new List<PlaceableObject>(anaKume);

        // Ana kümedeki her bir parçanın komşularına bakıcaz
        // Eğer komşuda farklı bir tür varsa, onun da kümesini bulup tüm adaylara eklicez
        // 2 İnşaat Alanı 1 Demir Kolonu tek bir listede toplanmış olucak
        
        foreach (var parca in anaKume)
        {
            KomsularinKumesiniEkle(parca, tumAdaylar);
        }

        // Tarifleri kontrol et
        foreach (var tarif in tumTarifler)
        {
            if (TarifeUyuyorMu(tumAdaylar, tarif))
            {
                Debug.Log("Birleşme oldu: " + tarif.sonucObjesi.objeAdi);
                BirlestirmeyiUygula(tumAdaylar, tarif, x, y);
                return;
            }
        }
    }
    
    private void KomsularinKumesiniEkle(PlaceableObject merkezObje, List<PlaceableObject> tumAdaylar)
    {
        // Grid koordinatını alıyoruz
        Vector3Int merkezPos = merkezObje.currentCell.cellPosition;
        Vector3Int[] yonler = { Vector3Int.right, Vector3Int.left, Vector3Int.forward, Vector3Int.back };

        foreach (var yon in yonler)
        {
            Vector3Int bakilanPos = merkezPos + yon;
            GridCell hucre = gridManager.GetCell(bakilanPos);

            // Hücre geçerli mi ve dolu mu
            if (hucre != null && !hucre.IsEmpty())
            {
                PlaceableObject komsu = hucre.currentObject;

                // Eğer bu komşu zaten listemizde yoksa demek ki farklı bir tür veya yeni bir küme
                if (!tumAdaylar.Contains(komsu))
                {
                    // O komşunun dahil olduğu kümeyi bul - Yanımızdaki Demir Kolonu gibi
                    List<PlaceableObject> komsuKume = BaglantiliObjeleriBul(bakilanPos.x, bakilanPos.z, komsu.verisi);
                    
                    // Bulunan kümeyi ana listeye dahil et
                    foreach (var k in komsuKume)
                    {
                        if (!tumAdaylar.Contains(k)) tumAdaylar.Add(k);
                    }
                }
            }
        }
    }

    private void BirlestirmeyiUygula(List<PlaceableObject> harcanacaklar, BirlestirmeVerisi tarif, int x, int y)
    {
        List<ObjeVerisi> gerekenler = new List<ObjeVerisi>(tarif.gerekenMalzemeler);
        List<PlaceableObject> silinecekler = new List<PlaceableObject>();

        // Reçete kadarını seç
        foreach (var gereken in gerekenler)
        {
             PlaceableObject silinecek = harcanacaklar.Find(obj => obj.verisi == gereken && !silinecekler.Contains(obj));
             if(silinecek != null) silinecekler.Add(silinecek);
        }

        // Seçilenleri yok et
        foreach (var nesne in silinecekler)
        {
            nesne.currentCell.currentObject = null; 
            Destroy(nesne.gameObject);
        }

        // YENİYİ OLUŞTUR 
        Vector3Int merkezPos = new Vector3Int(x, 0, y);
        Vector3 dunyaPozisyonu = gridManager.grid.GetCellCenterWorld(merkezPos);
        
        GameObject yeniObje = Instantiate(tarif.sonucObjesi.objePrefab, dunyaPozisyonu, Quaternion.identity);
        PlaceableObject po = yeniObje.GetComponent<PlaceableObject>();
        po.verisi = tarif.sonucObjesi;
        
        // objeyi Gride kilitlemiyoruz değişkene atıyoruz
        sonUretilenObje = po;
        
        // hareket ettirilebilir modda bırakıyoruz
        po.SetPreviewMode(true); 
    }

    private List<PlaceableObject> BaglantiliObjeleriBul(int baslangicX, int baslangicY, ObjeVerisi arananTur)
    {
        ziyaretEdilenler = new List<PlaceableObject>(); 
        List<PlaceableObject> sonucListesi = new List<PlaceableObject>();
        ZincirlemeAra(baslangicX, baslangicY, arananTur, sonucListesi);
        return sonucListesi;
    }

    // Sadece aynı türleri bulur
    private void ZincirlemeAra(int x, int y, ObjeVerisi arananTur, List<PlaceableObject> liste)
    {
        Vector3Int merkezPos = new Vector3Int(x, 0, y);
        GridCell hucre = gridManager.GetCell(merkezPos);

        if (hucre == null || hucre.IsEmpty()) return;

        PlaceableObject suankiObje = hucre.currentObject;

        if (ziyaretEdilenler.Contains(suankiObje)) return;
        
        // TÜR KONTROLÜ - Sadece aranan türle aynıysa devam et
        if (suankiObje.verisi != arananTur) return;

        ziyaretEdilenler.Add(suankiObje); 
        liste.Add(suankiObje);            

        // 4 Yöne yayıl
        ZincirlemeAra(x + 1, y, arananTur, liste);
        ZincirlemeAra(x - 1, y, arananTur, liste);
        ZincirlemeAra(x, y + 1, arananTur, liste);
        ZincirlemeAra(x, y - 1, arananTur, liste);
    }

    private bool TarifeUyuyorMu(List<PlaceableObject> eldekiNesneler, BirlestirmeVerisi tarif)
    {
        List<ObjeVerisi> kontrolListesi = new List<ObjeVerisi>();
        foreach(var nesne in eldekiNesneler) kontrolListesi.Add(nesne.verisi);

        List<ObjeVerisi> gerekenler = new List<ObjeVerisi>(tarif.gerekenMalzemeler);
        
        foreach (var eldeki in kontrolListesi)
        {
            if (gerekenler.Contains(eldeki)) gerekenler.Remove(eldeki);
        }
        return gerekenler.Count == 0;
    }
}
