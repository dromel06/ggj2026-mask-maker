using UnityEngine;
// using ForceDraw; // Removed to fix build errors & dependency

public class PaintController : MonoBehaviour
{

    [Header("Draw Controller")]
    public GameObject drawControllerObject;
    public GameObject maskObject;
    public SimpleDrawData drawData = new SimpleDrawData(); // Replaced DrawData
    public GameObject[] BrushesButtons;

    private SimpleDrawController drawLineController; // Replaced DrawLineController


    [Header("Rotación")]
    [Tooltip("Velocidad de rotación al arrastrar el ratón (derecho).")]
    public float rotationSpeed = 200f;
    [Tooltip("Ángulo mínimo relativo a la rotación horizontal inicial (grados).")]
    public float minAngle = -45f;
    [Tooltip("Ángulo máximo relativo a la rotación horizontal inicial (grados).")]
    public float maxAngle = 45f;

    [Header("Rotación vertical")]
    [Tooltip("Ángulo mínimo relativo a la rotación vertical inicial (grados).")]
    public float minVerticalAngle = -30f;
    [Tooltip("Ángulo máximo relativo a la rotación vertical inicial (grados).")]
    public float maxVerticalAngle = 30f;
    [Tooltip("Invertir eje Y al arrastrar (true para invertir).")]
    public bool invertY = false;

    [Space]
    [Tooltip("Si es false no se permitirá rotar (modo edición).")]
    public bool editMode = true;
    [Tooltip("Bloquea la rotación cuando es true.")]
    public bool rotationLocked = false;

    [Tooltip("Permite/deshabilita la rotación (WASD/tacto/ratón).")]
    public bool canRotate = true;

    [Tooltip("Control maestro para habilitar/deshabilitar toda la interacción (usado por el menú).")]
    public bool canPaint = true;

    [Header("Zoom")]
    public float mouseZoomSpeed = 100f; // Higher for scroll
    public float keyZoomSpeed = 20f;
    public float minZoom = 20f;
    public float maxZoom = 60f;

    [Header("Camera Rotation")]
    public float cameraRotationSpeed = 100f;
    public Vector2 cameraLimitX = new Vector2(-15, 15); // Pitch
    public Vector2 cameraLimitY = new Vector2(-15, 15); // Yaw

    float initialCameraPitch;
    float initialCameraYaw;
    float currentCameraPitch = 0;
    float currentCameraYaw = 0;

    float initialY;
    float currentRelAngleY;

    float initialX;
    float currentRelAngleX;


    // GameObject[] lastDrawnObjects; // No longer used directly here, managed by SimpleDrawController children

    // Estado local para gestionar disponibilidad del dibujo mientras se rota
    bool isRotating = false;

    void Start()
    {
        initialY = transform.localEulerAngles.y;
        currentRelAngleY = 0f;

        initialX = transform.localEulerAngles.x;
        currentRelAngleX = 0f;

        if (drawControllerObject != null)
        {
            drawLineController = drawControllerObject.GetComponent<SimpleDrawController>();
            // If SimpleDrawController is not yet added, we might need to add it or warn.
            // Assuming user will set it up or it's on the object.
        }

        if (Camera.main != null)
        {
            Vector3 camAngles = Camera.main.transform.localEulerAngles;
            initialCameraPitch = camAngles.x;
            initialCameraYaw = camAngles.y;
        }
    }

