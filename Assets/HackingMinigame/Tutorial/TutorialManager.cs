using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    [Header("Tutorial Data")]
    public TutorialStep[] tutorialSteps;
    
    [Header("UI References")]
    public TutorialUI tutorialUI;
    
    [Header("Settings")]
    public bool startTutorialOnStart = true;
    public bool tutorialCompleted = false;
    
    private int currentStepIndex = 0;
    private TutorialStep currentStep;
    private bool isTutorialActive = false;
    
    private void Start()
    {
        if (startTutorialOnStart && !tutorialCompleted)
        {
            StartTutorial();
        }
    }
    
    public void StartTutorial()
    {
        tutorialUI.gameObject.SetActive(true);

        if (tutorialSteps == null || tutorialSteps.Length == 0)
        {
            Debug.LogWarning("No tutorial steps assigned!");
            return;
        }
        
        isTutorialActive = true;
        currentStepIndex = 0;
        ShowCurrentStep();
    }
    
    private void ShowCurrentStep()
    {
        if (currentStepIndex >= tutorialSteps.Length)
        {
            EndTutorial();
            return;
        }
        
        currentStep = tutorialSteps[currentStepIndex];
        
        // Show tutorial UI
        tutorialUI.tutorialPanel.SetActive(true);
        tutorialUI.messageText.text = currentStep.messageText;
        
        // Enable highlight overlay for all steps
        if (tutorialUI.highlightOverlay != null)
        {
            tutorialUI.highlightOverlay.gameObject.SetActive(true);
        }
        
        // Handle node targeting if enabled
        if (currentStep.useNodeTargeting)
        {
            HandleNodeTargeting();
        }
        
        // Position arrow if showArrow is enabled and we have a target element
        if (currentStep.showArrow && currentStep.targetElement != null && tutorialUI.arrowImage != null)
        {
            PositionArrow();
        }
        
        // Handle arrow visibility based on showArrow setting
        if (tutorialUI.arrowImage != null)
        {
            tutorialUI.arrowImage.gameObject.SetActive(currentStep.showArrow);
        }
        
        // Position unmask if targeting is enabled or target element is set
        if (currentStep.useNodeTargeting || currentStep.targetElement != null)
        {
            PositionUnmask();
        }
        
        // Handle step action
        HandleStepAction();
    }
    
    private void PositionArrow()
    {
        RectTransform targetRect = currentStep.targetElement.GetComponent<RectTransform>();
        if (targetRect != null)
        {
            tutorialUI.arrowImage.rectTransform.position = targetRect.position + (Vector3)currentStep.arrowOffset;
            tutorialUI.arrowImage.color = currentStep.arrowColor;
        }
    }
    
    private void PositionUnmask()
    {
        if (tutorialUI.unmask == null)
        {
            Debug.LogWarning("Unmask GameObject is not assigned in TutorialUI!");
            return;
        }
        
        GameObject targetObject = null;
        
        // Determine the target object based on targeting type
        if (currentStep.useNodeTargeting)
        {
            // For node targeting, get the target node
            Node targetNode = GetTargetNode();
            if (targetNode != null)
            {
                targetObject = targetNode.gameObject;
            }
        }
        else if (currentStep.targetElement != null)
        {
            // For UI element targeting, use the target element
            targetObject = currentStep.targetElement;
        }
        
        if (targetObject != null )
        {
            // Get the RectTransform of the target object
            RectTransform targetRect = targetObject.GetComponent<RectTransform>();
            if (targetRect != null)
            {
                // Position the unmask at the target's position
                tutorialUI.unmask.GetComponent<RectTransform>().position = targetRect.position;
                tutorialUI.unmask.SetActive(true);
            }
            else
            {
                Debug.LogWarning($"Target object {targetObject.name} does not have a RectTransform component!");
            }
        }
        else
        {
            Debug.LogWarning("No valid target object found for unmask positioning!");
        }
    }
    
    private void HandleNodeTargeting()
    {
        // Get the target node from the grid coordinates
        Node targetNode = GetTargetNode();
        
        if (targetNode != null)
        {
            // Set the target element to the node's GameObject for arrow positioning
            currentStep.targetElement = targetNode.gameObject;
            
            // Highlight the node if requested
            if (currentStep.highlightTarget)
            {
                HighlightNode(targetNode);
            }
        }
        else
        {
            Debug.LogWarning($"Tutorial step {currentStep.stepNumber}: Target node not found at coordinates ({currentStep.targetNodeCoords.x}, {currentStep.targetNodeCoords.y})");
        }
    }
    
    private Node GetTargetNode()
    {
        // Get the node container from the hacking minigame
        if (HackingMinigame.Instance != null && HackingMinigame.Instance.nodeContainer != null)
        {
            // If node type targeting is enabled and a specific node type is set, find the first node of that type
            if (currentStep.useNodeTargeting && currentStep.targetNodeType != NodeType.Normal)
            {
                return FindFirstNodeOfType(currentStep.targetNodeType);
            }
            
            // Otherwise, use the coordinates as before
            int row = Mathf.RoundToInt(currentStep.targetNodeCoords.x);
            int col = Mathf.RoundToInt(currentStep.targetNodeCoords.y);
            
            return HackingMinigame.Instance.nodeContainer.GetNode(row, col);
        }
        
        return null;
    }
    
    private Node FindFirstNodeOfType(NodeType nodeType)
    {
        NodeContainer container = HackingMinigame.Instance.nodeContainer;
        
        // Search through all nodes in the grid
        for (int row = 0; row < container.rows; row++)
        {
            for (int col = 0; col < container.cols; col++)
            {
                Node node = container.GetNode(row, col);
                if (node != null && node.nodeType == nodeType)
                {
                    return node;
                }
            }
        }
        
        Debug.LogWarning($"No node of type {nodeType} found in the grid!");
        return null;
    }
    
    private void HighlightNode(Node node)
    {
        // Create a temporary highlight effect on the node
        // This could be done by temporarily changing the node's visual state
        // or by creating a highlight overlay
        
        // For now, we'll just log that the node should be highlighted
        Debug.Log($"Highlighting node at ({node.row}, {node.col}) for tutorial step {currentStep.stepNumber}");
        
        // TODO: Implement actual node highlighting visual effect
        // This could involve:
        // 1. Creating a highlight overlay on the node
        // 2. Temporarily changing the node's visual state
        // 3. Adding a pulsing effect
        // 4. Changing the node's color or adding a glow effect
    }
    
    private void ClearNodeHighlighting()
    {
        // Clear any node highlighting effects
        // This method will be called when moving to the next step
        // TODO: Implement actual node highlighting clearing
        Debug.Log("Clearing node highlighting");
    }
    
    private void HandleStepAction()
    {
        switch (currentStep.requiredAction)
        {
            case TutorialAction.Wait:
                StartCoroutine(WaitForDuration());
                break;
            case TutorialAction.Click:
                // Handle click actions
                if (currentStep.useNodeTargeting)
                {
                    // For node targeting, we wait for the OnNodeClicked method to be called
                    // The node click handling is done through the NodeClickHandler component
                    // No additional setup needed here as the system will automatically
                    // detect clicks on nodes and call OnNodeClicked
                }
                else if (currentStep.targetElement != null)
                {
                    // Add click listener to target element
                    Button targetButton = currentStep.targetElement.GetComponent<Button>();
                    if (targetButton != null)
                    {
                        targetButton.onClick.AddListener(OnTargetElementClicked);
                    }
                    else
                    {
                        // If no button component, add a click handler to the GameObject
                        StartCoroutine(WaitForClickOnTarget());
                    }
                }
                else
                {
                    // If no target element is set, wait for any click anywhere
                    StartCoroutine(WaitForAnyClick());
                }
                break;
            case TutorialAction.Navigate:
                // Wait for navigation action - this will be triggered externally
                // The tutorial will wait for OnNavigationOccurred() to be called
                break;
            case TutorialAction.Custom:
                // Handle custom trigger - wait for specific custom event
                if (!string.IsNullOrEmpty(currentStep.nextStepTrigger))
                {
                    StartCoroutine(WaitForCustomTrigger());
                }
                else
                {
                    // If no custom trigger specified, wait for a short duration
                    StartCoroutine(WaitForDuration());
                }
                break;
        }
    }
    
    private void OnTargetElementClicked()
    {
        // Remove the listener to prevent multiple calls
        if (currentStep?.targetElement != null)
        {
            Button targetButton = currentStep.targetElement.GetComponent<Button>();
            if (targetButton != null)
            {
                targetButton.onClick.RemoveListener(OnTargetElementClicked);
            }
        }
        
        NextStep();
    }
    
    private IEnumerator WaitForClickOnTarget()
    {
        bool clicked = false;
        
        while (!clicked && isTutorialActive && currentStep != null)
        {
            if (Input.GetMouseButtonDown(0)) // Left mouse button
            {
                // Check if click was on the target element
                Vector2 mousePosition = Input.mousePosition;
                RectTransform targetRect = currentStep.targetElement.GetComponent<RectTransform>();
                
                if (targetRect != null && RectTransformUtility.RectangleContainsScreenPoint(targetRect, mousePosition))
                {
                    clicked = true;
                }
            }
            yield return null;
        }
        
        if (clicked)
        {
            NextStep();
        }
    }
    
    private IEnumerator WaitForClickOnHighlight()
    {
        bool clicked = false;
        
        while (!clicked && isTutorialActive && currentStep != null)
        {
            if (Input.GetMouseButtonDown(0)) // Left mouse button
            {
                // Check if click was on the highlight overlay
                Vector2 mousePosition = Input.mousePosition;
                RectTransform highlightRect = tutorialUI.highlightOverlay.GetComponent<RectTransform>();
                
                if (highlightRect != null && RectTransformUtility.RectangleContainsScreenPoint(highlightRect, mousePosition))
                {
                    clicked = true;
                }
            }
            yield return null;
        }
        
        if (clicked)
        {
            // Hide the highlight overlay after successful click
            if (tutorialUI.highlightOverlay != null)
                tutorialUI.highlightOverlay.gameObject.SetActive(false);
            NextStep();
        }
    }
    
    private IEnumerator WaitForAnyClick()
    {
        bool clicked = false;
        
        while (!clicked && isTutorialActive && currentStep != null)
        {
            if (Input.GetMouseButtonDown(0)) // Left mouse button
            {
                clicked = true;
            }
            yield return null;
        }
        
        if (clicked)
        {
            NextStep();
        }
    }
    
    private IEnumerator WaitForCustomTrigger()
    {
        // For custom triggers, we'll wait for the OnCustomTrigger method to be called
        // This coroutine will just wait a frame to ensure the trigger system is ready
        yield return null;
        
        // The actual trigger handling is done in OnCustomTrigger method
        // This coroutine just ensures the step is properly initialized
    }
    
    private IEnumerator WaitForDuration()
    {
        yield return new WaitForSeconds(currentStep.waitDuration);
        NextStep();
    }
    
    public void NextStep()
    {
        // Clear any previous node highlighting
        ClearNodeHighlighting();
        
        // Hide unmask and highlight overlay when moving to next step
        if (tutorialUI.unmask != null)
        {
            tutorialUI.unmask.SetActive(false);
        }
        if (tutorialUI.highlightOverlay != null)
        {
            tutorialUI.highlightOverlay.gameObject.SetActive(false);
        }
        
        currentStepIndex++;
        ShowCurrentStep();
    }
    
    public void PreviousStep()
    {
        // Hide unmask and highlight overlay when moving to previous step
        if (tutorialUI.unmask != null)
        {
            tutorialUI.unmask.SetActive(false);
        }
        if (tutorialUI.highlightOverlay != null)
        {
            tutorialUI.highlightOverlay.gameObject.SetActive(false);
        }
        
        currentStepIndex = Mathf.Max(0, currentStepIndex - 1);
        ShowCurrentStep();
    }
    
    public void SkipTutorial()
    {
        EndTutorial();
    }
    
    private void EndTutorial()
    {
        isTutorialActive = false;
        tutorialCompleted = true;
        tutorialUI.tutorialPanel.SetActive(false);
        
        if (tutorialUI.arrowImage != null)
            tutorialUI.arrowImage.gameObject.SetActive(false);
            
        if (tutorialUI.highlightOverlay != null)
            tutorialUI.highlightOverlay.gameObject.SetActive(false);
            
        if (tutorialUI.unmask != null)
            tutorialUI.unmask.SetActive(false);
    }
    
    // Public methods for external triggers
    public void OnNodeClicked(GameObject node)
    {
        if (!isTutorialActive || currentStep == null) return;
        
        // Handle node targeting for click actions
        if (currentStep.requiredAction == TutorialAction.Click)
        {
            if (currentStep.useNodeTargeting)
            {
                // Check if the clicked node matches the target node coordinates
                Node clickedNode = node.GetComponent<Node>();
                Node targetNode = GetTargetNode();
                
                if (clickedNode != null && targetNode != null && clickedNode == targetNode)
                {
                    NextStep();
                }
            }
            else if (currentStep.targetElement == node)
            {
                // Handle regular UI element targeting
                NextStep();
            }
        }
    }
    
    public void OnNavigationOccurred()
    {
        if (!isTutorialActive || currentStep == null) return;
        
        if (currentStep.requiredAction == TutorialAction.Navigate)
        {
            NextStep();
        }
    }
    
    public void OnCustomTrigger(string triggerName)
    {
        if (!isTutorialActive || currentStep == null) return;
        
        if (currentStep.requiredAction == TutorialAction.Custom && 
            currentStep.nextStepTrigger == triggerName)
        {
            NextStep();
        }
    }
    
    public void SetCustomTrigger(string triggerName)
    {
        if (currentStep != null && currentStep.requiredAction == TutorialAction.Custom)
        {
            currentStep.nextStepTrigger = triggerName;
        }
    }
} 