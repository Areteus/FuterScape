using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public LayerMask ObsticleMask; 
    public Vector2 gridWorldSize; //difines area of world points that the grid is going to cover 
    public float nodeRaidus; //how much space each node covers 
    Node[,] grid; //making two diemensional array 

    float nodeDiameter;
    int gridSizeX, gridSizeY;
    void Start()
    {
        nodeDiameter = nodeRaidus * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);  // how many nodes can fit into worldSize.x 
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);  // how many nodes can fit into worldSize.y 
        CreateGrid();
    }

    void CreateGrid()
    {
        grid = new Node[gridSizeX, gridSizeY]; //new 2d array of nodes with the size of both GridSize X and Y
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y/2; //vector3.right * girdworldSizeX gives the left edge of the world and vector3.forward gives the worlds bottom left
        //loop through all position nodes will be in for a collision check if they are and obsticle or not
        for (int x = 0; x < gridSizeX; x++) // increases increments of node diameter 
        {
            for (int y = 0; y < gridSizeX; y++) // increases increments of nodeRaidus
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRaidus) + Vector3.forward * (y * nodeDiameter + nodeRaidus);
                bool walkable = !(Physics.CheckSphere(worldPoint, nodeRaidus, ObsticleMask)); //if collision returns true walkable will be false
                //create new node
                grid[x, y] = new Node(walkable, worldPoint, x, y); //populates grid with nodes
            }
        }

    }

    public List<Node> GetNeighbours(Node node) //returns a list of nodes 
    {
        List<Node> neighbours = new List<Node>();

        for (int x = -1; x <= 1; x++) //seaches in a 3x3 block around the node
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue; //skips itteration 

                int checkX = node.gridX + x; 
                int checkY = node.gridY + y; 

                if (checkX >= 0 && checkX < gridSizeX && checkY >=0 && checkY < gridSizeY) // checks if its inside the grid 
                {
                    neighbours.Add(grid[checkX, checkY]); 
                }
            }
        }

        return neighbours; 
    }

    public Node NodeFromWorldPoint(Vector3 worldPosition) // returns a node
    {
        float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x; // if the worldPosition on the x axis is zero we add half the girdworldsize
        float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y; // if the worldPosition on the y axis is zero we add half the girdworldsize
        percentX = Mathf.Clamp01(percentX); //sets to 0 to 1
        percentY = Mathf.Clamp01(percentY);  //this is for if the charcter or world position outside the of the grid 

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX); //this is to insure we arnt outside of the array 
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);

        return grid[x, y]; //returns node from the grid 
    }

    public List<Node> path;
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y)); // y is placed on z axis cause it represents z in 3d space
        if(grid !=null)
        {
            foreach (Node n in grid) //foreach node in the gird 
            {
                Gizmos.color = (n.walkable) ? Color.white : Color.red; //if its walkable then color is white otherwise its red
                if (path!=null)
                {
                    if (path.Contains(n))
                    {
                        Gizmos.color = Color.black;
                    }
                }
                Gizmos.DrawCube(n.worldPos, Vector3.one * (nodeDiameter - .1f)); 
            }
        }
    }


}
