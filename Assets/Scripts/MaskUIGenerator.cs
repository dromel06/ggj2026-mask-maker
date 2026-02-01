using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // Support for TextMeshPro

public class MaskUIGenerator : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Referencia al GameController para obtener la lista de máscaras.")]
    public GameController gameController;

    [Tooltip("El componente Dropdown de TextMeshPro que se llenará con las máscaras.")]
    public TMP_Dropdown maskDropdown;

    void Start()
    {
        if (gameController == null)
        {
            gameController = FindObjectOfType<GameController>();
        }

        if (gameController != null && maskDropdown != null)
        {
            SetupDropdown();
        }
        else
        {
            Debug.LogError("MaskUIGenerator: Faltan referencias (GameController o Dropdown).");
        }
    }

    void SetupDropdown()
    {
        // Limpiar opciones actuales y asegurar que empezamos limpio
        maskDropdown.ClearOptions();

        List<string> options = new List<string>();
        List<GameObject> masks = gameController.maskPrefabs;

        // Crear lista de nombres basada en los prefabs
        for (int i = 0; i < masks.Count; i++)
        {
            if (masks[i] != null)
            {
                options.Add(masks[i].name);
            }
            else
            {
                options.Add("Mask " + i);
            }
        }

        // Añadir opciones al dropdown
        maskDropdown.AddOptions(options);

        // Suscribirse al evento de cambio para detectar selección del usuario
        maskDropdown.onValueChanged.RemoveAllListeners();
        maskDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        
        // Opcional: Podrías querer sincronizar el valor inicial si `GameController` ya tiene una máscara activa
        // maskDropdown.value = 0; 
        // maskDropdown.RefreshShownValue();
    }

    void OnDropdownValueChanged(int index)
    {
        if (gameController != null)
        {
            gameController.RequestChangeMask(index);
        }
    }
}
