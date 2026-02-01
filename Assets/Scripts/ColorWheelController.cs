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

    [Header("Visuals")]
    public Sprite itemBackgroundImage; // Sprite para el fondo de cada item
    public Sprite itemColorSprite;     // Sprite para la forma del color (Círculo)

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
        // Structure: 
        // Root (RectTransform, Button, Script)
        //  -> Background (Image) [Child 0]
        //  -> Color (Image)      [Child 1]
        
        GameObject template = new GameObject("ColorButtonTemplate");
        RectTransform rt = template.AddComponent<RectTransform>();
        template.AddComponent<Button>();
        template.AddComponent<ColorWheelItem>();
        
        // --- Setup Background Image Object ---
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(template.transform, false);
        Image bgImg = bgObj.AddComponent<Image>();
        RectTransform bgRT = bgObj.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.sizeDelta = Vector2.zero; // Fill parent
        
        // --- Setup Color Image Object ---
        GameObject colorObj = new GameObject("ColorShape");
        colorObj.transform.SetParent(template.transform, false);
        Image colorImg = colorObj.AddComponent<Image>();
        RectTransform colorRT = colorObj.GetComponent<RectTransform>();
        colorRT.anchorMin = Vector2.zero;
        colorRT.anchorMax = Vector2.one;
        colorRT.sizeDelta = new Vector2(-10, -10); // Slight padding so background shows behind?
        // Or user can control via sprite size. Let's keep it full fill or small padding.
        
        // Configure Item
        ColorWheelItem templateItem = template.GetComponent<ColorWheelItem>();
        templateItem.button = template.GetComponent<Button>();
        templateItem.image = colorImg; // Main image controlled by script
        templateItem.backgroundImageItem = bgImg; // Decoration
        templateItem.button.targetGraphic = colorImg; // Animate the color not the root?
        
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
            
            // Re-bind references for instantiated object (Instantiate copies component refs to internal children correctly?)
            // Usually yes if they are in the hierarchy.
            // But 'image' and 'backgroundImageItem' point to Template children.
            // When instantiated, they point to NEW children. Smart Unity.
            
            // Assign Sprites
            if (item.image != null && itemColorSprite != null)
            {
                item.image.sprite = itemColorSprite;
            }
            // Background setup is in Setup()

            item.Setup(i, colors[i], this, itemBackgroundImage);

            items.Add(item);
        }
        
        Destroy(template); // Cleanup template
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
