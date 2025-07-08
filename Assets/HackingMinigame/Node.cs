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
    
    public bool isVisited = false;

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