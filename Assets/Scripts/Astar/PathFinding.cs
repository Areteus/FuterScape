using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinding : MonoBehaviour
{
    public Transform seeker, target;

    Grid grid;

    private void Awake()
    {
        grid = GetComponent<Grid>();
    }

    private void Update()
    {
        FindPath(seeker.position, target.position);
    }

    void FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node targetNode = grid.NodeFromWorldPoint(targetPos);

        List<Node> openSet = new List<Node>(); // the set nodes to be evaluated 
        HashSet<Node> closedSet = new HashSet<Node>(); //the set of nodes already evaluated
        openSet.Add(startNode); 
        while (openSet.Count > 0)
        {
            Node currentNode = openSet[0]; //gets the first element in the open set
            for (int i = 0; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentNode.fCost || openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost) // if the node in the open set has an f/h cost that is less then the current nodes f/h cost
                {
                    currentNode = openSet[i]; // sets the current node equal to that node
                }
            }

            openSet.Remove(currentNode); //removing current node from the open and adding it to closed set
            closedSet.Add(currentNode);

            if (currentNode == targetNode) //exit out of loop because path has been found
            {
                RetracePath(startNode, targetNode);
                return;
            }

            foreach (Node neighbour in grid.GetNeighbours(currentNode)) //gets list of nodes in the GetNeighbours and looping through all of the neighbours 
            {
                if(!neighbour.walkable || closedSet.Contains(neighbour))
                {
                    continue;
                }
                // if new path to neighbour is shorter or neighbour is not open
                int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour); // cost of getting current node to neighbour node 
                if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    //set the fcost by calculating gcost + hcost
                    neighbour.gCost = newMovementCostToNeighbour; // calculate it to new g cost wich is newMovementCostToNeighbour
                    neighbour.hCost = GetDistance(neighbour, targetNode); // calculate as distance from the node to the end node
                    neighbour.parent = currentNode; //setting the parent of the neighbor to the currentMode

                    if (!openSet.Contains(neighbour))
                    {
                        openSet.Add(neighbour);
                    }
                }
            }
        }
    }

    void RetracePath(Node startNode, Node endNode) //retracing path to get form the start node and to the endnode
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode; //trace this part backwards 

        while (currentNode != startNode) //retracing path untill we reach starting node
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse();

        grid.path = path;
    }

    int GetDistance(Node nodeA, Node nodeB) // this counts the x axis and the y axis, takes the lowest number and gives how many daiagonal moves it will take to make it either horizontal or vertical in line with the end node
    {
        //setting the equation for diagonal movement 14y + 10(x-y)
        int distX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int distY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (distX > distY)
        {
            return 14 * distY + 10 * (distX - distY);
        }
        return 14 * distX + 10 * (distY - distX);
    }
}
