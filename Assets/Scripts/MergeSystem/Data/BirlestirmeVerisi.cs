using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "YeniTarif", menuName = "Oyun/Yeni Tarif")]
public class BirlestirmeVerisi : ScriptableObject
{
    public string tarifAdi;

    public List<ObjeVerisi> gerekenMalzemeler;
    public ObjeVerisi sonucObjesi;
}
