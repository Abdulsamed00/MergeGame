using UnityEngine;
using System.Collections.Generic;

public class PlaceableObject : MonoBehaviour
{
    public ObjeVerisi verisi;
    public GridCell currentCell;
    public float heightOffset = 0.5f;
    
    [Header("Durum")]
    public bool kilitliMi = false;
    public int hareketHakki = 1;

    [Header("Scale Ayarları")]
    public Vector3 normalScale = new Vector3(0.6f, 0.6f, 0.6f);
    public Vector3 buyukScale = new Vector3(0.7f, 0.7f, 0.7f);

    public List<ObjeVerisi> icindekiMalzemeler = new List<ObjeVerisi>();
    private Animator animator;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        // Eğer inspector'da unutulursa default değerleri ata
        if(normalScale == Vector3.zero) normalScale = new Vector3(0.6f, 0.6f, 0.6f);
        if(buyukScale == Vector3.zero) buyukScale = new Vector3(0.7f, 0.7f, 0.7f);
    }

    void Start()
    {
        if (verisi != null && icindekiMalzemeler.Count == 0)
        {
            icindekiMalzemeler.Add(verisi);
        }
        
        // Başlangıçta stackli mi değil mi kontrol et
        // Eğer birden fazla malzeme varsa Stack modundadır
        if (icindekiMalzemeler.Count > 1) 
        {
            transform.localScale = buyukScale;
            SetStackedMode(true); // Animator'a söyle
        }
        else 
        {
            transform.localScale = normalScale;
            SetStackedMode(false);
        }
    }

    public void BoyutuGuncelle()
    {
        if (icindekiMalzemeler.Count > 1)
        {
            transform.localScale = buyukScale; 
            SetStackedMode(true);
        }
        else
        {
            transform.localScale = normalScale; 
            SetStackedMode(false);
        }
    }

    // --- ANİMASYON FONKSİYONLARI ---

    public void SetPreviewMode(bool isPreview)
    {
        if (animator != null) animator.SetBool("IsPreview", isPreview);
    }

    // YENİ: Animator'a objenin stack halinde olduğunu bildirir
    public void SetStackedMode(bool isStacked)
    {
        if (animator != null) animator.SetBool("IsStacked", isStacked);
    }

    public void PlayMergeAnimation()
    {
        if (animator != null)
        {
            SetStackedMode(false); // Merge olunca stack bozulur, normale döner
            animator.SetTrigger("Merge");
        }
    }

    public void PlaySpawnAnimation()
    {
        if (animator != null) animator.SetTrigger("Spawn");
    }

    public void PlayStackAnimation()
    {
        if (animator != null)
        {
            SetStackedMode(true); // Stack animasyonu çalarken durumu da kilitle
            animator.SetTrigger("Stack");
        }
    }
}