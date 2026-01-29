using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BirlestirmeYoneticisi : MonoBehaviour
{
    public static BirlestirmeYoneticisi Instance;

    [Header("Tarifler")]
    public List<BirlestirmeVerisi> tumTarifler;

    [Header("Referanslar")]
    public GridManager gridManager;

    [Header("Particle")]
    public GameObject mergeParticlePrefab;

    [Header("Animasyon Zamanlaması")]
    public float mergeBeklemeSuresi = 0.5f;

    private void Awake()
    {
        Instance = this;
    }

    public bool CanMerge(PlaceableObject elimizdeki, PlaceableObject yerdeki)
    {
        if (elimizdeki == null || yerdeki == null) return false;

        List<ObjeVerisi> toplam = new();
        toplam.AddRange(elimizdeki.icindekiMalzemeler);
        toplam.AddRange(yerdeki.icindekiMalzemeler);

        BirlestirmeVerisi tarif = TarifAra(toplam);
        if (tarif != null)
        {
            if (SeviyeSiniriAsiliyorMu(tarif.sonucObjesi)) return false;
            return true;
        }

        return elimizdeki.verisi == yerdeki.verisi;
    }

    public bool ManuelBirlestirme(PlaceableObject elimizdeki, PlaceableObject yerdeki)
    {
        GridCell hedefHucre = yerdeki.currentCell;
        Vector3 particlePos = gridManager.grid.GetCellCenterWorld(hedefHucre.cellPosition)
                              + Vector3.up * 0.6f;

        List<ObjeVerisi> toplam = new();
        toplam.AddRange(elimizdeki.icindekiMalzemeler);
        toplam.AddRange(yerdeki.icindekiMalzemeler);

        BirlestirmeVerisi tarif = TarifAra(toplam);

        if (tarif != null)
        {
            if (SeviyeSiniriAsiliyorMu(tarif.sonucObjesi)) return false;
            
            if (yerdeki != null)
                Destroy(yerdeki.gameObject);

            BinaOlustur(hedefHucre, tarif.sonucObjesi);
            return true;
        }

        if (elimizdeki.verisi == yerdeki.verisi)
        {
            yerdeki.icindekiMalzemeler.AddRange(elimizdeki.icindekiMalzemeler);
            yerdeki.BoyutuGuncelle();
            yerdeki.hareketHakki = 1;

            yerdeki.PlayStackAnimation();
            
            // --- EKLEME: Aynı tür objeler yığınlandığında da koleksiyonu tetikle ---
            if (CollectionManager.Instance != null)
            {
                CollectionManager.Instance.ObjeAcildi(yerdeki.verisi.collectionID);
            }
            // ---------------------------------------------------------------------

            return true;
        }

        return false;
    }

    void PlayMergeParticle(Vector3 pos)
    {
        if (mergeParticlePrefab == null) return;
        Instantiate(mergeParticlePrefab, pos, Quaternion.identity);
    }

    BirlestirmeVerisi TarifAra(List<ObjeVerisi> malzemeler)
    {
        foreach (var tarif in tumTarifler)
        {
            if (tarif.gerekenMalzemeler.Count != malzemeler.Count) continue;

            List<ObjeVerisi> kopya = new(malzemeler);
            bool uygun = true;

            foreach (var gereken in tarif.gerekenMalzemeler)
            {
                if (kopya.Contains(gereken))
                    kopya.Remove(gereken);
                else
                {
                    uygun = false;
                    break;
                }
            }

            if (uygun) return tarif;
        }
        return null;
    }

    public bool SeviyeSiniriAsiliyorMu(ObjeVerisi sonuc)
    {
        if (sonuc == null) return false;
        if (GameManager.Instance == null) return false;
        if (GameManager.Instance.SuankiLevelData == null) return false;

        var sinir = GameManager.Instance.SuankiLevelData.izinVerilenEnUstObje;
        return sinir != null && sonuc.objeSeviyesi > sinir.objeSeviyesi;
    }

    void BinaOlustur(GridCell hucre, ObjeVerisi bina)
    {
        Vector3 pos = gridManager.grid.GetCellCenterWorld(hucre.cellPosition);
        PlayMergeParticle(pos + Vector3.up * 0.6f);

        GameObject go = Instantiate(bina.objePrefab, pos, bina.objePrefab.transform.rotation);

        PlaceableObject po = go.GetComponent<PlaceableObject>();
        po.transform.localScale = Vector3.zero;

        po.verisi = bina;
        po.currentCell = hucre;
        hucre.currentObject = po;

        po.hareketHakki = 1;
        po.transform.position = pos + Vector3.up * po.heightOffset;
        po.icindekiMalzemeler.Clear();
        po.SetPreviewMode(false);

        // --- KRİTİK EKLEME: Türü ne olursa olsun koleksiyonu aç ---
        if (CollectionManager.Instance != null)
        {
            CollectionManager.Instance.ObjeAcildi(bina.collectionID);
        }
        // ---------------------------------------------------------

        StartCoroutine(MergeVeSpawnSirasi(po));
        GameManager.Instance.UretimYapildi(po.verisi, po.transform.position);
    }

    IEnumerator MergeVeSpawnSirasi(PlaceableObject po)
    {
        yield return new WaitForSeconds(mergeBeklemeSuresi);
        po.PlaySpawnAnimation();
    }
}