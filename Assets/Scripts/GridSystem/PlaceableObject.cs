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

    private Vector3 savedScale; 

    public List<ObjeVerisi> icindekiMalzemeler = new List<ObjeVerisi>();
    private Animator animator;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        normalScale = new Vector3(0.6f, 0.6f, 0.6f);
        buyukScale = new Vector3(0.7f, 0.7f, 0.7f);
    }

    void Start()
    {
        if (verisi != null && icindekiMalzemeler.Count == 0)
        {
            icindekiMalzemeler.Add(verisi);
        }
        BoyutuGuncelle();
    }

    public void BoyutuGuncelle()
    {
        if (animator != null && animator.enabled) return;

        if (icindekiMalzemeler.Count > 1)
            transform.localScale = buyukScale; 
        else
            transform.localScale = normalScale; 
    }

    public void SetPreviewMode(bool isPreview)
    {
        if (animator != null)
        {
            if (!animator.enabled) animator.enabled = true;
            animator.SetBool("IsPreview", isPreview);
        }
    }

    public void SkalayiKaydet()
    {
        savedScale = transform.localScale;
    }

    public void EskiSkalayaDon()
    {
        transform.localScale = savedScale;
    }

    // --- YENİ EKLENEN ANİMASYON TETİKLEYİCİLERİ ---
    
    public void PlayMergeAnimation()
    {
        if (animator != null)
        {
            animator.enabled = true;
            animator.SetTrigger("Merge"); // Animator'daki Merge triggerını çalıştır
        }
    }

    public void PlaySpawnAnimation()
    {
        if (animator != null)
        {
            animator.enabled = true;
            animator.SetTrigger("Spawn"); // Animator'daki Spawn triggerını çalıştır
        }
    }
}