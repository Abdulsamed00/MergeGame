using UnityEngine;
using UnityEngine.UI;

public class ObjectButton : MonoBehaviour
{
    public Image icon;
    private ObjeVerisi objeVerisi;

    public void Setup(ObjeVerisi veri)
    {
        objeVerisi = veri;
        icon.sprite = veri.uiIkonu;
    }

    public void OnClick()
    {
        CreativePlacementManager.Instance.ObjeSec(objeVerisi);
    }
}