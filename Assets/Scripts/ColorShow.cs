using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ColorShow : MonoBehaviour
{
    public PaintController paintController;
    public Image displayImage;

    void Update()
    {
        if (paintController != null && paintController.drawData != null)
        {
            displayImage.color = paintController.drawData.LineColor;
        }
    }
}