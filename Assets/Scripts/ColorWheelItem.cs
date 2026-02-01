using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ColorWheelItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Button button;
    public Image image;
    public Image backgroundImageItem; // Imagen de fondo para decoración
    public int index;
    public ColorWheelController controller;
    
    private Vector3 originalScale;
    private bool isSelected = false;

    void Awake()
    {
        originalScale = transform.localScale;
        if (button != null)
        {
            button.onClick.AddListener(OnClick);
        }
    }

    public void Setup(int idx, Color color, ColorWheelController ctrl, Sprite bgSprite)
    {
        index = idx;
        controller = ctrl;
        
        // Configurar color principal
        if (image != null)
        {
            image.color = color;
        }

        // Configurar fondo si existe
        if (backgroundImageItem != null && bgSprite != null)
        {
            backgroundImageItem.sprite = bgSprite;
            backgroundImageItem.gameObject.SetActive(true);
        }
        else if (backgroundImageItem != null)
        {
            // Ocultar si no hay sprite asignado
            backgroundImageItem.gameObject.SetActive(false); 
        }
    }

    void OnClick()
    {
        if (controller != null)
        {
            controller.SelectColor(index);
        }
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        UpdateScale();
    }

    void UpdateScale()
    {
        float scale = isSelected ? 1.3f : 1.0f;
        transform.localScale = originalScale * scale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isSelected)
        {
            transform.localScale = originalScale * 1.1f;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isSelected)
        {
            transform.localScale = originalScale;
        }
    }
}
