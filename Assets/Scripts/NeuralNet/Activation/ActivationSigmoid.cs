using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ActivationSigmoid : ActivationFunction
{
    override public double calculate(double x)
    {
        return 1.0f / (1.0f + Mathf.Exp(-(float)x));
    }

    override public double derivative(double x, double fOfx)
    {
        return fOfx * (1.0 - fOfx);
    }
}
