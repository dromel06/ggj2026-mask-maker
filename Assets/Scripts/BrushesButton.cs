using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BrushesButton : MonoBehaviour
{
    public float brushSize = 1.0f;
    public float selectedOffset = 10f;
    public PaintController paintController;


    Vector3 defaultPosition;

    void Start()
    {
        defaultPosition = GetComponent<RectTransform>().localPosition;
    }

    public void OnClick()
    {
        if (paintController != null && paintController.drawData != null)
        {
            paintController.changeDrawSize(brushSize);
            this.Selected(true);
            Debug.Log($"Brush size selected: {brushSize}");
        }
    }

    private void Selected(bool selected)
    {
        var button = GetComponent<Button>();

        if (selected && button != null)
        {
            var parent = transform.parent;
            foreach (Transform sibling in parent)
            {
                // Skip self
                if (sibling == transform) continue;

                var siblingButton = sibling.GetComponent<BrushesButton>();
                if (siblingButton != null)
                {
                    siblingButton.Selected(false);
                    // This localPosition reset might be redundant if Selected(false) handles it, 
                    // but keeping it to match original intention, though strictly Selected(false) logic below handles the specific offset.
                    // Actually, let's trust Selected(false) to handle visual state if we move visual state update outside.
                }
            }
        }

        GetComponent<RectTransform>().localPosition = defaultPosition + new Vector3(0, selected ? selectedOffset : 0, 0);

        if (button != null)
        {
            var colors = button.colors;
            colors.normalColor = selected ? Color.yellow : Color.white;
            button.colors = colors;
        }
    }
}
