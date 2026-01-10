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
        
        // Animasyon varsa Start'ta boyutu aniden değiştirmek yerine
        // Animator'ın scale değerini kullanmasına izin verin.
        // Ama veri tutarlılığı için dahili değişkenleri güncelleyebiliriz.
        if (icindekiMalzemeler.Count > 1) transform.localScale = buyukScale;
        else transform.localScale = normalScale;
    }

    public void BoyutuGuncelle()
    {
        if (icindekiMalzemeler.Count > 1)
        {
            transform.localScale = buyukScale; 
        }
        else
        {
            transform.localScale = normalScale; 
        }
    }

    // --- ANİMASYON FONKSİYONLARI ---

    public void SetPreviewMode(bool isPreview)
    {
        if (animator != null) animator.SetBool("IsPreview", isPreview);
    }

    public void PlayMergeAnimation()
    {
        if (animator != null) animator.SetTrigger("Merge");
    }

    public void PlaySpawnAnimation()
    {
        if (animator != null) animator.SetTrigger("Spawn");
    }

    // --- YENİ EKLENEN: Stack Animasyonu ---
    public void PlayStackAnimation()
    {
        if (animator != null) animator.SetTrigger("Stack");
    }
}