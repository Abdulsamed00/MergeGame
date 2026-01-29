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
        if(normalScale == Vector3.zero) normalScale = new Vector3(0.6f, 0.6f, 0.6f);
        if(buyukScale == Vector3.zero) buyukScale = new Vector3(0.7f, 0.7f, 0.7f);
    }

    void Start()
    {
        if (verisi != null && icindekiMalzemeler.Count == 0)
        {
            icindekiMalzemeler.Add(verisi);
        }

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
    
    public void SetPreviewMode(bool isPreview)
    {
        if (animator != null) animator.SetBool("IsPreview", isPreview);
    }

    public void SetStackedMode(bool isStacked)
    {
        if (animator != null) animator.SetBool("IsStacked", isStacked);
    }

    public void PlayMergeAnimation()
    {
        if (animator != null)
        {
            SetStackedMode(false);
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
            SetStackedMode(true);
            animator.SetTrigger("Stack");
        }
    }
}