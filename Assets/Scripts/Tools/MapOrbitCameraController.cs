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

    Vector2 lastTouchPos;
    Vector3 lastMousePos;

    void Start()
    {
        Vector3 offset = cam.transform.position - mapCenter.position;

        currentDistance = offset.magnitude;
        currentAngle = Mathf.Atan2(offset.x, offset.z) * Mathf.Rad2Deg;
        targetAngle = currentAngle;

        cam.transform.LookAt(mapCenter);
    }

    void Update()
    {
        // 📱 MOBİL
        if (Input.touchCount == 1)
        {
            Touch t = Input.GetTouch(0);

            if (!IsTouchOnGrid(t.position))
                RotateTouch(t);
        }
        else if (Input.touchCount == 2)
        {
            Vector2 mid =
                (Input.GetTouch(0).position + Input.GetTouch(1).position) * 0.5f;

            if (!IsTouchOnGrid(mid))
                ZoomTouch();
        }

#if UNITY_EDITOR
        // 🖱️ MOUSE
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
            dir * currentDistance +
            Vector3.up * (cam.transform.position.y - mapCenter.position.y);

        cam.transform.position = newPos;
        cam.transform.LookAt(mapCenter);
    }

    // ================== GRID KONTROL ==================

    bool IsTouchOnGrid(Vector2 screenPos)
    {
        Ray ray = cam.ScreenPointToRay(screenPos);

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
        {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("GameArea"))
                return true;
        }

        return false;
    }

    // ================== TOUCH ==================

    void RotateTouch(Touch t)
    {
        if (t.phase == TouchPhase.Began)
            lastTouchPos = t.position;

        if (t.phase == TouchPhase.Moved)
        {
            float deltaX = t.position.x - lastTouchPos.x;
            targetAngle += deltaX * rotationSpeed;
            lastTouchPos = t.position;
        }
    }

    void ZoomTouch()
    {
        Touch t0 = Input.GetTouch(0);
        Touch t1 = Input.GetTouch(1);

        float prevDist = Vector2.Distance(
            t0.position - t0.deltaPosition,
            t1.position - t1.deltaPosition
        );

        float currDist = Vector2.Distance(t0.position, t1.position);

        float diff = currDist - prevDist;

        currentDistance -= diff * zoomSpeed;
        currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
    }

    // ================== MOUSE ==================

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
