using UnityEngine;

[CreateAssetMenu(fileName = "YeniUlkeVerisi", menuName = "Oyun/Country Data")]
public class CountryData : ScriptableObject
{
    public string ulkeAdi;
    public AudioClip ulkeMuzigi;
}