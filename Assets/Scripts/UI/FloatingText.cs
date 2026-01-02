using UnityEngine;
using UnityEngine.UI; // Text kullanmak için şart

public class FloatingText : MonoBehaviour
{
    public float destroyTime = 1.5f; // Kaç saniye sonra yok olsun
    public Text textComponent;       // Yazının kendisi

    void Start()
    {
        // Doğar doğmaz yok olma sayacını başlat
        Destroy(gameObject, destroyTime);
    }

    void Update()
    {
        // Sürekli yukarı doğru uç
        transform.Translate(Vector3.up * Time.deltaTime);
    }

    // GameManager bu fonksiyonu çağırıp yazıyı ve rengi değiştirecek
    public void SetText(string metin, Color renk)
    {
        textComponent.text = metin;
        textComponent.color = renk;
    }
}