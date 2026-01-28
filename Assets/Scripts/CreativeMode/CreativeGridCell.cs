using UnityEngine;

public class CreativeGridCell : MonoBehaviour
{
    public Vector3Int CellPosition { get; private set; }
    private GameObject placedObject;
    public ObjeVerisi storedData { get; private set; } 

    public void Init(Vector3Int pos)
    {
        CellPosition = pos;
    }

    public bool IsEmpty()
    {
        return placedObject == null;
    }

    public void PlaceObject(GameObject obj, ObjeVerisi veri)
    {
        placedObject = obj;
        storedData = veri;

        // 1. Önce objeyi hücrenin merkezine koy
        obj.transform.position = transform.position;

        // 2. Objenin üzerindeki TÜM Görselleri (Renderer) al
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();

        if (renderers.Length > 0)
        {
            // En alt noktayı bulmak için varsayılan büyük bir sayı veriyoruz
            float enAltY = float.MaxValue;
            bool rendererBulundu = false;

            foreach (var rend in renderers)
            {
                // Particle System gibi şeyleri hesaba katma, sadece Mesh'leri al
                if (rend is ParticleSystemRenderer) continue;

                // Objenin en alt Y noktasını kontrol et
                if (rend.bounds.min.y < enAltY)
                {
                    enAltY = rend.bounds.min.y;
                    rendererBulundu = true;
                }
            }

            if (rendererBulundu)
            {
                // Grid'in zemin seviyesi
                float zeminY = transform.position.y;

                // Aradaki farkı hesapla (Ne kadar aşağıda kalmış?)
                float fark = zeminY - enAltY;

                // Objeyi o fark kadar yukarı taşı
                obj.transform.position += Vector3.up * fark;
            }
        }
        else
        {
            // Eğer Renderer bile bulunamazsa (Görünmez objeyse), manuel offset kullan
            PlaceableObject po = obj.GetComponent<PlaceableObject>();
            float manualOffset = (po != null) ? po.heightOffset : 0.5f;
            obj.transform.position += Vector3.up * manualOffset;
        }
    }

    public void Clear()
    {
        if (placedObject != null)
            Destroy(placedObject);

        placedObject = null;
        storedData = null;
    }

    public GameObject GetPlacedObject()
    {
        return placedObject;
    }
}