using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    //Creates the pathfinding array
    private void fillArray()
    {
        float lowX = -19;
        float lowY = -10;
        float maxX = 19;
        float maxY = 10;

        xStep = (maxX - lowX) / (float)numX;
        yStep = (maxY - lowY) / (float)numY;

        float xPos = lowX;
        float yPos = lowY;

        for (int x = 0; x < numX; x++)
        {
            
            for(int y = 0; y < numY; y ++)
            {
                RaycastHit2D hit = Physics2D.Raycast(new Vector2(xPos+xStep/2, yPos+yStep/2), Vector2.zero, 0.01f, 1<<LayerMask.NameToLayer("Terrain"));

                if (hit)
                {
                    pathfindingArray[x][y] = false;
                }
                else
                {
                    pathfindingArray[x][y] = true;
                }
                yPos += yStep;
            }
            yPos = lowY;
            xPos += xStep;
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

    private float heuristic(int startX, int startY, int goalX, int goalY)
    {
        //Calculates the manhattan distance
        float manhattanDist = Mathf.Abs(goalX - startX) + Mathf.Abs(goalY - startY);

        return manhattanDist;
    }
    

    public Vector2[] getPath(Vector2 start, Vector2 goal)
    {
        //Convert the start and goal into array positions
        Vector2 startArrayVals = vectorToArray(start.x, start.y);
        Vector2 goalArrayVals = vectorToArray(goal.x, goal.y);


        //Initialise the queue and currentnode
        PriorityQueue<PathNode> priorityQueue = new PriorityQueue<PathNode>();
        PathNode currentNode = new PathNode(null, 0, start);
        float h = heuristic((int)startArrayVals.x, (int)startArrayVals.y, (int)goalArrayVals.x, (int)goalArrayVals.y);
        currentNode.setHueristic(h);
        priorityQueue.push(currentNode);
        List<PathNode> visitedList = new List<PathNode>();


        while (vectorToArray(currentNode.getPosition().x, currentNode.getPosition().y) != goalArrayVals)
        {
            
            bool visitedCheck = true;

            //Pop nodes from the queue until a node that hasnt been visited is found.
            while(visitedCheck)
            {
                visitedCheck = false;
                currentNode = priorityQueue.pop();
                for(int i = 0; i < visitedList.Count; i++)
                {
                    if(visitedList[i].getPosition() == currentNode.getPosition())
                    {
                        visitedCheck = true;
                    }
                }
            }

            //Add the popped node to the visited list
            visitedList.Add(currentNode);

            //Get successors of current node
            PathNode[] successors = getSuccessors(currentNode);

            //Iterate through each successor
            for(int i = 0; i < successors.Length; i++)
            {
                bool visited = false;

                //Check if successor has been visited
                for(int j = 0; j < visitedList.Count; j++)
                {
                    if(visitedList[j].getPosition() == successors[i].getPosition())
                    {
                        visited = true;
                    }
                }

                //If the node has not been visited, push it to the queue
                if(!visited)
                {
                    Vector2 arrayNums = vectorToArray(successors[i].getPosition().x, successors[i].getPosition().y);

                    h = heuristic((int)arrayNums.x, (int)arrayNums.y, (int)goalArrayVals.x, (int)goalArrayVals.y);
                    successors[i].setHueristic(h);
                    priorityQueue.push(successors[i]);
                }
            }
        }

        //Backtrack from the goal node
        Vector2[] calculatedPath = backtrack(currentNode);

        return calculatedPath;
    }

    //Backtracks from a node to find all the moves to reach that node from the player
    private Vector2[] backtrack(PathNode startNode)
    {
        List<Vector2> tempList = new List<Vector2>();

        PathNode currentNode = startNode;
        tempList.Insert(0, currentNode.getPosition());

        while(currentNode.getParent() != null)
        {
            currentNode = currentNode.getParent();
            tempList.Insert(0, currentNode.getPosition());
        }

        return tempList.ToArray();
    }

    //Converts an array position into a Vector 2 of the world space position.
    private Vector2 arrayToVector(int x, int y)
    {
        float xVal = -19f + x * xStep + xStep / 2;
        float yVal = -10f + y * yStep + yStep / 2;

        return new Vector2(xVal, yVal);
    }

    //Converts a world space position into a Vector2 containing the array position of that world space.
    private Vector2 vectorToArray(float xPos, float yPos)
    {
        float checkX = -19f;
        int x = 0;
        float checkY = -10f;
        int y = 0;

        while (checkX + xStep < xPos)
        {
            x++;
            checkX += xStep;
        }

        while (checkY + yStep < yPos)
        {
            y++;
            checkY += yStep;
        }

        return new Vector2(x, y);
    }


    //Returns the successors of the node passed in.
    private PathNode [] getSuccessors(PathNode par)
    {
        Vector2 arrayPos = vectorToArray(par.getPosition().x, par.getPosition().y);

        List <PathNode> tempList = new List<PathNode>();

        //Check left
        if(arrayPos.x > 0)
        {
            if(pathfindingArray[(int)arrayPos.x-1][(int)arrayPos.y])
            {
                PathNode tempNode = new PathNode(par, par.getCost() + 1, arrayToVector((int)arrayPos.x-1,(int)arrayPos.y));
                tempList.Add(tempNode);
            }
        }

        //Check right
        if (arrayPos.x < numX -1 )
        {
            if (pathfindingArray[(int)arrayPos.x + 1][(int)arrayPos.y])
            {
                PathNode tempNode = new PathNode(par, par.getCost() + 1, arrayToVector((int)arrayPos.x + 1, (int)arrayPos.y));
                tempList.Add(tempNode);
            }
        }

        //Check up
        if (arrayPos.y < numY - 1)
        {
            if (pathfindingArray[(int)arrayPos.x][(int)arrayPos.y + 1])
            {
                PathNode tempNode = new PathNode(par, par.getCost() + 1, arrayToVector((int)arrayPos.x, (int)arrayPos.y + 1));
                tempList.Add(tempNode);
            }
        }

        //Check down
        if (arrayPos.y > 0)
        {
            if (pathfindingArray[(int)arrayPos.x][(int)arrayPos.y - 1])
            {
                PathNode tempNode = new PathNode(par, par.getCost() + 1, arrayToVector((int)arrayPos.x, (int)arrayPos.y - 1));
                tempList.Add(tempNode);
            }
        }

        //Convert to an array and return
        return tempList.ToArray();

    }

    

}
