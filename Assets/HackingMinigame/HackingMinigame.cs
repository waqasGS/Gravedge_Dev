using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HackingMinigame : MonoBehaviour
{
    public Node currentNode;
    public NodeContainer nodeContainer;

    private void Start()
    {
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
}