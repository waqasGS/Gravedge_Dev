using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class NodeContainer : MonoBehaviour
{
    public int rows;
    public int cols;
    private Node[,] nodeGrid;
    
    [Header("Node Configuration")]
    public int firewallCount = 3;
    public int antivirusCount = 3;
    [Range(0f, 1f)]
    public float firewallChance = 0.5f;
    [Range(0f, 1f)]
    public float antivirusChance = 0.5f;

    
    private void Start()
    {
        nodeGrid = new Node[rows, cols];
        var index = 0;

        for (var row = 0; row < rows; row++)
        for (var col = 0; col < cols; col++)
        {
            nodeGrid[row, col] = transform.GetChild(index).GetComponent<Node>();
            nodeGrid[row, col].row = row;
            nodeGrid[row, col].col = col;
            index++;
        }

        GenerateConnectedMaze(new Vector2Int(0, 0));
        AssignStartAndEnd(new Vector2Int(0, 0), 5); // Distance threshold adjustable
        AssignSpecialNodes();
    }

    public Node GetNode(int row, int col)
    {
        return nodeGrid[row, col];
    }

    public Node GetLeftNeighbor(int row, int col)
    {
        return col > 0 ? nodeGrid[row, col - 1] : null;
    }

    public Node GetRightNeighbor(int row, int col)
    {
        return col < cols - 1 ? nodeGrid[row, col + 1] : null;
    }

    public Node GetTopNeighbor(int row, int col)
    {
        return row > 0 ? nodeGrid[row - 1, col] : null;
    }

    public Node GetBottomNeighbor(int row, int col)
    {
        return row < rows - 1 ? nodeGrid[row + 1, col] : null;
    }

    public void GenerateConnectedMaze(Vector2Int startCoords)
    {
        var rand = new Random();
        var visited = new bool[rows, cols];
        var stack = new Stack<Node>();

        var startNode = nodeGrid[startCoords.x, startCoords.y];
        visited[startCoords.x, startCoords.y] = true;
        stack.Push(startNode);

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            var neighbors = new List<Node>();

            var left = GetLeftNeighbor(current.row, current.col);
            var right = GetRightNeighbor(current.row, current.col);
            var top = GetTopNeighbor(current.row, current.col);
            var bottom = GetBottomNeighbor(current.row, current.col);

            if (left != null && !visited[left.row, left.col]) neighbors.Add(left);
            if (right != null && !visited[right.row, right.col]) neighbors.Add(right);
            if (top != null && !visited[top.row, top.col]) neighbors.Add(top);
            if (bottom != null && !visited[bottom.row, bottom.col]) neighbors.Add(bottom);

            // Shuffle neighbors to randomize path
            ShuffleList(neighbors, rand);

            foreach (var neighbor in neighbors)
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

    private void EnableEdgeBetween(Node a, Node b)
    {
        if (!a.connectedNeighbors.Contains(b))
            a.connectedNeighbors.Add(b);
        if (!b.connectedNeighbors.Contains(a))
            b.connectedNeighbors.Add(a);

        var dRow = b.row - a.row;
        var dCol = b.col - a.col;

        if (dRow == 1) // b is below a
        {
            a.edgeDown?.SetActive(true);
            b.edgeUp?.SetActive(true);
        }
        else if (dRow == -1) // b is above a
        {
            a.edgeUp?.SetActive(true);
            b.edgeDown?.SetActive(true);
        }
        else if (dCol == 1) // b is to the right of a
        {
            a.edgeRight?.SetActive(true);
            b.edgeLeft?.SetActive(true);
        }
        else if (dCol == -1) // b is to the left of a
        {
            a.edgeLeft?.SetActive(true);
            b.edgeRight?.SetActive(true);
        }
    }

    private void ShuffleList<T>(List<T> list, Random rand)
    {
        for (var i = 0; i < list.Count; i++)
        {
            var j = rand.Next(i, list.Count);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    public void AssignStartAndEnd(Vector2Int startCoords, int minDistanceFromStart)
    {
        var startNode = nodeGrid[startCoords.x, startCoords.y];
        startNode.nodeType = NodeType.Start;
        startNode.UpdateNodeVisuals();

        var distances = new Dictionary<Node, int>();
        var queue = new Queue<Node>();

        distances[startNode] = 0;
        queue.Enqueue(startNode);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            var currentDistance = distances[current];

            foreach (var neighbor in GetConnectedNeighbors(current))
                if (!distances.ContainsKey(neighbor))
                {
                    distances[neighbor] = currentDistance + 1;
                    queue.Enqueue(neighbor);
                }
        }

        // Filter candidates by minimum distance
        var validEndCandidates = new List<Node>();
        foreach (var pair in distances)
            if (pair.Value >= minDistanceFromStart && pair.Key.nodeType != NodeType.Start)
                validEndCandidates.Add(pair.Key);

        if (validEndCandidates.Count == 0)
        {
            Debug.LogWarning("No valid end node found with the given distance.");
            return;
        }

        // Pick a random end node
        var rand = new Random();
        var endNode = validEndCandidates[rand.Next(validEndCandidates.Count)];
        endNode.nodeType = NodeType.End;
        endNode.UpdateNodeVisuals();
    }

    private List<Node> GetConnectedNeighbors(Node node)
    {
        return node.connectedNeighbors;
    }
    
    private void AssignSpecialNodes()
    {
        var rand = new Random();
        var availableNodes = new List<Node>();

        foreach (var node in nodeGrid)
        {
            if (node.nodeType == NodeType.Normal)
                availableNodes.Add(node);
        }

        ShuffleList(availableNodes, rand);

        int firewallAssigned = 0;
        int antivirusAssigned = 0;

        foreach (var node in availableNodes)
        {
            // Firewall chance
            if (firewallAssigned < firewallCount && rand.NextDouble() <= firewallChance)
            {
                node.nodeType = NodeType.Firewall;
                node.UpdateNodeVisuals();  // Or set overlayFirewall directly
                firewallAssigned++;
                continue;
            }

            // Antivirus chance
            if (antivirusAssigned < antivirusCount && rand.NextDouble() <= antivirusChance)
            {
                node.nodeType = NodeType.Antivius;
                node.UpdateNodeVisuals();  // Or set overlayAntivius directly
                antivirusAssigned++;
            }

            if (firewallAssigned >= firewallCount && antivirusAssigned >= antivirusCount)
                break;
        }
    }


}