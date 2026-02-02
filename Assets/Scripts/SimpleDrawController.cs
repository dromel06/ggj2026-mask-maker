using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleDrawController : MonoBehaviour
{
    public enum DrawMode { Line, Sphere }
    
    [Header("Settings")]
    public DrawMode drawMode = DrawMode.Line;
    [Tooltip("Material para las líneas dibujadas.")]
    public Material lineMaterial;
    [Tooltip("Distancia mínima entre puntos para suavizar el trazo.")]
    public float minDistance = 0.01f;
    [Tooltip("Offset para evitar z-fighting con la máscara.")]
    public float drawOffset = 0.01f;

    [Header("References")]
    [Tooltip("Referencia al PaintController para obtener configuraciones (color, tamaño).")]
    public PaintController paintController;

    private bool isAvailable = true;
    private LineRenderer currentLine;
    private List<Vector3> currentPoints = new List<Vector3>();

    void Start()
    {
        if (paintController == null)
            paintController = FindObjectOfType<PaintController>();
            
        // Setup default material if none provided (optional fallback)
        if (lineMaterial == null)
        {
            // Trying to create a basic default material in code is tricky for shaders, 
            // best to warn user to assign one.
            Debug.LogWarning("SimpleDrawController: No Line Material assigned!");
        }
    }

    void Update()
    {
        if (!isAvailable) return;
        if (paintController == null || !paintController.canPaint) return;

        // Input Logic
        // 0 = Left Mouse Button
        if (Input.GetMouseButtonDown(0))
        {
            StartStroke();
        }
        else if (Input.GetMouseButton(0))
        {
            UpdateStroke();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            EndStroke();
        }
    }

    public void SetIsAvailableTrue()
    {
        isAvailable = true;
    }

    public void SetIsAvailableFalse()
    {
        isAvailable = false;
        // If we were drawing, finish the stroke
        EndStroke();
    }

    public void FlushLines()
    {
        EndStroke();
    }

    private void StartStroke()
    {
        // Validate raycast
        if (!GetRaycastPoint(out RaycastHit hit)) return;

        if (drawMode == DrawMode.Line)
        {
            CreateLineObject();
            AddPoint(hit);
        }
        else if (drawMode == DrawMode.Sphere)
        {
            DrawSphere(hit);
            lastWorldPoint = hit.point; // Init for distance check
        }
    }

    private void UpdateStroke()
    {
        // For lines, we need a currentLine. For spheres, we just need to pass distance check.
        if (drawMode == DrawMode.Line && currentLine == null) return;

        if (GetRaycastPoint(out RaycastHit hit))
        {
            // Check distance to last point
            if (Vector3.Distance(lastWorldPoint, hit.point) > minDistance)
            {
                if (drawMode == DrawMode.Line)
                {
                    AddPoint(hit);
                }
                else if (drawMode == DrawMode.Sphere)
                {
                    DrawSphere(hit);
                    lastWorldPoint = hit.point;
                }
            }
        }
    }
    
    // Cached for distance check
    private Vector3 lastWorldPoint;

    private void EndStroke()
    {
        currentLine = null;
        currentPoints.Clear();
    }

    private void DrawSphere(RaycastHit hit)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        
        // Remove collider so it doesn't block future raycasts
        Destroy(sphere.GetComponent<Collider>());
        
        sphere.name = "DrawSphere";
        
        // Parent to Mask
        if (paintController != null && paintController.maskObject != null)
        {
            sphere.transform.SetParent(paintController.maskObject.transform, false);
            // Position in Local Space
            sphere.transform.localPosition = paintController.maskObject.transform.InverseTransformPoint(hit.point + (hit.normal * drawOffset));
        }
        else
        {
            sphere.transform.SetParent(transform);
            sphere.transform.position = hit.point + (hit.normal * drawOffset);
        }

        // Apply Settings
        if (paintController != null && paintController.drawData != null)
        {
            float size = paintController.drawData.LineSize;
            sphere.transform.localScale = Vector3.one * size;
            
            Color col = paintController.drawData.LineColor;
            Renderer r = sphere.GetComponent<Renderer>();
            if (r != null)
            {
                r.material = lineMaterial != null ? new Material(lineMaterial) : new Material(Shader.Find("Sprites/Default"));
                r.material.color = col;
            }
        }
        else
        {
            sphere.transform.localScale = Vector3.one * 0.05f;
        }
    }

    private void CreateLineObject()
    {
        GameObject lineObj = new GameObject("DrawLine");
        
        // Parent to the mask object if available, so it moves with it
        if (paintController != null && paintController.maskObject != null)
        {
            lineObj.transform.SetParent(paintController.maskObject.transform, false); // worldPositionStays = false implies local 0,0,0? No, we want worldPositionStays=true usually, but here we set positions manually.
            // Actually, we want to parent it.
            // And we will set points in LOCAL space of the mask.
        }
        else
        {
            lineObj.transform.SetParent(transform);
        }
        
        currentLine = lineObj.AddComponent<LineRenderer>();
        
        // Use Local Space so points move with parent
        currentLine.useWorldSpace = false; 
        
        // Apply Settings from PaintController.drawData
        if (paintController != null && paintController.drawData != null)
        {
            currentLine.startWidth = paintController.drawData.LineSize;
            currentLine.endWidth = paintController.drawData.LineSize;
            
            Color col = paintController.drawData.LineColor;
            currentLine.startColor = col;
            currentLine.endColor = col;
            
            currentLine.material = lineMaterial != null ? new Material(lineMaterial) : new Material(Shader.Find("Sprites/Default"));
            currentLine.material.color = col; 
        }
        else
        {
            currentLine.startWidth = 0.05f;
            currentLine.endWidth = 0.05f;
            currentLine.material = new Material(Shader.Find("Sprites/Default"));
        }
        
        currentLine.positionCount = 0;
        currentPoints.Clear();
    }

    private void AddPoint(RaycastHit hit)
    {
        if (currentLine == null) return;

        lastWorldPoint = hit.point;

        // Calculate position slightly offset from surface
        Vector3 worldPos = hit.point + (hit.normal * drawOffset);
        
        Vector3 finalPos = worldPos;

        // Convert to Local Space of the LineRenderer's parent (The Mask)
        if (currentLine.transform.parent != null)
        {
            finalPos = currentLine.transform.parent.InverseTransformPoint(worldPos);
        }

        currentPoints.Add(finalPos);
        currentLine.positionCount = currentPoints.Count;
        currentLine.SetPosition(currentPoints.Count - 1, finalPos);
    }

    private bool GetRaycastPoint(out RaycastHit hitInfo)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        return Physics.Raycast(ray, out hitInfo);
    }
}
