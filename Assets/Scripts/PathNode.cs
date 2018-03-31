using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathNode
{
    private PathNode parent;
    private float cost;
    private float heuristic;
    private Vector2 position;

    public PathNode(PathNode par, float c, Vector2 pos)
    {
        parent = par;
        cost = c;
        position = pos;
    }

    public float getTotalCost()
    {
        return cost + heuristic;
    }

    public Vector2 getPosition()
    {
        return position;
    }

    public float getCost()
    {
        return cost;
    }

    public void setHueristic(float h)
    {
        heuristic = h;
    }

    public PathNode getParent()
    {
        return parent;
    }
}
