using UnityEngine;

public class MapOrbitCameraController : MonoBehaviour
{
    [Header("References")]
    public Camera cam;
    public Transform mapCenter;

    [Header("Rotation")]
    public float rotationSpeed = 0.2f;
    public float smoothSpeed = 8f;

    [Header("Zoom")]
    public float zoomSpeed = 0.02f;
    public float minDistance = 8f;
    public float maxDistance = 18f;

    float currentAngle;
    float targetAngle;
    float currentDistance;
    float initialHeight;

    Vector2 lastMidPoint; // İki parmağın ortasının son konumu
    Vector3 lastMousePos;

    void Start()
    {
        Vector3 offset = cam.transform.position - mapCenter.position;
        currentDistance = new Vector2(offset.x, offset.z).magnitude;
        initialHeight = cam.transform.position.y - mapCenter.position.y;
        currentAngle = Mathf.Atan2(offset.x, offset.z) * Mathf.Rad2Deg;
        targetAngle = currentAngle;
    }

    void Update()
    {
        // ================== MOBİL KONTROL (SADECE 2 PARMAK) ==================
        if (Input.touchCount == 2)
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);

            // İki parmağın arasındaki orta noktayı bul
            Vector2 currentMidPoint = (t0.position + t1.position) / 2f;

            // Eğer parmaklardan biri yeni dokunduysa, referans noktasını sıfırla
            // Bu, kameranın aniden sıçramasını engeller.
            if (t0.phase == TouchPhase.Began || t1.phase == TouchPhase.Began)
            {
                lastMidPoint = currentMidPoint;
            }
            else
            {
                // Grid üzerinde değilse (UI veya oyun alanı kontrolü)
                if (!IsTouchOnGrid(currentMidPoint))
                {
                    // 1. ROTASYON (Dönme)
                    // Orta noktanın ne kadar kaydığına bakarak döndürüyoruz
                    float deltaX = currentMidPoint.x - lastMidPoint.x;
                    targetAngle += deltaX * rotationSpeed;

                    // 2. ZOOM (Yakınlaşma)
                    ZoomTouch(t0, t1);
                }

                // Son pozisyonu güncelle
                lastMidPoint = currentMidPoint;
            }
        }

#if UNITY_EDITOR
        // MOUSE KONTROLLERİ (Test için aynı kalabilir)
        if (!IsTouchOnGrid(Input.mousePosition))
        {
            RotateMouse();
            ZoomMouse();
        }
#endif
    }

    void LateUpdate()
    {
        currentAngle = Mathf.LerpAngle(
            currentAngle,
            targetAngle,
            Time.deltaTime * smoothSpeed
        );

        Vector3 dir = Quaternion.Euler(0f, currentAngle, 0f) * Vector3.back;

        Vector3 newPos =
            mapCenter.position +
            (dir * currentDistance) + 
            (Vector3.up * initialHeight);

        cam.transform.position = newPos;
        cam.transform.LookAt(mapCenter);
    }

    // ================== GRID KONTROL ==================
    bool IsTouchOnGrid(Vector2 screenPos)
    {
        Ray ray = cam.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
        {
            int layerIndex = LayerMask.NameToLayer("GameArea");
            if(layerIndex != -1 && hit.collider.gameObject.layer == layerIndex)
                return true;
        }
        return false;
    }

    // ================== TOUCH FONKSİYONLARI ==================

    // Zoom fonksiyonunu Update içinden parametre alacak şekilde güncelledik
    void ZoomTouch(Touch t0, Touch t1)
    {
        // Önceki pozisyonları hesapla (Delta kullanarak)
        Vector2 t0Prev = t0.position - t0.deltaPosition;
        Vector2 t1Prev = t1.position - t1.deltaPosition;

        // Önceki karedeki parmak arası mesafe
        float prevDist = Vector2.Distance(t0Prev, t1Prev);

        // Şu anki parmak arası mesafe
        float currDist = Vector2.Distance(t0.position, t1.position);

        // Farkı al
        float diff = currDist - prevDist;

        // Mesafeyi uygula
        currentDistance -= diff * zoomSpeed;
        currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
    }

    // ================== MOUSE FONKSİYONLARI ==================
    void RotateMouse()
    {
        if (Input.GetMouseButtonDown(0))
            lastMousePos = Input.mousePosition;

        if (Input.GetMouseButton(0))
        {
            float deltaX = Input.mousePosition.x - lastMousePos.x;
            targetAngle += deltaX * rotationSpeed;
            lastMousePos = Input.mousePosition;
        }
    }

    void ZoomMouse()
    {
        float scroll = Input.mouseScrollDelta.y;
        if (scroll != 0)
        {
            currentDistance -= scroll * zoomSpeed * 20f;
            currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
        }
    }
}