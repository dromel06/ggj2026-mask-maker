using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // Asegúrate de tener TextMeshPro instalado
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("El panel completo del menú principal (arrastrar aquí el GameObject 'PanelMenu' o similar).")]
    public GameObject menuPanel;

    [Tooltip("El texto del título del juego (arrastrar aquí el objeto con el componente TextMeshPro o Text).")]
    public TMP_Text gameTitleText; // Cambiar a 'Text' si usas UI Legacy

    [Tooltip("El panel de confirmación para cambiar máscara (debe estar dentro o sobre el menú).")]
    public GameObject confirmationPanel;

    [Tooltip("El panel del HUD del juego (se ocultará al mostrar el menú).")]
    public GameObject gameHUDPanel;

    [Tooltip("Referencia al PaintController para deshabilitarlo cuando el menú está activo.")]
    public PaintController paintController;

    [Header("Menu Effects")]
    [Tooltip("Intensidad del efecto de paralaje en el menú.")]
    public float parallaxStrength = 15f;

    [Header("Audio References")]
    [Tooltip("AudioSource para la música del menú.")]
    public AudioSource menuMusic;

    [Tooltip("AudioSource para la música del juego (in-game).")]
    public AudioSource gameMusic;

    [Header("Mask Management")]
    [Tooltip("Lista de prefabs de las máscaras disponibles.")]
    public List<GameObject> maskPrefabs;

    [Tooltip("Punto donde se instanciará la máscara (un Empty GameObject en la escena).")]
    public Transform maskSpawnPoint;

    // Estado interno
    private bool isMenuActive = true;
    private GameObject currentMaskInstance;
    private int pendingMaskIndex = -1;
    
    // Parallax state
    private Vector3 originalPos;
    private Quaternion originalRot;

    void Start()
    {
        // Inicialización: Asegurar estado inicial correcto
        if (menuPanel != null) menuPanel.SetActive(true);
        if (confirmationPanel != null) confirmationPanel.SetActive(false);
        if (gameHUDPanel != null) gameHUDPanel.SetActive(false); // HUD oculto al inicio (si empezamos en menú)
        
        isMenuActive = true;
        UpdateMusicState();
        UpdatePaintState();
        CaptureOriginalPaintTransform();
        
        // Pausar el tiempo si se desea que el menú detenga el juego
        Time.timeScale = 0f; 
    }

    void Update()
    {
        // Detectar tecla ESC para abrir/cerrar menú
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }

        // Efecto Parallax en el menú
        if (isMenuActive && paintController != null)
        {
            ApplyMenuParallax();
        }
    }

    private void CaptureOriginalPaintTransform()
    {
        if (paintController != null)
        {
            originalPos = paintController.transform.localPosition;
            originalRot = paintController.transform.localRotation;
        }
    }

    private void RestorePaintTransform()
    {
        if (paintController != null)
        {
            paintController.transform.localPosition = originalPos;
            paintController.transform.localRotation = originalRot;
        }
    }

    /// <summary>
    /// Aplica una rotación y posición sutil al PaintController basada en la posición del mouse.
    /// </summary>
    private void ApplyMenuParallax()
    {
        // Obtenemos posición mouse normalizada (-0.5 a 0.5)
        float mouseX = (Input.mousePosition.x / Screen.width) - 0.5f;
        float mouseY = (Input.mousePosition.y / Screen.height) - 0.5f;

        // --- Rotación ---
        float targetRotY = mouseX * parallaxStrength;
        float targetRotX = -mouseY * parallaxStrength; 
        Quaternion targetRotation = originalRot * Quaternion.Euler(targetRotX, targetRotY, 0f); // Relativo a la original

        // --- Posición (Movimiento suave) ---
        // Movemos un poco en X e Y opuesto al mouse para efecto de profundidad o directo
        float moveStrength = parallaxStrength * 0.01f; // Escalar fuerza para movimiento
        Vector3 targetPosition = originalPos + new Vector3(mouseX * moveStrength, mouseY * moveStrength, 0f);

        // Usamos unscaledDeltaTime porque el juego está en pausa
        paintController.transform.localRotation = Quaternion.Slerp(paintController.transform.localRotation, targetRotation, Time.unscaledDeltaTime * 5f);
        paintController.transform.localPosition = Vector3.Lerp(paintController.transform.localPosition, targetPosition, Time.unscaledDeltaTime * 5f);
    }

    /// <summary>
    /// Alterna la visibilidad del menú y el estado del juego.
    /// </summary>
    public void ToggleMenu()
    {
        isMenuActive = !isMenuActive;

        if (menuPanel != null)
        {
            menuPanel.SetActive(isMenuActive);
        }

        if (gameHUDPanel != null)
        {
            gameHUDPanel.SetActive(!isMenuActive);
        }

        // Si cerramos el menú, ocultamos también la confirmación si estaba abierta
        if (!isMenuActive && confirmationPanel != null)
        {
            confirmationPanel.SetActive(false);
            pendingMaskIndex = -1;
            
            // Restaurar transform original al salir del menú
            RestorePaintTransform();
        }
        else if (isMenuActive)
        {
            // Al entrar al menú, aseguramos tener la posición actual como base (opcional, o usamos la 'Start' base)
            // Por simplicidad usamos la capturada en Start o al entrar? 
            // Si el jugador giró la máscara, ¿queremos resetearla o mantenerla girada y añadir parallax?
            // El usuario dijo "abrir menu como inicio", tal vez resetear?
            // Vamos a capturar la actual al entrar para que el salto no sea brusco, pero el parallax sea relativo a esa.
            CaptureOriginalPaintTransform();
        }

        // Manejo del tiempo (Pausar juego si está en menú)
        Time.timeScale = isMenuActive ? 0f : 1f;

        UpdateMusicState();
        UpdatePaintState();
    }

    private void UpdatePaintState()
    {
        if (paintController != null)
        {
            paintController.canPaint = !isMenuActive;
        }
    }

    /// <summary>
    /// Actualiza qué música suena basándose en si estamos en el menú o no.
    /// </summary>
    private void UpdateMusicState()
    {
        if (isMenuActive)
        {
            // Activar música de menú, desactivar juego
            if (menuMusic != null && !menuMusic.isPlaying) menuMusic.Play();
            if (gameMusic != null) gameMusic.Stop(); // O Pause() si prefieres
        }
        else
        {
            // Activar música de juego, desactivar menú
            if (menuMusic != null) menuMusic.Stop();
            if (gameMusic != null && !gameMusic.isPlaying) gameMusic.Play();
        }
    }

    /// <summary>
    /// Método para llamar desde los botones de selección de máscara en el UI.
    /// </summary>
    /// <param name="maskIndex">Índice del prefab en la lista 'maskPrefabs'.</param>
    public void RequestChangeMask(int maskIndex)
    {
        if (maskIndex < 0 || maskIndex >= maskPrefabs.Count)
        {
            Debug.LogWarning("Indice de mascara invalido: " + maskIndex);
            return;
        }

        // Si ya hay una máscara, pedir confirmación
        if (currentMaskInstance != null)
        {
            pendingMaskIndex = maskIndex;
            if (confirmationPanel != null)
            {
                confirmationPanel.SetActive(true);
            }
            else
            {
                // Si no hay panel de confirmación, cambiamos directamente (fallback)
                ConfirmChangeMask(); 
            }
        }
        else
        {
            // Primera vez (o no hay máscara actual), instanciar directamente
            pendingMaskIndex = maskIndex;
            ConfirmChangeMask();
        }
    }

    /// <summary>
    /// Confirmar el cambio de máscara (Boton 'Sí' en el panel de confirmación).
    /// </summary>
    /// Confirmar el cambio de máscara (Boton 'Sí' en el panel de confirmación).
    /// </summary>
    public void ConfirmChangeMask()
    {
        if (pendingMaskIndex == -1) return;

        // Borrar anterior (Limpiar todo lo que haya en el spawn point)
        if (maskSpawnPoint != null)
        {
            foreach (Transform child in maskSpawnPoint)
            {
                Destroy(child.gameObject);
                paintController.DeleteLines();
            }
        }
        else if (currentMaskInstance != null)
        {
            // Fallback si no hay spawnpoint
            Destroy(currentMaskInstance);
        }

        // Instanciar nueva
        if (maskSpawnPoint != null)
        {
            currentMaskInstance = Instantiate(maskPrefabs[pendingMaskIndex], maskSpawnPoint.position, maskSpawnPoint.rotation, maskSpawnPoint);
        }
        else
        {
            // Si no hay punto de spawn, instanciar en el origen o donde sea
            currentMaskInstance = Instantiate(maskPrefabs[pendingMaskIndex]);
        }
        
        // Asegurar que paintController sepa de la nueva máscara (si es necesario)
        if (paintController != null)
        {
            // paintController.maskObject = currentMaskInstance; // Si PaintController necesita referencia explícita
        }

        // Resetear estado de confirmación
        if (confirmationPanel != null) confirmationPanel.SetActive(false);
        pendingMaskIndex = -1;

        // Opcional: Cerrar menú al seleccionar máscara?
        // ToggleMenu(); 
    }

    /// <summary>
    /// Cancelar el cambio de máscara (Boton 'No' en el panel de confirmación).
    /// </summary>
    public void CancelChangeMask()
    {
        pendingMaskIndex = -1;
        if (confirmationPanel != null) confirmationPanel.SetActive(false);
    }
    
    // Método para el botón de Salir del Juego
    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
