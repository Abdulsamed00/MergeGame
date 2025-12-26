using System.Collections.Generic;
using UnityEngine;

public class BirlestirmeYoneticisi : MonoBehaviour
{
    public List<BirlestirmeVerisi> tumTarifler;
    public GridManager gridManager;
    
    public PlaceableObject sonUretilenObje; 

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
            // --- YIĞINLAMA (1+1=2) ---
            yerdeki.icindekiMalzemeler.AddRange(elimizdeki.icindekiMalzemeler);
            
            // Hareket hakkı kazandı
            yerdeki.hareketHakki = 1; 
            
            // --- ANİMASYON YOK, BOYUT BÜYÜTME VAR ---
            yerdeki.BoyutuGuncelle(); // Scale 1.0 olacak
            yerdeki.SetPreviewMode(false); 

            return 1; 
        }
        else if (toplam >= gereken)
        {
            // --- DÖNÜŞÜM (Bina) ---
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
            return 2;
        }

        return 0;
    }

    public void BirlestirmeKontrol(int x, int y, PlaceableObject p) { }
}