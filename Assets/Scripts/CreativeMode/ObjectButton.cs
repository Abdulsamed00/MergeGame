using UnityEngine;
using UnityEngine.UI;

public class ObjectButton : MonoBehaviour
{
    [Header("UI Bileşenleri")]
    public Image icon;
    public Button buttonComponent; // Tıklamayı kapatmak için
    public GameObject lockOverlay; // Kilit simgesi (Panel içinde bir Image)

    private ObjeVerisi objeVerisi;
    private bool isUnlocked = false;

    // Setup fonksiyonunu güncelledik, artık kilit durumunu da alıyor
    public void Setup(ObjeVerisi veri, bool acikMi)
    {
        objeVerisi = veri;
        isUnlocked = acikMi;

        icon.sprite = veri.uiIkonu;

        if (isUnlocked)
        {
            // AÇIKSA
            if (lockOverlay != null) lockOverlay.SetActive(false); // Kilit resmini gizle
            buttonComponent.interactable = true; // Tıklanabilir yap
            icon.color = Color.white; // Rengi normal yap
        }
        else
        {
            // KAPALIYSA
            if (lockOverlay != null) lockOverlay.SetActive(true); // Kilit resmini aç
            buttonComponent.interactable = false; // Tıklamayı kapat
            icon.color = new Color(0.3f, 0.3f, 0.3f, 1f); // Hafif karart
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