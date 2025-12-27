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

        Vector3 offset = transform.position - target.position;//Kamera ile target arasındaki vektörel fark alınıyor

        distance = new Vector2(offset.x, offset.z).magnitude;//Kameranın targeta olan yatay uzaklığını hesaplanır
        height = offset.y;//Kameranın y eksenini saklar

        currentAngle = Mathf.Atan2(offset.x, offset.z) * Mathf.Rad2Deg;//Kameranın targeta göre hangi açıda olduğunu bulur
        //Mathf.Atan2 ile bir noktanın merkeze olan açısını verir
        //Mathf.Rad2Deg ile radyanı dereceye çevirir(radians to degrees)
        targetAngle = currentAngle;
    }

    void LateUpdate()
    {
        currentAngle = Mathf.LerpAngle(currentAngle, targetAngle, Time.deltaTime * rotateSpeed);//currentAngle'dan targetAngle'a yavaş yavaş dön

        Vector3 direction = Quaternion.Euler(0f, currentAngle, 0f) * Vector3.back;
        //Kamera unityde varsayılan olan -z ekseni yönüne bakar o yüzden Vector3.back kullanılır.
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
