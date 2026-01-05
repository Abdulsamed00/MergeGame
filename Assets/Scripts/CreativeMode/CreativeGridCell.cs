using UnityEngine;

public class CreativeGridCell : MonoBehaviour
{
    public Vector3Int CellPosition { get; private set; }
    private GameObject placedObject;

    public void Init(Vector3Int pos)
    {
        CellPosition = pos;
    }

    public bool IsEmpty()
    {
        return placedObject == null;
    }

    public void PlaceObject(GameObject obj)
    {
        placedObject = obj;
        obj.transform.position = transform.position;
    }

    public void Clear()
    {
        if (placedObject != null)
            Destroy(placedObject);

        placedObject = null;
    }

    public GameObject GetPlacedObject()
    {
        return placedObject;
    }
}