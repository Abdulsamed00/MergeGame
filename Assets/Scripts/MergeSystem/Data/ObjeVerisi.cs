using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Project ekranında tıkladığımızda ScriptableObject oluşturabilmek için menü eklentisi
[CreateAssetMenu(fileName = "YeniObje", menuName = "Oyun/Yeni Obje")]
public class ObjeVerisi : ScriptableObject
{
    public string objeAdi;
    public GameObject objePrefab;

    public Sprite uiIkonu; //Sonraki obje kutusunda çıkacak ikon.
    [Range(0, 100)] public float spawnYuzdesi; //Bu objenin spawnlanma yüzdesi.
}
