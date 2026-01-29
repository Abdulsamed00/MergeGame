using UnityEngine;

public class GridCell : MonoBehaviour
{
    public Vector3Int cellPosition;
    public PlaceableObject currentObject;

    public bool IsEmpty()
    {
        return currentObject == null;
    }
}
