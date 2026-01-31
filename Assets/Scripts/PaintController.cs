using UnityEngine;
using ForceDraw;


public class PaintController : MonoBehaviour
{

    [Header("Draw Controller")]
    public GameObject drawControllerObject;
    public GameObject maskObject;

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

    float initialY;
    float currentRelAngleY;

    float initialX;
    float currentRelAngleX;

    GameObject[] lastDrawnObjects;


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
        // No permitir rotación si no estamos en modo edición o si está bloqueada
        if (!editMode || rotationLocked) return;

        if (Input.GetMouseButtonUp(0)) // botón izquierdo soltado // cambiar los 
        {
            drawLineController.FlushLines();
            // if (drawControllerObject != null)
            // {
            //     foreach (Transform child in drawControllerObject.transform)
            //     {
            //         child.transform.parent = maskObject.transform;
            //     }
            // }
        }

        if (Input.GetMouseButton(1)) // botón derecho mantenido
        {
            float deltaY = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            currentRelAngleY = Mathf.Clamp(currentRelAngleY + deltaY, minAngle, maxAngle);

            float mouseY = Input.GetAxis("Mouse Y");
            if (invertY) mouseY = -mouseY;
            float deltaX = -mouseY * rotationSpeed * Time.deltaTime; // negativo para comportamiento más natural al arrastrar
            currentRelAngleX = Mathf.Clamp(currentRelAngleX + deltaX, minVerticalAngle, maxVerticalAngle);

            Vector3 e = transform.localEulerAngles;
            e.y = initialY + currentRelAngleY;
            e.x = initialX + currentRelAngleX;
            transform.localEulerAngles = e;
            drawLineController.SetIsAvailableFalse();
        }

        if (Input.GetMouseButtonUp(1))
        {
            drawLineController.SetIsAvailableTrue();
        }
    }
}