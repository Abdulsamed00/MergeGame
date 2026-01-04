using UnityEngine;
using UnityEngine.UI; // Image ve Button için şart
using UnityEngine.SceneManagement;

public class LevelSelectButton : MonoBehaviour
{
    [Header("Buton Ayarları")]
    public int butonNumarasi; 
    private Button myButton;
    private int gercekLevelIndex;

    [Header("Görseller")]
    public GameObject lockIcon; // Kilit Resmi (GameObject kalabilir)
    
    // DİKKAT: Bunları GameObject değil 'Image' yaptık ki rengini değiştirelim
    public Image star1; 
    public Image star2;
    public Image star3;

    public void ButonuGuncelle(int ulkeCarpani) 
    {
        myButton = GetComponent<Button>();
        gercekLevelIndex = (ulkeCarpani * 10) + (butonNumarasi - 1);

        int enYuksekLevel = PlayerPrefs.GetInt("HighestUnlockedLevel", 0);
        bool isUnlocked = gercekLevelIndex <= enYuksekLevel;

        if (isUnlocked)
        {
            // --- LEVEL AÇIK ---
            if(lockIcon) lockIcon.SetActive(false); // Kilidi kaldır
            myButton.interactable = true; 

            // Yıldızları Boya
            YildizlariBoya();

            myButton.onClick.RemoveAllListeners();
            myButton.onClick.AddListener(() => LeveliBaslat());
        }
        else
        {
            // --- LEVEL KİLİTLİ ---
            if(lockIcon) lockIcon.SetActive(true); // Kilidi göster
            myButton.interactable = false; 
            
            // Kilitliyse yıldızlar sönük (Siyah/Gri) kalsın
            Color sonukRenk = new Color(0.2f, 0.2f, 0.2f, 1f); // Koyu Gri
            if(star1) star1.color = sonukRenk;
            if(star2) star2.color = sonukRenk;
            if(star3) star3.color = sonukRenk;
        }
    }

    void YildizlariBoya()
    {
        string saveKey = "Level_" + gercekLevelIndex + "_Stars";
        int kazanilanYildiz = PlayerPrefs.GetInt(saveKey, 0);

        // Renkleri Tanımla
        Color sariRenk = Color.yellow; // Veya new Color(1f, 0.8f, 0f); (Altın Sarısı)
        Color sonukRenk = new Color(0.2f, 0.2f, 0.2f, 1f); // Koyu Gri/Siyah

        // Mantık: Eğer kazanılan yıldız sayısı yetiyorsa Sarı yap, yetmiyorsa Sönük yap.
        if (star1 != null) star1.color = (kazanilanYildiz >= 1) ? sariRenk : sonukRenk;
        if (star2 != null) star2.color = (kazanilanYildiz >= 2) ? sariRenk : sonukRenk;
        if (star3 != null) star3.color = (kazanilanYildiz >= 3) ? sariRenk : sonukRenk;
    }

    void LeveliBaslat()
    {
        DataTransfer.secilenLevelIndex = gercekLevelIndex;
        SceneManager.LoadScene("SampleScene");
    }
}