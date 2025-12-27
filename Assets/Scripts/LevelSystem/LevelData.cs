using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "YeniLevelData", menuName = "Oyun/Level Data")]
public class LevelData : ScriptableObject
{
   public int levelID;
   public string ulkeAdi;

   public int gridGenislik;
   public int gridYukseklik;

   public int hedeflenenBinaSayisi = 1;

   public List<ObjeVerisi> spawnlanablirObjeler;
}
