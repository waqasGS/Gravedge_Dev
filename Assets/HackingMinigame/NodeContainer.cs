using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeContainer : MonoBehaviour
{
    public int rows;
    public int cols;
    private Node[,] nodeGrid;

    private void Start()
    {
        nodeGrid =  new Node[rows, cols];
        int index = 0;
        
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                nodeGrid[row, col] = transform.GetChild(index).GetComponent<Node>();
                nodeGrid[row, col].row = row;
                nodeGrid[row, col].col = col;
                index++;
            }
        }
        
        GenerateConnectedMaze(new Vector2Int(0, 0));
    }
    
    public Node GetLeftNeighbor(int row, int col)
    {
        return (col > 0) ? nodeGrid[row, col - 1] : null;
    }

    public Node GetRightNeighbor(int row, int col)
    {
        return (col < cols - 1) ? nodeGrid[row, col + 1] : null;
    }

    public Node GetTopNeighbor(int row, int col)
    {
        return (row > 0) ? nodeGrid[row - 1, col] : null;
    }

    public Node GetBottomNeighbor(int row, int col)
    {
        return (row < rows - 1) ? nodeGrid[row + 1, col] : null;
    }
    
    public void GenerateConnectedMaze(Vector2Int startCoords)
{
    System.Random rand = new System.Random();
    bool[,] visited = new bool[rows, cols];
    Stack<Node> stack = new Stack<Node>();

    Node startNode = nodeGrid[startCoords.x, startCoords.y];
    visited[startCoords.x, startCoords.y] = true;
    stack.Push(startNode);

    while (stack.Count > 0)
    {
        Node current = stack.Pop();
        List<Node> neighbors = new List<Node>();

        Node left = GetLeftNeighbor(current.row, current.col);
        Node right = GetRightNeighbor(current.row, current.col);
        Node top = GetTopNeighbor(current.row, current.col);
        Node bottom = GetBottomNeighbor(current.row, current.col);

        if (left != null && !visited[left.row, left.col]) neighbors.Add(left);
        if (right != null && !visited[right.row, right.col]) neighbors.Add(right);
        if (top != null && !visited[top.row, top.col]) neighbors.Add(top);
        if (bottom != null && !visited[bottom.row, bottom.col]) neighbors.Add(bottom);

        // Shuffle neighbors to randomize path
        ShuffleList(neighbors, rand);

        foreach (Node neighbor in neighbors)
        {
            if (!visited[neighbor.row, neighbor.col])
            {
                // Connect nodes
                EnableEdgeBetween(current, neighbor);
                visited[neighbor.row, neighbor.col] = true;
                stack.Push(current);
                stack.Push(neighbor);
                break; // Only one edge per node
            }
        }
    }
}

private void EnableEdgeBetween(Node a, Node b)
{
    int dRow = b.row - a.row;
    int dCol = b.col - a.col;

    if (dRow == 1) // b is below a
    {
        a.edgeDown.SetActive(true);
        b.edgeUp.SetActive(true);
    }
    else if (dRow == -1) // b is above a
    {
        a.edgeUp.SetActive(true);
        b.edgeDown.SetActive(true);
    }
    else if (dCol == 1) // b is to the right of a
    {
        a.edgeRight.SetActive(true);
        b.edgeLeft.SetActive(true);
    }
    else if (dCol == -1) // b is to the left of a
    {
        a.edgeLeft.SetActive(true);
        b.edgeRight.SetActive(true);
    }
}

private void ShuffleList<T>(List<T> list, System.Random rand)
{
    for (int i = 0; i < list.Count; i++)
    {
        int j = rand.Next(i, list.Count);
        (list[i], list[j]) = (list[j], list[i]);
    }
}

}