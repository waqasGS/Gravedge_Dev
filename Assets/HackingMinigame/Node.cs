using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Node : MonoBehaviour
{
    public GameObject nodeUnvisitedVisual;
    public GameObject nodeCurrentVisual;
    public GameObject nodeVisitedVisual;

    public GameObject edgeLeft;
    public GameObject edgeRight;
    public GameObject edgeDown;
    public GameObject edgeUp;
    
    public Image edgeLeftTravel;
    public Image edgeRightTravel;
    public Image edgeDownTravel;
    public Image edgeUpTravel;
    
    public GameObject overlayStart;
    public GameObject overlayEnd;
    public GameObject overlayFirewall;
    public GameObject overlayAntivius;
    
    public NodeType nodeType = NodeType.Normal;
    public NodeStatus nodeStatus =  NodeStatus.Unvisited;

    [Header("Runtime")]
    public int row;
    public int col;
    public NodeContainer nodeContainer;

    [HideInInspector]
    public List<Node> connectedNeighbors = new List<Node>();
    
    private void OnEnable()
    {
        nodeContainer = GetComponentInParent<NodeContainer>();
        connectedNeighbors.Clear();
        
        edgeDown.SetActive(false);
        edgeUp.SetActive(false);
        edgeLeft.SetActive(false);
        edgeRight.SetActive(false);
        
        // Then enable edges based on connection data
        foreach (var neighbor in connectedNeighbors)
        {
            var dRow = neighbor.row - row;
            var dCol = neighbor.col - col;

            if (dRow == 1) edgeDown.SetActive(true);
            else if (dRow == -1) edgeUp.SetActive(true);
            else if (dCol == 1) edgeRight.SetActive(true);
            else if (dCol == -1) edgeLeft.SetActive(true);
        }
        
        edgeDownTravel.fillAmount = 0.0f;
        edgeUpTravel.fillAmount = 0.0f;
        edgeLeftTravel.fillAmount = 0.0f;
        edgeRightTravel.fillAmount = 0.0f;
        
        nodeCurrentVisual.SetActive(false);
        nodeVisitedVisual.SetActive(false);
        nodeUnvisitedVisual.SetActive(true);
        
        UpdateNodeVisuals();
    }
    
    public void OnClick_Node()
    {
        // Only handle click if we're not currently swiping
        NodeSwipeHandler swipeHandler = GetComponent<NodeSwipeHandler>();
        if (swipeHandler == null || !swipeHandler.IsSwiping)
        {
            HackingMinigame.Instance.TravelToNode(this);
        }
    }

    public void UpdateNodeVisuals()
    {
        overlayStart.SetActive(false);
        overlayEnd.SetActive(false);
        overlayFirewall.SetActive(false);
        overlayAntivius.SetActive(false);

        switch (nodeType)
        {
            case NodeType.Start:
                overlayStart.SetActive(true);
                break;
            case NodeType.End:
                overlayEnd.SetActive(true);
                break;
            case NodeType.Firewall:
                overlayFirewall.SetActive(true);
                break;
            case NodeType.Antivius:
                overlayAntivius.SetActive(true);
                break;
            case NodeType.Normal:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        switch (nodeStatus)
        {
            case NodeStatus.Unvisited:
                nodeVisitedVisual.SetActive(false);
                nodeUnvisitedVisual.SetActive(true);
                break;
            case NodeStatus.Visited:
                nodeVisitedVisual.SetActive(true);
                nodeUnvisitedVisual.SetActive(false);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    public Node GetLeftNeighbor()
    {
        return nodeContainer.GetLeftNeighbor(row, col);
    }

    public Node GetRightNeighbor()
    {
        return nodeContainer.GetRightNeighbor(row, col);
    }

    public Node GetTopNeighbor()
    {
        return nodeContainer.GetTopNeighbor(row, col);
    }

    public Node GetBottomNeighbor()
    {
        return nodeContainer.GetBottomNeighbor(row, col);
    }
}

public enum NodeType
{
    Normal,
    Start,
    End,
    Firewall,
    Antivius,
}

public enum NodeStatus
{
    Unvisited,
    Visited
}