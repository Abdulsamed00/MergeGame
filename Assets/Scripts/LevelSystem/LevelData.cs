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
    [Header("Genel")]
    public int levelID;
    public string ulkeAdi;
    public string levelAdi;

    [Header("Grid")]
    public int gridGenislik;
    public int gridYukseklik;

    [Header("Audio")]
    public AudioClip levelMusic;   // 🔥 LEVEL'E ÖZEL MÜZİK

    [Header("Görsel Ayarlar")]
    public GridCell zeminPrefabi;

    [Header("Görevler")]
    public List<LevelHedef> hedefler;

    [Header("Kısıtlamalar")]
    public ObjeVerisi izinVerilenEnUstObje;

    [Header("Spawn Edilecek Objeler")]
    public List<LevelSpawnVerisi> levelObjeleri;

    [Header("Yıldız Sistemi")]
    public int yildiz1Puani;
    public int yildiz2Puani;
    public int yildiz3Puani;
}