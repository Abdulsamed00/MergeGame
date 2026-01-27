using UnityEngine;
using UnityEngine.UI;

public class FloatingText : MonoBehaviour
{
    public float destroyTime = 1.5f;
    public Text textComponent;
    
    private Camera mainCamera;

    void Start() 
    { 
        mainCamera = Camera.main;
        Destroy(gameObject, destroyTime); 
    }

    // Kamera hareketinden sonra çalışsın diye LateUpdate kullanıyoruz
    void LateUpdate() 
    { 
        if (mainCamera != null)
        {
            // Yazının rotasyonunu kameranın rotasyonuyla eşitleniyor
            transform.rotation = mainCamera.transform.rotation;
        }

        // Yukarı uçacak
        // Space.World ekledim
        // Çünkü yazı döndüğü için kendi yukarısı değişti. Biz dünyanın yukarısına gitmesini istiyoruz
        transform.Translate(Vector3.up * Time.deltaTime, Space.World); 
    }

    public void SetText(string t, Color c) 
    { 
        textComponent.text = t; 
        textComponent.color = c; 
    }
}