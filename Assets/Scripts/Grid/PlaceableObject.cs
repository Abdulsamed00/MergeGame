using UnityEngine;
using System.Collections.Generic;

public class PlaceableObject : MonoBehaviour
{
    public ObjeVerisi verisi;
    public GridCell currentCell;
    public float heightOffset = 0.5f;
    
    [Header("Durum")]
    public bool kilitliMi = false; 
    public int hareketHakki = 0; // 0=Durur, 1=Gider

    [Header("Scale Ayarları")]
    // Normal boyutu 0.6, Birleşince 1.0 olsun istedin
    public Vector3 normalScale = new Vector3(0.6f, 0.6f, 0.6f);
    public Vector3 buyukScale = new Vector3(0.85f, 0.85f, 0.85f);

    public List<ObjeVerisi> icindekiMalzemeler = new List<ObjeVerisi>();

    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        if (verisi != null && icindekiMalzemeler.Count == 0)
        {
            icindekiMalzemeler.Add(verisi);
        }
        
        // Başlangıçta boyutunu ayarla
        BoyutuGuncelle();
    }

    // --- YENİ FONKSİYON: BOYUT AYARLAMA ---
    public void BoyutuGuncelle()
    {
        // Eğer içinde 1'den fazla malzeme varsa (Yığınsa) BÜYÜT
        if (icindekiMalzemeler.Count > 1 && !kilitliMi)
        {
            transform.localScale = buyukScale;
        }
        else
        {
            // Tekliyse veya Binaysa (Bina prefabının kendi boyutu vardır ama yine de kontrol edelim)
            // Binayı scale ile bozmayalım, sadece tuğlalar için geçerli olsun.
            if (!kilitliMi) transform.localScale = normalScale;
        }
    }

    public void SetPreviewMode(bool isPreview)
    {
        if (kilitliMi)
        {
            if (animator) animator.enabled = false;
            return;
        }

        if (animator != null)
        {
            // ARTIK SADECE MOUSE İLE TUTARKEN OYNASIN
            // Yığın olunca oynamasın, sadece büyüsün istedin.
            animator.enabled = isPreview;
        }
    }
}