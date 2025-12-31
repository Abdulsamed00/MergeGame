using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelSpawnVerisi
{
    public ObjeVerisi obje;
    [Range(0, 100)] public float spawnYuzdesi;
}

[CreateAssetMenu(fileName = "YeniLevelData", menuName = "Oyun/Level Data")]
public class LevelData : ScriptableObject
{
   public int levelID;
   public string ulkeAdi;

   public int gridGenislik;
   public int gridYukseklik;

   public int hedeflenenBinaSayisi = 1;

   public List<LevelSpawnVerisi> levelObjeleri;
}
