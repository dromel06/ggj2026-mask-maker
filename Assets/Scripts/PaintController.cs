using UnityEngine;
using ForceDraw;

public class PaintController : MonoBehaviour
{

    [Header("Draw Controller")]
    public GameObject drawControllerObject;
    public GameObject maskObject;
    public DrawData drawData;
    public GameObject[] BrushesButtons;

    private DrawLineController drawLineController;


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

    float initialY;
    float currentRelAngleY;

    float initialX;
    float currentRelAngleX;

    GameObject[] lastDrawnObjects;

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
            drawLineController = drawControllerObject.GetComponent<DrawLineController>();
        }
    }

    void Update()
    {
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

        if (Input.GetMouseButtonUp(0)) // botón izquierdo soltado
        {
            if (drawLineController != null) drawLineController.FlushLines();
        }

        // Acumular cambios de rotación desde distintas fuentes: ratón derecho, toque, WASD/teclas (Horizontal/Vertical)
        float deltaRelY = 0f;
        float deltaRelX = 0f;

        // Mouse right button drag
        if (Input.GetMouseButton(1))
        {
            deltaRelY += Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y");
            if (invertY) mouseY = -mouseY;
            deltaRelX += -mouseY * rotationSpeed * Time.deltaTime;
        }

        // Touch (iOS/Android) - usar el deltaPosition del primer dedo
        if (Input.touchCount == 2)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Moved)
            {
                // Ajuste de sensibilidad para deltaPosition en píxeles
                float touchMultiplier = 0.1f;
                deltaRelY += t.deltaPosition.x * touchMultiplier * rotationSpeed * Time.deltaTime;
                float touchY = t.deltaPosition.y * touchMultiplier;
                if (invertY) touchY = -touchY;
                deltaRelX += -touchY * rotationSpeed * Time.deltaTime;
            }
        }

        // WASD / flechas (ejes Horizontal/Vertical)
        float axisH = Input.GetAxis("Horizontal");
        float axisV = Input.GetAxis("Vertical");
        if (Mathf.Abs(axisH) > 0.0001f || Mathf.Abs(axisV) > 0.0001f)
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
}