using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Paneller")]
    public GameObject ulkeSecimPaneli;  // Türkiye, Japonya vb. olduğu yer
    public GameObject levelSecimPaneli; // 1-10 butonlarının olduğu yer

    [Header("Level Butonları")]
    // 1'den 10'a kadar olan butonlardaki scriptleri buraya sürükleyeceğiz
    public List<LevelSelectButton> levelButonlari;

    public GameObject BilgiPaneli, TR, BR, JP, MSR, TarifP;

    private void Start()
    {
  
    }

    private void Update()
    {
       
    }

    // Ülke butonlarına atanacak fonksiyon
    // TR için 0, JP için 1, BR için 2...
    public void UlkeSecildi(int ulkeIndex)
    {
        ulkeSecimPaneli.SetActive(false);
        levelSecimPaneli.SetActive(true);

        // Tüm butonlara dönüp "Kendinizi bu ülkeye göre ayarlayın" diyoruz
        foreach (var buton in levelButonlari)
        {
            buton.ButonuGuncelle(ulkeIndex);
        }
    }

    // "Geri" butonuna atanacak fonksiyon
    public void GeriDon()
    {
        levelSecimPaneli.SetActive(false);
        ulkeSecimPaneli.SetActive(true);
    }
    
    public void VerileriSifirla()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("VERİLER SİLİNDİ, SAHNE YENİLENİYOR...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void BilgiPaneliAc()
    {
        BilgiPaneli.SetActive(true);
        
    }
}