using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PriorityQueue<T> {

    private List<PathNode> list;

    public PriorityQueue()
    {
        list = new List<PathNode>();
    }

    public void push(PathNode node)
    {
        list.Add(node);
        sort();
    }

    public PathNode pop()
    {
        if (list.Count > 0)
        {
            PathNode temp = list[0];
            list.Remove(list[0]);
            sort();
            return temp;
        }
        return null;
    }

    public void sort()
    {
        bool bubbleSorting = true;


        while(bubbleSorting)
        {
            bubbleSorting = false;
            for (int i = 0; i < list.Count -1; i++)
            {
                if(list[i].getTotalCost() > list[i+1].getTotalCost())
                {
                    PathNode tempNode = list[i];
                    list[i] = list[i + 1];
                    list[i + 1] = tempNode;
                    bubbleSorting = true;
                }
            }
        }
    }

}
