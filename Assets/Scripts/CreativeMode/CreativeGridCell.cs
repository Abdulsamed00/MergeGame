using UnityEngine;

public class CreativeGridCell : MonoBehaviour
{
    public Vector3Int CellPosition { get; private set; }
    private GameObject placedObject;
    
    // --- YENİ EKLENEN: Obje verisini burada saklıyoruz ---
    public ObjeVerisi storedData { get; private set; } 

    public void Init(Vector3Int pos)
    {
        CellPosition = pos;
    }

    public bool IsEmpty()
    {
        return placedObject == null;
    }

    // --- GÜNCELLENEN: Artık veriyi de parametre alıyor ---
    public void PlaceObject(GameObject obj, ObjeVerisi veri)
    {
        placedObject = obj;
        storedData = veri; // Veriyi hafızaya at
        obj.transform.position = transform.position;
    }

    public void Clear()
    {
        if (placedObject != null)
            Destroy(placedObject);

        placedObject = null;
        storedData = null; // Veriyi temizle
    }

    public GameObject GetPlacedObject()
    {
        return placedObject;
    }
}