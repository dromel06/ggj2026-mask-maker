using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ColorWheelItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Button button;
    public Image image;
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

    public void Setup(int idx, Color color, ColorWheelController ctrl)
    {
        index = idx;
        controller = ctrl;
        if (image != null)
        {
            image.color = color;
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
