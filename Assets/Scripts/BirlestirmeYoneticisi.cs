using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirlestirmeYoneticisi : MonoBehaviour
{
    public List<BirlestirmeVerisi> tumTarifler;

    public void BirlestirmeKontrol(int x, int y, ObjeVerisi koyulanObje)
    {
        List<ObjeVerisi> komsular = KomsulariGetir(x, y);
        
        komsular.Add(koyulanObje);
        
        //eldeki objeler tarife uyuyor mu orayı kontrol etme kısmı
        foreach (var tarif in tumTarifler)
        {
            if (TarifeUyuyorMu(komsular, tarif))
            {
                Debug.Log("Birleşme oldu");
                return;
            }
        }
    }

    // eldeki malzeme tarife yeterli mi orayı kontrol etme kısmı
    private bool TarifeUyuyorMu(List<ObjeVerisi> eldekiMalzemeleri, BirlestirmeVerisi tarif)
    {
        List<ObjeVerisi> geciciListe = new List<ObjeVerisi>(eldekiMalzemeleri);

        //tarifteki her malzemeyi kontrol ediyoruz
        foreach (var gereken in tarif.gerekenMalzemeler)
        {
            //eğer listede varsa kaldırıyoruz
            if (geciciListe.Contains(gereken))
            {
                geciciListe.Remove(gereken);
            }
            else
            {
                return false;
            }
        }
        return true;
    }
    
    private List<ObjeVerisi> KomsulariGetir(int x, int y)
    {
        // Gridci arkadaşın kodu bitince burayı dolduracağız.
        return new List<ObjeVerisi>();
    }
}
