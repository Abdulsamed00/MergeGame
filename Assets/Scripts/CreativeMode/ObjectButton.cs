using UnityEngine;
using UnityEngine.UI;

public class ObjectButton : MonoBehaviour
{
    [Header("UI Bileşenleri")]
    public Image icon;
    public Button buttonComponent;
    public GameObject lockOverlay;

    private ObjeVerisi objeVerisi;
    private bool isUnlocked = false;

    public void Setup(ObjeVerisi veri, bool acikMi)
    {
        objeVerisi = veri;
        isUnlocked = acikMi;

        icon.sprite = veri.uiIkonu;

        if (isUnlocked)
        {

            if (lockOverlay != null)
            {
                lockOverlay.SetActive(false);
            }
            buttonComponent.interactable = true;
            icon.color = Color.white;
        }
        else
        {
            if (lockOverlay != null)
            {
                lockOverlay.SetActive(true);
            }
            buttonComponent.interactable = false;
            icon.color = new Color(0.3f, 0.3f, 0.3f, 1f);
        }
    }

    public void OnClick()
    {
        if (isUnlocked)
        {
            CreativePlacementManager.Instance.ObjeSec(objeVerisi);
        }
        else
        {
            Debug.Log("Bu obje henüz açılmamış!");
        }
    }
}