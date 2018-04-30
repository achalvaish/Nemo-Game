using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ActivationRELU : ActivationFunction
{
    override public double calculate(double x)
    {
        return Mathf.Max(0.0f, (float)x);
    }


    override public double derivative(double x, double fOfx)
    {
        if (x > 0)
        {
            return 1.0;
        }
        else
        {
            return 0.0;
        }
    }
}
