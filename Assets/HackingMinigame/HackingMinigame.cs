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
    public TextMeshProUGUI messageText;
    
    [Header("Player State")]
    public int accessLevel = 0; // This value determines if the player can pass special nodes
    
    [Header("Runtime")]
    public Node currentNode;
    public NodeContainer nodeContainer;
    
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
        nodeContainer = GetComponentInChildren<NodeContainer>();
        SetCurrentNode(0, 0);
    }

    private void SetCurrentNode(int row, int col)
    {
        if (currentNode != null)
        {
            currentNode.nodeCurrentVisual.SetActive(false);
        }
        currentNode = nodeContainer.GetNode(0, 0);      // Set Starting Node
        currentNode.nodeCurrentVisual.SetActive(true);
    }

    public void TravelToNode(Node targetNode)
    {
        if (!IsConnected(currentNode, targetNode))
            return;
        
        if (!TryConsumeAccess(targetNode))
        {
            Debug.Log("Access denied.");
            messageText.text = "Access denied.";
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
            messageText.text = $"Access level {accessLevel} too low for {targetNode.nodeType}. Required: {requiredLevel}";
            return false;
        }
        
        // Consume access level if it's a special node (not Normal, Start, or End)
        if (targetNode.nodeType == NodeType.Firewall || targetNode.nodeType == NodeType.Antivius)
        {
            AccessLevel -= requiredLevel;
            AccessLevel = Mathf.Max(0, accessLevel);

            Debug.Log($"Access level consumed: -{requiredLevel}. Remaining: {accessLevel}");
            messageText.text = $"Access level consumed: -{requiredLevel}. Remaining: {accessLevel}";
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

        // Fix: Disable from-node's standard edges too (needed for Start node edge visibility)
        DisableStandardEdges(from);
        
        // Reset the opposite travel edge on 'to'
        Image oppositeEdge = GetEdgeTravelBetween(to, from);
        if (oppositeEdge != null)
        {
            oppositeEdge.fillAmount = 0f;
            oppositeEdge.enabled = false;
        }

        // Disable normal edges on 'to' so the travel edge is fully visible
        DisableStandardEdges(to);

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

}