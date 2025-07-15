using UnityEngine;

[CreateAssetMenu(fileName = "TutorialStep", menuName = "Hacking Minigame/Tutorial Step")]
public class TutorialStep : ScriptableObject
{
    [Header("Step Information")]
    public int stepNumber;
    public string messageText;
    
    [Header("UI Targeting")]
    public GameObject targetElement; // The UI element to highlight/point to
    public Vector2 arrowOffset = Vector2.zero; // Offset for arrow position relative to target
    
    [Header("Node Targeting")]
    public bool useNodeTargeting = false;
    public Vector2 targetNodeCoords = Vector2.zero; // Grid coordinates (row, col) of the node to highlight
    public NodeType targetNodeType = NodeType.Normal; // Target a specific node type
    
    [Header("Step Behavior")]
    public TutorialAction requiredAction = TutorialAction.Wait;
    public float waitDuration = 0f; // For Wait actions
    public string nextStepTrigger = ""; // Custom trigger condition
    
    [Header("Visual Settings")]
    public bool highlightTarget = true;
    public bool dimBackground = false;
    public bool showArrow = true; // Whether to show the arrow for this step
    public Color arrowColor = Color.white;
}

public enum TutorialAction
{
    Wait,
    Click,
    Navigate,
    Custom
} 