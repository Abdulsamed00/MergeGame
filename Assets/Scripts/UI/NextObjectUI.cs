using UnityEngine;
using UnityEngine.UI;

public class NextObjectUI : MonoBehaviour
{
    [Header("UI Elemanlari")]
    public Image nextObjectImage;

    [Header("Bağlanti")]
    public PlacementManager placementManager;

    //Dinleyicileri ekleme/kaldırma.
    //Sadece UI aktifken dinliyor. Update fonksiyonu gereksiz yere çalışmıyor.
    void OnEnable()
    {
        if (placementManager != null)
        {
            placementManager.OnNextObjectChanged.AddListener(UpdateImage);
        }
    }

    void OnDisable()
    {
        if (placementManager != null)
        {
            placementManager.OnNextObjectChanged.RemoveListener(UpdateImage);
        }
    }

    //UI görüntüsünü güncelleme fonksiyonu
    public void UpdateImage(Sprite newSprite)
    {
        if (newSprite != null)
        {
            nextObjectImage.sprite = newSprite;
            nextObjectImage.enabled = true;
            nextObjectImage.preserveAspect = true;
        }
        else
        {
            nextObjectImage.enabled = false;
        }
    }
}