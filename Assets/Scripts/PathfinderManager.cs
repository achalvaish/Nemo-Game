using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pathNode
{
    private pathNode parent;
    private float cost;
    private Transform position;

    public pathNode(pathNode par, float c, Transform pos)
    {
        parent = par;
        cost = c;
        position = pos;
    }
}

public class PathfinderManager : MonoBehaviour {


    public int numX, numY;
    private float xStep, yStep;

    private bool [][] pathfindingArray;

	// Use this for initialization
	void Start () {
        pathfindingArray = new bool[numX][];

        for(int i = 0; i < numX; i++)
        {
            pathfindingArray[i] = new bool[numY];
        }

        fillArray();

        //printArray();
	}


    private void fillArray()
    {
        float lowX = -9;
        float lowY = -5;
        float maxX = 9;
        float maxY = 5;

        xStep = (maxX - lowX) / (float)numX;
        yStep = (maxY - lowY) / (float)numY;

        int xCount = 0;
        int yCount = 0;

        for (float x = lowX; x < maxX; x += xStep)
        {
            for(float y = lowY; y < maxY; y += yStep)
            {
                RaycastHit2D hit = Physics2D.Raycast(new Vector2(x+xStep/2, y+yStep/2), Vector2.zero, 0.01f, 1<<LayerMask.NameToLayer("Terrain"));

                if(hit)
                {
                    pathfindingArray[xCount][yCount] = false;
                }
                else
                {
                    pathfindingArray[xCount][yCount] = true;
                }
                yCount++;
            }
            yCount = 0;
            xCount++;
        }


    }

    private void printArray()
    {
        string debugString = "";
        for(int y = 0; y < numY; y++)
        {
            for(int x = 0; x < numX; x++)
            {
                if(pathfindingArray[x][y])
                {
                    debugString += 'T';
                }
                else
                {
                    debugString += 'F';
                }
            }
            debugString += '\n';
        }
        Debug.Log(debugString);
    }

   /* private float heurstic(int startX, int startY, int goalX, int goalY)
    {

    }


    public Transform[] getPath(Transform start, Transform goal)
    {
        //Convert the start and goal into array positions
        float startX = -9f;
        int x = 0;
        float startY = -5f;
        int y = 0;

        float gX = -9f;
        int goalX = 0;
        float gY = -5f;
        int goalY = 0;

        while(startX + xStep < start.position.x)
        {
            x++;
            startX += xStep;
        }

        while(startY + yStep < start.position.y)
        {
            y++;
            startY += yStep;
        }

        while (gX + xStep < start.position.x)
        {
            goalX++;
            gX += xStep;
        }

        while (gY + yStep < start.position.y)
        {
            goalY++;
            gY += yStep;
        }


        //Initialise the queue and currentnode
        Queue<pathNode> q = new Queue<pathNode>();
        //pathNode currentNode



    }

    */

}
