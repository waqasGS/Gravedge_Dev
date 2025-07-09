using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HackingMinigame : MonoBehaviour
{
    #region Singleton

    public static HackingMinigame Instance;

    private void Awake()
    {
        Instance = this;
    }

    #endregion

    public TextMeshProUGUI accessLevelText;
    public Slider timeLeftSlider;
    
    [Header("Timer Settings")]
    public float hackTimeLimit = 60f; // Time limit in seconds
    public bool isTimerRunning = false;
    
    [Header("Player State")]
    public int accessLevel = 0; // This value determines if the player can pass special nodes
    
    [Header("Runtime")]
    public Node currentNode;
    public NodeContainer nodeContainer;
    
    public Action onReachedEndNode;
    public Action onHackFailed;
    
    private float currentTimeLeft;
    
    public int AccessLevel
    {
        get => accessLevel;
        set
        {
            accessLevel = value;
            accessLevelText.text = value.ToString();
        }
    }

    private void Start()
    {
        AccessLevel = accessLevel;      // to update text
        InitializeTimer();
        
        nodeContainer = GetComponentInChildren<NodeContainer>();
        nodeContainer.Init();
        SetCurrentNode(0, 0);
        
        onReachedEndNode += OnReachedEndNode;
        onHackFailed += OnHackFailed;
        
        StartTimer();
    }
    
    private void InitializeTimer()
    {
        currentTimeLeft = hackTimeLimit;
        timeLeftSlider.maxValue = hackTimeLimit;
        timeLeftSlider.value = hackTimeLimit;
    }
    
    private void StartTimer()
    {
        isTimerRunning = true;
        StartCoroutine(CountdownTimer());
    }
    
    private IEnumerator CountdownTimer()
    {
        while (currentTimeLeft > 0f && isTimerRunning)
        {
            currentTimeLeft -= Time.deltaTime;
            timeLeftSlider.value = currentTimeLeft;
            
            if (currentTimeLeft <= 0f)
            {
                currentTimeLeft = 0f;
                timeLeftSlider.value = 0f;
                onHackFailed?.Invoke();
                break;
            }
            
            yield return null;
        }
    }

    private void OnReachedEndNode()
    {
        isTimerRunning = false;
        StartCoroutine(AnimateHackSuccess());
    }
    
    private void OnHackFailed()
    {
        isTimerRunning = false;
        StartCoroutine(AnimateHackFailure());
    }

    private IEnumerator AnimateHackSuccess()
    {
        MessageLine.Instance.ShowMessage("Hack Successfull", Color.green);
        yield return new WaitForSeconds(1.0f);
        EndHack();
    }
    
    private IEnumerator AnimateHackFailure()
    {
        MessageLine.Instance.ShowMessage("Hack Failed - Time's Up!", Color.red);
        yield return new WaitForSeconds(1.0f);
        EndHack();
    }

    private void SetCurrentNode(int row, int col)
    {
        if (currentNode != null)
        {
            currentNode.nodeCurrentVisual.SetActive(false);
        }
        currentNode = nodeContainer.GetNode(row, col);      // Set Starting Node
        currentNode.nodeCurrentVisual.SetActive(true);
    }

    public void TravelToNode(Node targetNode)
    {
        if (!IsConnected(currentNode, targetNode))
            return;
        
        if (!TryConsumeAccess(targetNode))
        {
            Debug.Log("Access denied.");
            MessageLine.Instance.ShowMessage("Access denied", Color.red);
            return;
        }
        
        StartCoroutine(AnimateTravel(currentNode, targetNode));
    }
    
    private bool TryConsumeAccess(Node targetNode)
    {
        // Enforce access restriction based on node type
        int requiredLevel = GetRequiredAccessForNodeType(targetNode.nodeType);
        if (accessLevel < requiredLevel)
        {
            Debug.Log($"Access level {accessLevel} too low for {targetNode.nodeType}. Required: {requiredLevel}");
            MessageLine.Instance.ShowMessage($"Access level {accessLevel} too low for {targetNode.nodeType}. Required: {requiredLevel}", Color.yellow);
            return false;
        }
        
        // Consume access level only if it's a special node AND we haven't visited it before
        if ((targetNode.nodeType == NodeType.Firewall || targetNode.nodeType == NodeType.Antivius) && 
            targetNode.nodeStatus == NodeStatus.Unvisited)
        {
            AccessLevel -= requiredLevel;
            AccessLevel = Mathf.Max(0, accessLevel);

            Debug.Log($"Access level consumed: -{requiredLevel}. Remaining: {accessLevel}");
            MessageLine.Instance.ShowMessage($"Access level consumed: {requiredLevel}. Remaining: {accessLevel}", Color.cyan);
        }
        else if ((targetNode.nodeType == NodeType.Firewall || targetNode.nodeType == NodeType.Antivius) && 
                 targetNode.nodeStatus == NodeStatus.Visited)
        {
            Debug.Log($"Already visited {targetNode.nodeType} node. No access consumed.");
            MessageLine.Instance.ShowMessage($"Already visited {targetNode.nodeType} node. No access consumed.", Color.green);
        }

        return true;
    }
    
    private IEnumerator AnimateTravel(Node from, Node to)
    {
        Image edgeToAnimate = GetEdgeTravelBetween(from, to);
        if (edgeToAnimate == null)
            yield break;

        // Disable all other travel edges on 'from' except the current one
        DisableOtherEdges(from, edgeToAnimate);

        // Disable from-node's standard edges too (needed for Start node edge visibility)
        DisableStandardEdges(from);
        
        // Reset the opposite travel edge on 'to'
        Image oppositeEdge = GetEdgeTravelBetween(to, from);
        if (oppositeEdge != null)
        {
            oppositeEdge.fillAmount = 0f;
            oppositeEdge.enabled = false;
        }

        // Only disable the specific standard edge on 'to' that would interfere with travel animation
        DisableSpecificStandardEdge(to, from);

        float duration = 0.3f;
        float elapsed = 0f;

        edgeToAnimate.fillAmount = 0f;
        edgeToAnimate.enabled = true;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            edgeToAnimate.fillAmount = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }

        edgeToAnimate.fillAmount = 1f;

        // Transition complete — update node visuals
        from.nodeCurrentVisual.SetActive(false);
        to.nodeCurrentVisual.SetActive(true);
        from.nodeStatus = NodeStatus.Visited;
        from.UpdateNodeVisuals();

        currentNode = to;
        
        if (to.nodeType == NodeType.End)
        {
            onReachedEndNode?.Invoke();
        }
    }


    private void DisableOtherEdges(Node node, Image exceptEdge)
    {
        var edges = new List<Image>
        {
            node.edgeLeftTravel,
            node.edgeRightTravel,
            node.edgeUpTravel,
            node.edgeDownTravel
        };

        foreach (var edge in edges)
        {
            if (edge != null && edge != exceptEdge && edge.enabled)
            {
                edge.enabled = false;
                edge.fillAmount = 0f;
            }
        }
    }
    
    private int GetRequiredAccessForNodeType(NodeType type)
    {
        switch (type)
        {
            case NodeType.Firewall:
                return 2;
            case NodeType.Antivius:
                return 3;
            case NodeType.Start:
            case NodeType.End:
                return 0; // Always accessible
            case NodeType.Normal:
            default:
                return 0;
        }
    }
    
    private void DisableStandardEdges(Node node)
    {
        if (node.edgeLeft != null) node.edgeLeft.SetActive(false);
        if (node.edgeRight != null) node.edgeRight.SetActive(false);
        if (node.edgeUp != null) node.edgeUp.SetActive(false);
        if (node.edgeDown != null) node.edgeDown.SetActive(false);
    }
    
    private void DisableSpecificStandardEdge(Node node, Node fromNode)
    {
        // Determine which direction we're coming from and disable only that specific edge
        if (fromNode == node.GetLeftNeighbor())
        {
            if (node.edgeLeft != null) node.edgeLeft.SetActive(false);
        }
        else if (fromNode == node.GetRightNeighbor())
        {
            if (node.edgeRight != null) node.edgeRight.SetActive(false);
        }
        else if (fromNode == node.GetTopNeighbor())
        {
            if (node.edgeUp != null) node.edgeUp.SetActive(false);
        }
        else if (fromNode == node.GetBottomNeighbor())
        {
            if (node.edgeDown != null) node.edgeDown.SetActive(false);
        }
    }
    
    private bool IsConnected(Node from, Node to)
    {
        return from.connectedNeighbors.Contains(to);
    }
    
    private Image GetEdgeTravelBetween(Node from, Node to)
    {
        if (to == from.GetLeftNeighbor())
            return from.edgeLeftTravel;
        if (to == from.GetRightNeighbor())
            return from.edgeRightTravel;
        if (to == from.GetTopNeighbor())
            return from.edgeUpTravel;
        if (to == from.GetBottomNeighbor())
            return from.edgeDownTravel;

        return null;
    }

    public void OnClick_AbortHack()
    {
        EndHack();
    }

    private void EndHack()
    {
        isTimerRunning = false;
        StopAllCoroutines();
        Destroy(gameObject);
    }
}