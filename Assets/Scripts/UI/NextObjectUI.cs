using UnityEngine;
using UnityEngine.UI;

public class NextObjectUI : MonoBehaviour
{
    [Header("UI Elemanlari")]
    public Image nextObjectImage; // Buraya UI'daki Image'ı sürükle

    [Header("Bağlanti")]
    public PlacementManager placementManager;

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