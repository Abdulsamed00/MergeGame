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

        obj.transform.position = transform.position;

        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();

        if (renderers.Length > 0)
        {
            float enAltY = float.MaxValue;
            bool rendererBulundu = false;

            foreach (var rend in renderers)
            {
                if (rend is ParticleSystemRenderer)
                {
                    continue;
                }

                if (rend.bounds.min.y < enAltY)
                {
                    enAltY = rend.bounds.min.y;
                    rendererBulundu = true;
                }
            }

            if (rendererBulundu)
            {
                float zeminY = transform.position.y;

                float fark = zeminY - enAltY;
                obj.transform.position += Vector3.up * fark;
            }
        }
        else
        {
            PlaceableObject po = obj.GetComponent<PlaceableObject>();
            float manualOffset = (po != null) ? po.heightOffset : 0.5f;
            obj.transform.position += Vector3.up * manualOffset;
        }
    }

    public void Clear()
    {
        if (placedObject != null)
        {
            Destroy(placedObject);
        }

        placedObject = null;
        storedData = null;
    }

    public GameObject GetPlacedObject()
    {
        return placedObject;
    }
}