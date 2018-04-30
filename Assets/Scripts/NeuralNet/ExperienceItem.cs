using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Experience {

    private double[] stateRepresentation;
    private int actionTaken;
    private float result;

    public double[] getStateRepresentation()
    {
        return stateRepresentation;
    }

    public int getActionTaken()
    {
        return actionTaken;
    }

    public float getResult()
    {
        if (result == -1.0f)
        {
            throw new System.Exception("Attempt to read result before it has been set");
        }
        else
        {
            return result;
        }
    }

    public void SetResult(float result)
    {
        this.result = result;
    }

    public Experience(double[] stateRepresentation, int actionTaken)
    {
        this.stateRepresentation = stateRepresentation;
        this.actionTaken = actionTaken;
        this.result = -1.0f; // I'm using -1 as a default value to signify that the result of the game hasn't been stored yet
    }
}
