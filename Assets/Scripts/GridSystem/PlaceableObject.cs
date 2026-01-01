using UnityEngine;
using System.Collections.Generic;

public class PlaceableObject : MonoBehaviour
{
    public ObjeVerisi verisi;
    public GridCell currentCell;
    public float heightOffset = 0.5f;
    
    [Header("Durum")]
    public bool kilitliMi = false;  // Artık pek kullanmayacağız ama dursun
    public int hareketHakki = 1;    // Varsayılan 1 olsun

    [Header("Scale Ayarları")]
    public Vector3 normalScale = new Vector3(0.6f, 0.6f, 0.6f);
    public Vector3 buyukScale = new Vector3(0.7f, 0.7f, 0.7f);

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
        // Eğer içinde 1'den fazla malzeme varsa (Yığınsa) BÜYÜT
        if (icindekiMalzemeler.Count > 1)
        {
            transform.localScale = buyukScale; // Örneğin (0.7, 0.7, 0.7)
        }
        else
        {
            transform.localScale = normalScale; // Örneğin (0.6, 0.6, 0.6)
        }
    }

    public void SetPreviewMode(bool isPreview)
    {
        if (animator != null)
        {
            animator.enabled = isPreview;
        }
    }
}