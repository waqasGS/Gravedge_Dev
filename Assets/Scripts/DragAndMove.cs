using UnityEngine;

public class DragAndMove : MonoBehaviour
{
    [Header("Drag Settings")]
    [SerializeField] private bool enableDragging = true;
    [SerializeField] private bool globalDragging = false;
    [SerializeField] private float dragSpeed = 1f;
    [SerializeField] private bool constrainToScreen = true;
    [SerializeField] private bool snapToGrid = false;
    [SerializeField] private float gridSize = 1f;
    
    [Header("Movement Constraints")]
    [SerializeField] private bool lockX = false;
    [SerializeField] private bool lockY = false;
    [SerializeField] private bool lockZ = false;
    
    [Header("Position Limits")]
    [SerializeField] private bool usePositionLimits = false;
    [SerializeField] private Vector3 minPosition = Vector3.zero;
    [SerializeField] private Vector3 maxPosition = Vector3.zero;
    
    private Camera mainCamera;
    private bool isDragging = false;
    private Vector3 offset;
    private Vector3 originalPosition;
    private float originalZ;
    
    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindObjectOfType<Camera>();
        }
        
        if (mainCamera == null)
        {
            Debug.LogError("No camera found! DragAndMove script requires a camera to work.");
            enabled = false;
            return;
        }
        
        originalPosition = transform.position;
        originalZ = transform.position.z;
    }
    
    void Update()
    {
        if (!enableDragging) return;
        
        HandleInput();
    }
    
    void HandleInput()
    {
        // Mouse input
        if (Input.GetMouseButtonDown(0))
        {
            StartDrag();
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            ContinueDrag();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            EndDrag();
        }
        
        // Touch input for mobile
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    StartDrag();
                    break;
                case TouchPhase.Moved:
                    if (isDragging)
                        ContinueDrag();
                    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    EndDrag();
                    break;
            }
        }
    }
    
    void StartDrag()
    {
        Vector3 mousePosition = Input.mousePosition;
        
        // If global dragging is enabled, start dragging immediately
        if (globalDragging)
        {
            isDragging = true;
            Vector3 worldPoint = mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, originalZ));
            offset = transform.position - worldPoint;
            return;
        }
        
        // Otherwise, check if we clicked on this specific object
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.gameObject == gameObject)
            {
                isDragging = true;
                Vector3 worldPoint = mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, originalZ));
                offset = transform.position - worldPoint;
            }
        }
        else
        {
            // If no raycast hit, try to drag anyway (useful for UI elements)
            isDragging = true;
            Vector3 worldPoint = mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, originalZ));
            offset = transform.position - worldPoint;
        }
    }
    
    void ContinueDrag()
    {
        if (!isDragging) return;
        
        Vector3 mousePosition = Input.mousePosition;
        Vector3 worldPoint = mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, originalZ));
        Vector3 newPosition = worldPoint + offset;
        
        // Apply movement constraints
        if (lockX) newPosition.x = transform.position.x;
        if (lockY) newPosition.y = transform.position.y;
        if (lockZ) newPosition.z = transform.position.z;
        
        // Apply drag speed
        newPosition = Vector3.Lerp(transform.position, newPosition, dragSpeed * Time.deltaTime * 10f);
        
        // Snap to grid if enabled
        if (snapToGrid)
        {
            newPosition.x = Mathf.Round(newPosition.x / gridSize) * gridSize;
            newPosition.y = Mathf.Round(newPosition.y / gridSize) * gridSize;
            newPosition.z = Mathf.Round(newPosition.z / gridSize) * gridSize;
        }
        
        // Constrain to screen bounds if enabled
        if (constrainToScreen)
        {
            newPosition = ConstrainToScreenBounds(newPosition);
        }
        
        // Apply position limits if enabled
        if (usePositionLimits)
        {
            newPosition = ConstrainToPositionLimits(newPosition);
        }
        
        transform.position = newPosition;
    }
    
    void EndDrag()
    {
        isDragging = false;
    }
    
    Vector3 ConstrainToScreenBounds(Vector3 position)
    {
        Vector3 screenPoint = mainCamera.WorldToScreenPoint(position);
        
        // Get screen bounds
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        
        // Constrain X
        if (screenPoint.x < 0)
        {
            screenPoint.x = 0;
        }
        else if (screenPoint.x > screenWidth)
        {
            screenPoint.x = screenWidth;
        }
        
        // Constrain Y
        if (screenPoint.y < 0)
        {
            screenPoint.y = 0;
        }
        else if (screenPoint.y > screenHeight)
        {
            screenPoint.y = screenHeight;
        }
        
        // Convert back to world position
        Vector3 constrainedWorldPoint = mainCamera.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, originalZ));
        
        // Apply axis locks
        if (lockX) constrainedWorldPoint.x = transform.position.x;
        if (lockY) constrainedWorldPoint.y = transform.position.y;
        if (lockZ) constrainedWorldPoint.z = transform.position.z;
        
        return constrainedWorldPoint;
    }
    
    Vector3 ConstrainToPositionLimits(Vector3 position)
    {
        Vector3 constrainedPosition = position;
        
        // Clamp X position
        if (!lockX)
        {
            constrainedPosition.x = Mathf.Clamp(constrainedPosition.x, minPosition.x, maxPosition.x);
        }
        
        // Clamp Y position
        if (!lockY)
        {
            constrainedPosition.y = Mathf.Clamp(constrainedPosition.y, minPosition.y, maxPosition.y);
        }
        
        // Clamp Z position
        if (!lockZ)
        {
            constrainedPosition.z = Mathf.Clamp(constrainedPosition.z, minPosition.z, maxPosition.z);
        }
        
        return constrainedPosition;
    }
    
    // Public methods for external control
    public void SetDraggingEnabled(bool enabled)
    {
        enableDragging = enabled;
        if (!enabled)
        {
            isDragging = false;
        }
    }
    
    public void SetGlobalDragging(bool enabled)
    {
        globalDragging = enabled;
        if (!enabled)
        {
            isDragging = false;
        }
    }
    
    public void ResetToOriginalPosition()
    {
        transform.position = originalPosition;
    }
    
    public void SetDragSpeed(float speed)
    {
        dragSpeed = Mathf.Max(0.1f, speed);
    }
    
    public void SetGridSize(float size)
    {
        gridSize = Mathf.Max(0.1f, size);
    }
    
    public void SetPositionLimits(Vector3 min, Vector3 max)
    {
        minPosition = min;
        maxPosition = max;
        usePositionLimits = true;
    }
    
    public void SetPositionLimitsEnabled(bool enabled)
    {
        usePositionLimits = enabled;
    }
    
    public Vector3 GetMinPosition()
    {
        return minPosition;
    }
    
    public Vector3 GetMaxPosition()
    {
        return maxPosition;
    }
    
    // Gizmos for debugging
    void OnDrawGizmosSelected()
    {
        // Grid snapping visualization
        if (snapToGrid)
        {
            Gizmos.color = Color.yellow;
            Vector3 gridCenter = transform.position;
            gridCenter.x = Mathf.Round(gridCenter.x / gridSize) * gridSize;
            gridCenter.y = Mathf.Round(gridCenter.y / gridSize) * gridSize;
            gridCenter.z = Mathf.Round(gridCenter.z / gridSize) * gridSize;
            
            Gizmos.DrawWireCube(gridCenter, Vector3.one * gridSize);
        }
        
        // Position limits visualization
        if (usePositionLimits)
        {
            Gizmos.color = Color.red;
            Vector3 center = (minPosition + maxPosition) * 0.5f;
            Vector3 size = maxPosition - minPosition;
            Gizmos.DrawWireCube(center, size);
            
            // Draw min and max points
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(minPosition, 0.2f);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(maxPosition, 0.2f);
        }
    }
}
