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

    // =======================
    // MERGE OLABİLİR Mİ?
    // =======================
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

    // =======================
    // GERÇEK MERGE
    // =======================
    public bool ManuelBirlestirme(PlaceableObject elimizdeki, PlaceableObject yerdeki)
    {
        GridCell hedefHucre = yerdeki.currentCell;
        Vector3 particlePos = gridManager.grid.GetCellCenterWorld(hedefHucre.cellPosition)
                              + Vector3.up * 0.6f;

        List<ObjeVerisi> toplam = new();
        toplam.AddRange(elimizdeki.icindekiMalzemeler);
        toplam.AddRange(yerdeki.icindekiMalzemeler);

        BirlestirmeVerisi tarif = TarifAra(toplam);

        // =======================
        // A) TARİFLİ BİRLEŞME
        // =======================
        if (tarif != null)
        {
            if (SeviyeSiniriAsiliyorMu(tarif.sonucObjesi)) return false;
            
            if (yerdeki != null)
                Destroy(yerdeki.gameObject);

            BinaOlustur(hedefHucre, tarif.sonucObjesi);
            return true;
        }

        // =======================
        // B) STACK (AYNI OBJE)
        // =======================
        if (elimizdeki.verisi == yerdeki.verisi)
        {
            yerdeki.icindekiMalzemeler.AddRange(elimizdeki.icindekiMalzemeler);
            yerdeki.BoyutuGuncelle();
            yerdeki.hareketHakki = 1;

            yerdeki.PlayStackAnimation();

            return true;
        }

        return false;
    }

    // =======================
    // PARTICLE
    // =======================
    void PlayMergeParticle(Vector3 pos)
    {
        if (mergeParticlePrefab == null) return;
        Instantiate(mergeParticlePrefab, pos, Quaternion.identity);
    }

    // =======================
    // TARİF BUL
    // =======================
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

    // =======================
    // SEVİYE SINIRI
    // =======================
    public bool SeviyeSiniriAsiliyorMu(ObjeVerisi sonuc)
    {
        // 1. KORUMA: Gelen sonuç verisi yoksa işlemi durdur (false dön)
        if (sonuc == null) return false;

        // 2. KORUMA: GameManager yoksa (Tutorial sahnesi veya yanlış başlangıç)
        if (GameManager.Instance == null)
        {
            // Eğer GameManager yoksa seviye sınırı da yoktur, izin ver.
            return false;
        }

        // 3. KORUMA: Level Data yüklenmemişse
        if (GameManager.Instance.SuankiLevelData == null)
        {
            // Level verisi yoksa sınır yoktur.
            return false;
        }

        // --- ASIL KOD ---
        var sinir = GameManager.Instance.SuankiLevelData.izinVerilenEnUstObje;

        // Sınır yoksa (null ise) her şeye izin ver, varsa karşılaştır.
        return sinir != null && sonuc.objeSeviyesi > sinir.objeSeviyesi;
    }

    // =======================
    // BİNA OLUŞTUR
    // =======================
    void BinaOlustur(GridCell hucre, ObjeVerisi bina)
    {
        Vector3 pos = gridManager.grid.GetCellCenterWorld(hucre.cellPosition);

        // 🔥 SADECE YENİ OBJE OLUŞURKEN PARTICLE
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

        StartCoroutine(MergeVeSpawnSirasi(po));
        
        
        GameManager.Instance.UretimYapildi(po.verisi, po.transform.position);
    }


    IEnumerator MergeVeSpawnSirasi(PlaceableObject po)
    {
        yield return new WaitForSeconds(mergeBeklemeSuresi);
        po.PlaySpawnAnimation();
    }
}