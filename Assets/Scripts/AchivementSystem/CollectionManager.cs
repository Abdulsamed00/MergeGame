using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollectionManager : MonoBehaviour
{
    public static CollectionManager Instance;

    [Header("Veri Tabanı")]
    public CollectionDatabase anaListe; 

    [Header("UI Referansları")]
    public GameObject achievementPanel; // <--- YENİ: Panelin kendisi (AchievementPanel)
    public RawImage renderScreen; 
    public Image lockIcon;       
    public Text nameText;        
    public Text progressText;    
    public Button prevButton;     
    public Button nextButton;     

    [Header("3D Stüdyo Ayarları")]
    public Camera collectionCamera;
    public Camera MainCamera;
    public Transform studioSpawnPoint; 
    private GameObject current3DModel; 

    private int currentIndex = 0; 

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Oyun başlar başlamaz kamerayı ve paneli kapatıyoruz ki yük olmasın
        if (collectionCamera != null) collectionCamera.gameObject.SetActive(false);
        if (achievementPanel != null) achievementPanel.SetActive(false);

        if (prevButton != null && nextButton != null)
        {
            prevButton.onClick.AddListener(OncekiObje);
            nextButton.onClick.AddListener(SonrakiObje);
        }
    }

    
    public void KoleksiyonuAc()
    {
        MainCamera.gameObject.SetActive(false);
        if (achievementPanel != null) achievementPanel.SetActive(true);
        if (collectionCamera != null) collectionCamera.gameObject.SetActive(true); 
        GuncelleUI(); // Görüntüyü yenile
    }

   
    public void KoleksiyonuKapat()
    {
        if (achievementPanel != null) achievementPanel.SetActive(false); 
        if (collectionCamera != null) collectionCamera.gameObject.SetActive(false);
        MainCamera.gameObject.SetActive(true);
    }

    // --- KAYIT SİSTEMİ ---
    public void ObjeAcildi(int id)
    {
        string key = "Collection_" + id;
        if (PlayerPrefs.GetInt(key, 0) == 0)
        {
            PlayerPrefs.SetInt(key, 1);
            PlayerPrefs.Save();
        }
    }

    // --- BUTON FONKSİYONLARI ---
    public void SonrakiObje()
    {
        if (currentIndex < anaListe.tumObjelerSorted.Count - 1)
        {
            currentIndex++;
            GuncelleUI();
        }
    }

    public void OncekiObje()
    {
        if (currentIndex > 0)
        {
            currentIndex--;
            GuncelleUI();
        }
    }

    // --- GÖRÜNTÜYÜ GÜNCELLEME ---
    void GuncelleUI()
    {
        ObjeVerisi veri = anaListe.tumObjelerSorted[currentIndex];
        bool isUnlocked = PlayerPrefs.GetInt("Collection_" + veri.collectionID, 0) == 1;

        if (current3DModel != null) Destroy(current3DModel);

        if (isUnlocked)
        {
            lockIcon.gameObject.SetActive(false); 
            renderScreen.color = Color.white;     
            nameText.text = veri.objeAdi;         

            if (veri.objePrefab != null)
            {
                current3DModel = Instantiate(veri.objePrefab, studioSpawnPoint.position, Quaternion.identity, studioSpawnPoint);
                SetLayerRecursively(current3DModel, LayerMask.NameToLayer("3DUI"));
                current3DModel.transform.localRotation = Quaternion.Euler(0, 45, 0); 
                
                // Objelerin animasyon oynuyordu, durdurdum
                Animator[] tumAnimatorler = current3DModel.GetComponentsInChildren<Animator>();
                foreach (var anim in tumAnimatorler)
                {
                    anim.enabled = false; // Motoru durdur
                }
                
                //Eski tip animation bileşini kullanıldıysa;
                Animation[] eskiAnimasyonlar = current3DModel.GetComponentsInChildren<Animation>();
                foreach (var anim in eskiAnimasyonlar)
                {
                    anim.enabled = false;
                }
            }
        }
        else
        {
            lockIcon.gameObject.SetActive(true);  
            renderScreen.color = Color.clear;     
            nameText.text = "???";               
        }

        int acilanSayisi = ToplamAcilanSayisi();
        progressText.text = $"{acilanSayisi}/{anaListe.tumObjelerSorted.Count}";

        prevButton.interactable = (currentIndex > 0);
        nextButton.interactable = (currentIndex < anaListe.tumObjelerSorted.Count - 1);
    }

    int ToplamAcilanSayisi()
    {
        int sayi = 0;
        foreach(var item in anaListe.tumObjelerSorted)
        {
            if (PlayerPrefs.GetInt("Collection_" + item.collectionID, 0) == 1) sayi++;
        }
        return sayi;
    }

    void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (null == obj) return;
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            if (null == child) continue;
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}