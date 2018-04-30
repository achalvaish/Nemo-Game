using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ActivationLinear : ActivationFunction
{
    override public double calculate(double x)
    {
        return x;
    }

    override public double derivative(double x, double fOfx)
    {
        return 1.0;
    }
}
