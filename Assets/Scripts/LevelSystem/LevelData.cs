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
   public string levelAdi;

   public int gridGenislik;
   public int gridYukseklik;

   public int hedeflenenBinaSayisi;

   public List<LevelSpawnVerisi> levelObjeleri;
   
   [Header("Yıldız Sistemi")]
   public int yildiz1Puani; 
   public int yildiz2Puani; 
   public int yildiz3Puani;
}
