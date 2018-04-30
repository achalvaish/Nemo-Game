using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class ActivationFunction
{
    public abstract double calculate(double x);

    public abstract double derivative(double x, double fOfx);
}
