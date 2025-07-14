using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NodeSwipeHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Header("Swipe Settings")]
    public float minSwipeDistance = 50f; // Minimum distance to register as a swipe
    public float maxSwipeTime = 0.5f; // Maximum time to complete a swipe
    [Header("Debug")]
    public bool showDebugInfo = false; // Enable debug logging
    
    private Node node;
    private Vector2 startPosition;
    private Vector2 endPosition;
    private float startTime;
    private bool isDragging = false;
    private bool hasSwiped = false;
    
    // Public property to check if currently swiping
    public bool IsSwiping => isDragging || hasSwiped;
    
    // Method to reset swipe state
    public void ResetSwipeState()
    {
        isDragging = false;
        hasSwiped = false;
    }
    
    private IEnumerator ResetSwipeStateAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ResetSwipeState();
    }
    
    private void Awake()
    {
        node = GetComponent<Node>();
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        startPosition = eventData.position;
        startTime = Time.time;
        isDragging = false;
        hasSwiped = false;
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (hasSwiped) return; // Prevent multiple swipes
        
        isDragging = true;
        endPosition = eventData.position;
        
        // Check if swipe conditions are met
        float swipeDistance = Vector2.Distance(startPosition, endPosition);
        float swipeTime = Time.time - startTime;
        
        if (swipeDistance >= minSwipeDistance && swipeTime <= maxSwipeTime)
        {
            ProcessSwipe();
        }
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isDragging || hasSwiped)
        {
            // If we didn't drag or already swiped, let the normal click handler work
            return;
        }
        
        // If we dragged but didn't meet swipe criteria, treat as normal click
        float swipeDistance = Vector2.Distance(startPosition, endPosition);
        if (swipeDistance < minSwipeDistance)
        {
            // Allow normal click behavior
            ResetSwipeState();
            return;
        }
        
        // If we dragged but took too long, don't process as swipe
        float swipeTime = Time.time - startTime;
        if (swipeTime > maxSwipeTime)
        {
            ResetSwipeState();
            return;
        }
        
        // Reset swipe state after a short delay to allow click processing
        StartCoroutine(ResetSwipeStateAfterDelay(0.1f));
    }
    
    private void ProcessSwipe()
    {
        hasSwiped = true;
        
        // Calculate swipe direction
        Vector2 swipeDirection = (endPosition - startPosition).normalized;
        
        // Find the target node based on swipe direction
        Node targetNode = GetTargetNodeFromSwipeDirection(swipeDirection);
        
        if (targetNode != null && node.connectedNeighbors.Contains(targetNode))
        {
            // Initiate travel to the target node
            if (showDebugInfo)
                Debug.Log($"Swipe detected: {node.name} -> {targetNode.name}");
            HackingMinigame.Instance.TravelToNode(targetNode);
        }
        else
        {
            // Invalid swipe - no connected node in that direction
            if (showDebugInfo)
                Debug.Log($"Swipe direction has no connected node: {node.name} -> {(targetNode != null ? targetNode.name : "null")}");
        }
        
        // Reset swipe state after processing
        StartCoroutine(ResetSwipeStateAfterDelay(0.1f));
    }
    
    private Node GetTargetNodeFromSwipeDirection(Vector2 swipeDirection)
    {
        // Determine which direction had the strongest component
        float absX = Mathf.Abs(swipeDirection.x);
        float absY = Mathf.Abs(swipeDirection.y);
        
        if (absX > absY)
        {
            // Horizontal swipe
            if (swipeDirection.x > 0)
            {
                return node.GetRightNeighbor();
            }
            else
            {
                return node.GetLeftNeighbor();
            }
        }
        else
        {
            // Vertical swipe
            if (swipeDirection.y > 0)
            {
                return node.GetTopNeighbor();
            }
            else
            {
                return node.GetBottomNeighbor();
            }
        }
    }
} 