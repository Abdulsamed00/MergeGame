using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public GridCell zeminPrefabi;
    
    [Header("Başlangıç Durumu")] 
    // --- DEĞİŞİKLİK BURADA: Liste yerine sayı yaptık ---
    [Tooltip("Oyun başlarken şans oranlarına göre kaç tane obje spawn olsun?")]
    public int baslangicObjeSayisi = 3; 
    // --------------------------------------------------
    
    [Header("Görevler")]
    public List<LevelHedef> hedefler;
   
    [Header("Kısıtlamalar")]
    public ObjeVerisi izinVerilenEnUstObje; 

    public List<LevelSpawnVerisi> levelObjeleri;
   
    [Header("Yıldız Sistemi")]
    public int yildiz1Puani; 
    public int yildiz2Puani; 
    public int yildiz3Puani;
}