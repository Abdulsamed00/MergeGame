using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "YeniObje", menuName = "Oyun/Yeni Obje")]
public class ObjeVerisi : ScriptableObject
{
    public string objeAdi;
    public GameObject objePrefab;
}
