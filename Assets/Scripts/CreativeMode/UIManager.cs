using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public CollectionDatabase anaVeritabani; 
    
    public ObjectButton buttonPrefab;
    public Transform contentParent;

    [Header("Ayarlar")]
    public bool kilitliObjeleriGoster = true; 

    void Start()
    {
        ListeleyiOlustur();
    }

    void ListeleyiOlustur()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        foreach (var obje in anaVeritabani.tumObjelerSorted)
        {
            string key = "Collection_" + obje.collectionID;
            bool isUnlocked = PlayerPrefs.GetInt(key, 0) == 1;

            if (!kilitliObjeleriGoster && !isUnlocked)
            {
                continue;
            }

            var btn = Instantiate(buttonPrefab, contentParent);
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

    
    public void UnlockAllCheat()
    {
        foreach (var obje in anaVeritabani.tumObjelerSorted)
        {
            PlayerPrefs.SetInt("Collection_" + obje.collectionID, 1);
        }
        PlayerPrefs.Save();
        ListeleyiOlustur();
    }
}