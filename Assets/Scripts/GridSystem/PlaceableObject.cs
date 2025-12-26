using UnityEngine;
using System.Collections.Generic;

public class PlaceableObject : MonoBehaviour
{
    public ObjeVerisi verisi;
    public GridCell currentCell;
    public float heightOffset = 0.5f;
    
    [Header("Durum")]
    public bool kilitliMi = false; 
    public bool yeniSpawnOldu = false; // Kodda false ama Awake'de zorlayacağız

    // 0 = Kıpırdayamaz (Tekliler ve Binalar)
    // 1 = 1 kere hareket edebilir (2'li Yığınlar)
    public int hareketHakki = 0; 

    public List<ObjeVerisi> icindekiMalzemeler = new List<ObjeVerisi>();

    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
        
        // --- İŞTE ÇÖZÜM BURASI KOMUTANIM ---
        // Prefab ayarı ne olursa olsun, oyun açıldığında herkes "Eski" ve "Hareketsiz" başlar.
        // Sadece PlacementManager tarafından yaratılanlar sonradan True yapılacak.
        yeniSpawnOldu = false;
        hareketHakki = 0;
    }

    void Start()
    {
        if (verisi != null && icindekiMalzemeler.Count == 0)
        {
            icindekiMalzemeler.Add(verisi);
        }
        
        // Başlangıçta animasyon durumunu kontrol et (InitialSpawnManager burayı tetikler)
        // Awake'de false yaptığımız için başlangıç objeleri DURACAK.
        SetPreviewMode(false);
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
            // Animasyon SADECE şu durumlarda çalışır:
            // 1. Mouse ile tutuyorsan (isPreview)
            // 2. Yeni doğduysa (PlacementManager true yaptıysa)
            // 3. Hareket hakkı varsa (Birleşmiş objeyse)
            bool oynamaliMi = isPreview || yeniSpawnOldu || hareketHakki > 0;
            animator.enabled = oynamaliMi;
        }
    }
   
    
}