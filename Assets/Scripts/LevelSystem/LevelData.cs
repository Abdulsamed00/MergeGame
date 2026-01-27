using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Hedefleri tutmak için kutu
[System.Serializable]
public struct LevelHedef
{
    public ObjeVerisi istenenObje; 
    public int adet;               
}

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
    
    [Header("Görsel Ayarlar")]
    [Tooltip("Bu levelda zemin nasıl görünecek? Boş bırakılırsa varsayılan kullanılır.")]
    public GridCell zeminPrefabi;
    
    [Header("Görevler")]
    public List<LevelHedef> hedefler;
   
    [Header("Kısıtlamalar")]
    // Bu bölümde yapılabilecek en yüksek seviyeli bina
    // Eğer oyuncu bunun üstüne çıkmaya çalışırsa birleşme olmayacak
    public ObjeVerisi izinVerilenEnUstObje; 

    public List<LevelSpawnVerisi> levelObjeleri;
   
    [Header("Yıldız Sistemi")]
    public int yildiz1Puani; 
    public int yildiz2Puani; 
    public int yildiz3Puani;
}