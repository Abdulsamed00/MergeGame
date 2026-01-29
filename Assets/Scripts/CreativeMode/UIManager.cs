using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    // CollectionManager'daki listenin aynısını buraya da atamalısın
    // VEYA CollectionDatabase scriptable object'ini direkt referans alabilirsin (Daha sağlıklı olur)
    public CollectionDatabase anaVeritabani; 
    
    public ObjectButton buttonPrefab;
    public Transform contentParent;

    [Header("Ayarlar")]
    public bool kilitliObjeleriGoster = true; // True ise kilitli gözükür, False ise hiç gözükmez

    void Start()
    {
        ListeleyiOlustur();
    }

    void ListeleyiOlustur()
    {
        // Önce temizlik (Eğer menü açılıp kapanıyorsa dublike olmasın)
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // CollectionDatabase içindeki listeyi dönüyoruz
        foreach (var obje in anaVeritabani.tumObjelerSorted)
        {
            // CollectionManager'daki Key mantığının AYNISI: "Collection_" + ID
            string key = "Collection_" + obje.collectionID;
            
            // 1 ise açılmış, 0 veya yoksa kilitli
            bool isUnlocked = PlayerPrefs.GetInt(key, 0) == 1;

            // Eğer kilitlileri hiç göstermek istemiyorsan ve obje kilitliyse -> atla
            if (!kilitliObjeleriGoster && !isUnlocked) continue;

            // Butonu oluştur
            var btn = Instantiate(buttonPrefab, contentParent);
            
            // Setup'a kilit durumunu da gönderiyoruz
            btn.Setup(obje, isUnlocked);
        }
    }

    public void MainMenu()
    {
        if (CreativeSaveManager.Instance != null)
        {
            CreativeSaveManager.Instance.SaveCreativeMap();
        }

        SceneManager.LoadScene("UI");
    }

    
    // Geliştirici Testi İçin: Bütün kilitleri açan hile kodu
    public void UnlockAllCheat()
    {
        foreach (var obje in anaVeritabani.tumObjelerSorted)
        {
            PlayerPrefs.SetInt("Collection_" + obje.collectionID, 1);
        }
        PlayerPrefs.Save();
        ListeleyiOlustur(); // Listeyi yenile
    }
}