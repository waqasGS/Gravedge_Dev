using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public GameObject nodeUnvisitedVisual;
    public GameObject nodeCurrentVisual;
    public GameObject nodeVisitedVisual;

    public GameObject edgeLeft;
    public GameObject edgeRight;
    public GameObject edgeDown;
    public GameObject edgeUp;
    
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

    private void OnEnable()
    {
        nodeContainer = GetComponentInParent<NodeContainer>();
        
        edgeDown.SetActive(false);
        edgeUp.SetActive(false);
        edgeLeft.SetActive(false);
        edgeRight.SetActive(false);
        
        nodeCurrentVisual.SetActive(false);
        nodeVisitedVisual.SetActive(false);
        nodeUnvisitedVisual.SetActive(true);
        
        UpdateNodeVisuals();
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

    public void OnClick_Node()
    {
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