    void Update()
    {
        // Si no se permite pintar (ej. estamos en el menú), salimos inmediatamente
        if (!canPaint)
        {
            if (drawLineController != null) drawLineController.SetIsAvailableTrue(); // Or False? Logic was True to allowing 'reset'?
            // Actually valid logic: if menu is open, we shouldn't draw. SimpleDrawController checks canPaint too.
            return;
        }
        ;

        // No permitir rotación si no estamos en modo edición, está bloqueada o deshabilitada
        if (!editMode || rotationLocked || !canRotate)
        {
            if (isRotating)
            {
                isRotating = false;
                if (drawLineController != null) drawLineController.SetIsAvailableTrue();
            }
            return;
        }

        // Logic for flushing lines moved to SimpleDrawController internals or kept here?
        // SimpleDrawController handles input now.
        // We can keep 'FlushLines' call if needed explicitly, but SimpleDrawController does it on MouseUp.
        if (Input.GetMouseButtonUp(0))
        {
            if (drawLineController != null) drawLineController.FlushLines();
        }

        // Acumular cambios de rotación desde distintas fuentes: ratón derecho, toque, WASD/teclas (Horizontal/Vertical)
        float deltaRelY = 0f;
        float deltaRelX = 0f;

        // Mouse right button drag -> Camera Rotation (Look)
        bool isCameraRotating = false;
        if (Input.GetMouseButton(1))
        {
            isCameraRotating = true;
            // Camera Rotation Logic
            float mouseX = Input.GetAxis("Mouse X") * cameraRotationSpeed * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * cameraRotationSpeed * Time.deltaTime;
            if (invertY) mouseY = -mouseY;

            currentCameraYaw += mouseX;
            currentCameraPitch -= mouseY; // Pitch up/down

            currentCameraYaw = Mathf.Clamp(currentCameraYaw, cameraLimitY.x, cameraLimitY.y);
            currentCameraPitch = Mathf.Clamp(currentCameraPitch, cameraLimitX.x, cameraLimitX.y);

            Camera cam = Camera.main;
            if (cam != null)
            {
                cam.transform.localEulerAngles = new Vector3(
                    initialCameraPitch + currentCameraPitch,
                    initialCameraYaw + currentCameraYaw,
                    0f
                );
            }
        }

        // Touch (iOS/Android) for Camera Rotation
        if (Input.touchCount == 2)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Moved)
            {
                isCameraRotating = true;
                float touchMultiplier = 0.1f;
                float tX = t.deltaPosition.x * touchMultiplier * cameraRotationSpeed * Time.deltaTime;
                float tY = t.deltaPosition.y * touchMultiplier * cameraRotationSpeed * Time.deltaTime;
                if (invertY) tY = -tY;

                currentCameraYaw += tX;
                currentCameraPitch -= tY;

                currentCameraYaw = Mathf.Clamp(currentCameraYaw, cameraLimitY.x, cameraLimitY.y);
                currentCameraPitch = Mathf.Clamp(currentCameraPitch, cameraLimitX.x, cameraLimitX.y);

                Camera cam = Camera.main;
                if (cam != null)
                {
                    cam.transform.localEulerAngles = new Vector3(
                        initialCameraPitch + currentCameraPitch,
                        initialCameraYaw + currentCameraYaw,
                        0f
                    );
                }
            }
        }


        // WASD / Keys -> Object Rotation (Horizontal/Vertical)
        float axisH = Input.GetAxis("Horizontal"); // A/D or Left/Right
        float axisV = Input.GetAxis("Vertical");   // W/S or Up/Down
        if (Mathf.Abs(axisH) > 0.001f || Mathf.Abs(axisV) > 0.001f)
        {
            deltaRelY += axisH * rotationSpeed * Time.deltaTime;
            float keyY = axisV;
            if (invertY) keyY = -keyY;

            deltaRelX += -keyY * rotationSpeed * Time.deltaTime;
        }

        bool haveRotationInput = Mathf.Abs(deltaRelY) > 0.0001f || Mathf.Abs(deltaRelX) > 0.0001f;

        if (haveRotationInput)
        {
            currentRelAngleY = Mathf.Clamp(currentRelAngleY + deltaRelY, minAngle, maxAngle);
            currentRelAngleX = Mathf.Clamp(currentRelAngleX + deltaRelX, minVerticalAngle, maxVerticalAngle);

            Vector3 e = transform.localEulerAngles;
            e.y = initialY + currentRelAngleY;
            e.x = initialX + currentRelAngleX;
            transform.localEulerAngles = e;

            if (!isRotating)
            {
                isRotating = true;
                if (drawLineController != null) drawLineController.SetIsAvailableFalse();
            }
        }
        else
        {
            if (isRotating)
            {
                isRotating = false;
                if (drawLineController != null) drawLineController.SetIsAvailableTrue();
            }
        }

        // --- Zoom Logic ---
        float zoomDelta = -Input.mouseScrollDelta.y * mouseZoomSpeed * Time.deltaTime; // Mouse Wheel

        if (Input.GetKey(KeyCode.R)) // Zoom In
        {
            zoomDelta -= keyZoomSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.F)) // Zoom Out
        {
            zoomDelta += keyZoomSpeed * Time.deltaTime;
        }

        if (Mathf.Abs(zoomDelta) > 0.001f)
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                float newFOV = cam.fieldOfView + zoomDelta;
                cam.fieldOfView = Mathf.Clamp(newFOV, minZoom, maxZoom);
            }
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("T key pressed - toggling draw mode.");
            if (drawLineController != null)
            {
                if (drawLineController.drawMode == SimpleDrawController.DrawMode.Line)
                    drawLineController.drawMode = SimpleDrawController.DrawMode.Sphere;
                else
                    drawLineController.drawMode = SimpleDrawController.DrawMode.Line;

                Debug.Log($"Draw Mode: {drawLineController.drawMode}");
            }
        }


        switch (Input.inputString)
        {
            case "1":
                BrushesButtons[0].GetComponent<BrushesButton>().OnClick();
                break;
            case "2":
                BrushesButtons[1].GetComponent<BrushesButton>().OnClick();
                break;
            case "3":
                BrushesButtons[2].GetComponent<BrushesButton>().OnClick();
                break;
            case "4":
                BrushesButtons[3].GetComponent<BrushesButton>().OnClick();
                break;
        }


    }

    public void changeDrawSize(float newSize)
    {
        if (drawData != null)
        {
            drawData.LineSize = newSize;
        }
    }

    public void DeleteLines()
    {
        if (drawLineController != null)
        {
            foreach (Transform child in drawLineController.transform)
            {
                Destroy(child.gameObject);
            }
            drawLineController.FlushLines();
        }

    }
}