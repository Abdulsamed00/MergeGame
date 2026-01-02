using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ObjeTuru
{
    Materyal, // Demir, Odun, Tuğla, İnşaat Alanı vb. (Sayacı artırmaz)
    Bina      // Ev, Kale, Kulübe vb. (Sayacı artırır, oyunu kazandırır)
}

[CreateAssetMenu(fileName = "YeniObje", menuName = "Oyun/Yeni Obje")]
public class ObjeVerisi : ScriptableObject
{
    public string objeAdi;
    public GameObject objePrefab;
    

    public Sprite uiIkonu; //Sonraki obje kutusunda çıkacak ikon.
    
    [Header("Obje Ayarları")]
    public ObjeTuru tur;
    
    [Header("Popülasyon Ayarları")]
    public int minPopulasyon = 0; 
    public int maxPopulasyon = 0;
}
