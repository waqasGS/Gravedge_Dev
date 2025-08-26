using UnityEngine;
using UnityEngine.Events;

public class DragAndMove : MonoBehaviour
{
    [Header("Drag Settings")]
    [SerializeField] private bool enableDragging = true;
    [SerializeField] private bool globalDragging = false;
    [SerializeField] private float dragSpeed = 1f;
    [SerializeField] private bool constrainToScreen = true;
    [SerializeField] private bool snapToGrid = false;
    [SerializeField] private float gridSize = 1f;
    
    [Header("Drag Threshold")]
    [SerializeField] private bool useDragThreshold = false;
    [SerializeField] private float dragThreshold = 2f;
    [SerializeField] private bool thresholdReached = false;
    [SerializeField] private bool useAccumulatedTravel = true; // New option to choose between accumulated travel and direct distance
    [SerializeField] private bool resetAccumulatedDistanceOnDragEnd = true; // New option to control whether accumulated distance resets on drag end
    
    [Header("Movement Constraints")]
    [SerializeField] private bool lockX = false;
    [SerializeField] private bool lockY = false;
    [SerializeField] private bool lockZ = false;
    
    [Header("Position Limits")]
    [SerializeField] private bool usePositionLimits = false;
    [SerializeField] private Vector3 minPosition = Vector3.zero;
    [SerializeField] private Vector3 maxPosition = Vector3.zero;
    
    [Header("Events")]
    public UnityEvent OnDragThresholdReached;
    public UnityEvent OnDragThresholdReset;
    
    private Camera mainCamera;
    private bool isDragging = false;
    private Vector3 offset;
    private Vector3 originalPosition;
    private float originalZ;
    private Vector3 dragStartPosition;
    private Vector3 lastDragPosition; // Track the last position for accumulated travel
    private float accumulatedTravelDistance = 0f; // Track total distance traveled
    
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
            dragStartPosition = transform.position;
            lastDragPosition = transform.position;
            // Only reset accumulated distance if the option is enabled
            if (resetAccumulatedDistanceOnDragEnd)
            {
                accumulatedTravelDistance = 0f;
            }
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
                dragStartPosition = transform.position;
                lastDragPosition = transform.position;
                // Only reset accumulated distance if the option is enabled
                if (resetAccumulatedDistanceOnDragEnd)
                {
                    accumulatedTravelDistance = 0f;
                }
            }
        }
        else
        {
            // If no raycast hit, try to drag anyway (useful for UI elements)
            isDragging = true;
            Vector3 worldPoint = mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, originalZ));
            offset = transform.position - worldPoint;
            dragStartPosition = transform.position;
            lastDragPosition = transform.position;
            // Only reset accumulated distance if the option is enabled
            if (resetAccumulatedDistanceOnDragEnd)
            {
                accumulatedTravelDistance = 0f;
            }
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
        
        // Track accumulated travel distance AFTER position update
        if (isDragging && useAccumulatedTravel)
        {
            float frameDistance = Vector3.Distance(transform.position, lastDragPosition);
            accumulatedTravelDistance += frameDistance;
        }
        
        // Update last position for next frame
        lastDragPosition = transform.position;
        
        // Check drag threshold if enabled
        if (useDragThreshold)
        {
            CheckDragThreshold();
        }
    }
    
    void EndDrag()
    {
        isDragging = false;
        
        // Reset accumulated travel
        if (resetAccumulatedDistanceOnDragEnd)
        {
            accumulatedTravelDistance = 0f;
        }
        
        // Reset threshold when drag ends
        if (useDragThreshold && thresholdReached)
        {
            thresholdReached = false;
            OnDragThresholdReset?.Invoke();
        }
    }
    
    void CheckDragThreshold()
    {
        if (!isDragging) return;
        
        float dragDistance;
        
        if (useAccumulatedTravel)
        {
            // Use accumulated travel distance
            dragDistance = accumulatedTravelDistance;
        }
        else
        {
            // Use direct distance from start position
            dragDistance = Vector3.Distance(transform.position, dragStartPosition);
        }
        
        if (!thresholdReached && dragDistance >= dragThreshold)
        {
            thresholdReached = true;
            OnDragThresholdReached?.Invoke();
        }
        else if (thresholdReached && dragDistance < dragThreshold)
        {
            thresholdReached = false;
            OnDragThresholdReset?.Invoke();
        }
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
        
        // Reset threshold state
        if (useDragThreshold && thresholdReached)
        {
            thresholdReached = false;
            OnDragThresholdReset?.Invoke();
        }
    }
    
    public void ResetDragThreshold()
    {
        if (useDragThreshold && thresholdReached)
        {
            thresholdReached = false;
            OnDragThresholdReset?.Invoke();
        }
        dragStartPosition = transform.position;
        lastDragPosition = transform.position;
        accumulatedTravelDistance = 0f;
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
    
    // Drag threshold methods
    public void SetDragThresholdEnabled(bool enabled)
    {
        useDragThreshold = enabled;
        if (!enabled && thresholdReached)
        {
            thresholdReached = false;
            OnDragThresholdReset?.Invoke();
        }
    }
    
    public void SetDragThreshold(float threshold)
    {
        dragThreshold = Mathf.Max(0.1f, threshold);
    }
    
    public bool IsThresholdReached()
    {
        return thresholdReached;
    }
    
    public float GetCurrentDragDistance()
    {
        if (!isDragging) return 0f;
        
        if (useAccumulatedTravel)
        {
            return accumulatedTravelDistance;
        }
        else
        {
            return Vector3.Distance(transform.position, dragStartPosition);
        }
    }
    
    public float GetAccumulatedTravelDistance()
    {
        return accumulatedTravelDistance;
    }
    
    public float GetDragThreshold()
    {
        return dragThreshold;
    }
    
    public void SetAccumulatedTravelMode(bool enabled)
    {
        useAccumulatedTravel = enabled;
        // Reset accumulated travel when switching modes
        accumulatedTravelDistance = 0f;
    }
    
    public bool IsAccumulatedTravelMode()
    {
        return useAccumulatedTravel;
    }

    public void SetResetAccumulatedDistanceOnDragEnd(bool enabled)
    {
        resetAccumulatedDistanceOnDragEnd = enabled;
    }

    public bool GetResetAccumulatedDistanceOnDragEnd()
    {
        return resetAccumulatedDistanceOnDragEnd;
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
