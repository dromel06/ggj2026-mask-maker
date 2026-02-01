using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ColorWheelController : MonoBehaviour
{
    public PaintController paintController;
    public Canvas targetCanvas; // Reference to the Canvas
    public float radius = 150f;
    public Color[] colors = new Color[] {
        Color.red, Color.green, Color.blue, Color.yellow, Color.cyan,
        Color.magenta, Color.white, Color.black, new Color(1, 0.5f, 0), new Color(0.5f, 0, 1)
    };

    private List<ColorWheelItem> items = new List<ColorWheelItem>();
    private int currentIndex = 0;
    private bool isVisible = true;

    void Start()
    {
        GenerateWheel();
        // Start hidden or visible? Assuming visible for now or waiting for input
        // Let's default to hidden until requested, or keep visible if that was previous behavior
        // User asked to position around mouse, implying it pops up.
        // Let's Hide initially.
        Hide();
    }

    void Update()
    {
        // Toggle visibility for testing, or listen for open command
        // For now, let's say 'C' toggles it, or we just rely on logic.
        // But user didn't specify OPEN trigger, only CLOSE triggers.
        // I will add a generic "Open" trigger on Right Click for consistency with previous context
        if ( Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(2))
        {
            if (isVisible)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }
    }

    public void Show()
    {
        isVisible = true;
        foreach (var item in items) item.gameObject.SetActive(true);

        // Position around mouse
        if (targetCanvas != null)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                targetCanvas.transform as RectTransform,
                Input.mousePosition,
                targetCanvas.worldCamera,
                out localPoint);

            transform.localPosition = localPoint;
        }
        else
        {
            transform.position = Input.mousePosition; // Fallback
        }
    }

    public void Hide()
    {
        isVisible = false;
        foreach (var item in items) item.gameObject.SetActive(false);
    }

    void GenerateWheel()
    {
        // ... (Cleanup old items if needed, but Start runs once)
        // Ensure we are attached to a RectTransform
        if (GetComponent<RectTransform>() == null)
        {
            gameObject.AddComponent<RectTransform>();
        }

        // Create a default button template if none exists
        GameObject template = new GameObject("ColorButtonTemplate");
        template.AddComponent<RectTransform>();
        template.AddComponent<Image>();
        template.AddComponent<Button>();
        template.AddComponent<ColorWheelItem>();
        template.SetActive(false);
        template.transform.SetParent(transform, false);

        float angleStep = 360f / colors.Length;

        for (int i = 0; i < colors.Length; i++)
        {
            GameObject btnObj = Instantiate(template, transform);
            // btnObj.SetActive(true); // Handled by Show()
            btnObj.name = "ColorButton_" + i;

            RectTransform rect = btnObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(50, 50); // Default size

            // Position in circle
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector2 pos = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            rect.anchoredPosition = pos;

            ColorWheelItem item = btnObj.GetComponent<ColorWheelItem>();
            item.button = btnObj.GetComponent<Button>();
            item.image = btnObj.GetComponent<Image>();
            item.Setup(i, colors[i], this);

            items.Add(item);
        }

        Destroy(template);
    }

    public void SelectColor(int index)
    {
        currentIndex = index;

        // Update UI
        for (int i = 0; i < items.Count; i++)
        {
            items[i].SetSelected(i == index);
        }

        // Update PaintController
        if (paintController != null && paintController.drawData != null)
        {
            paintController.drawData.LineColor = colors[index];
            Debug.Log($"Color selected: {colors[index]}");
        }

        Hide(); // Hide on selection
    }
}
