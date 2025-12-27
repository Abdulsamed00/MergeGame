using UnityEngine;

public class CameraControlTool : MonoBehaviour
{
    [Header("References")]
    public Transform target;

    [Header("Settings")]
    public float rotateSpeed = 6f;
    private float distance;
    private float height;
    private float currentAngle;
    private float targetAngle;

    void Awake()
    {
        if (target == null)
        {
            GameObject t = new GameObject("CameraTarget");
            target = t.transform;
        }
    }
    
    public void InitFromGridCenter(Vector3 gridCenter)
    {
        target.position = gridCenter;

        Vector3 offset = transform.position - target.position;

        distance = new Vector2(offset.x, offset.z).magnitude;
        height = offset.y;

        currentAngle = Mathf.Atan2(offset.x, offset.z) * Mathf.Rad2Deg;
        targetAngle = currentAngle;
    }

    void LateUpdate()
    {
        currentAngle = Mathf.LerpAngle(
            currentAngle,
            targetAngle,
            Time.deltaTime * rotateSpeed
        );

        Vector3 direction = Quaternion.Euler(0f, currentAngle, 0f) * Vector3.back;
        Vector3 newPosition = target.position + direction * distance + Vector3.up * height;

        transform.position = newPosition;
        transform.LookAt(target);
    }

    public void RotateRight()
    {
        targetAngle += 90f;
    }

    public void RotateLeft()
    {
        targetAngle -= 90f;
    }
}
