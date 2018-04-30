using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NeuralNet
{
    [SerializeField]
    public int[] layerSizes;

    // My terminology is a bit different to Encog's. If a layer contains a bias neuron, it means that the NEXT layer is biased
    public bool[] layerContainsBiasNeuron;

    public ActivationFunction[] activationFuncs;

    public double[][][] networkWeights;

    // Activations from the last forward pass
    public double[][][] activations;

    // Gradients
    public double[][][] gradWeights;
    public double[][] gradUnits;

    public int GetNumLayers()
    {
        // Result is inclusive of the input and output layers
        return layerSizes.Length;
    }

    public int GetNumOutputNeurons()
    {
        return networkWeights[networkWeights.Length - 1][0].Length;
    }

    public double GetWeight(int fromLayer, int fromIdx, int toIdx)
    {
        return networkWeights[fromLayer][fromIdx][toIdx];
    }

    public NeuralNet()
    {
        // Dummy just for serialization
    }

    public NeuralNet(int[] layerSizes, bool[] layerContainsBiasNeuron, ActivationFunction[] activationFunctions)
    {
        this.layerSizes = layerSizes;

        // Append an extra 'false' to the bias info to store the fact
        // that the output layer does not contain a bias neuron
        this.layerContainsBiasNeuron = new bool[layerContainsBiasNeuron.Length + 1];
        for (int i = 0; i < layerContainsBiasNeuron.Length; i++)
        {
            this.layerContainsBiasNeuron[i] = layerContainsBiasNeuron[i];
        }
        this.layerContainsBiasNeuron[this.layerContainsBiasNeuron.Length - 1] = false;

        this.activationFuncs = activationFunctions;

        this.networkWeights = new double[layerSizes.Length - 1][][];

        for (int fromLayer = 0; fromLayer < (layerSizes.Length - 1); fromLayer++)
        {
            int fromCount = layerSizes[fromLayer] + (layerContainsBiasNeuron[fromLayer] ? 1 : 0);
            int toCount = layerSizes[fromLayer + 1];

            networkWeights[fromLayer] = new double[fromCount][];

            for (int fromIdx = 0; fromIdx < fromCount; fromIdx++)
            {
                networkWeights[fromLayer][fromIdx] = new double[toCount];

                // Weight initialisation
                for (int toIdx = 0; toIdx < toCount; toIdx++)
                {
                    double randomWeight = Random.Range(-1.0f, 1.0f);

                    // Divide by the square root of the fan-in
                    randomWeight /= Mathf.Sqrt(fromCount);

                    networkWeights[fromLayer][fromIdx][toIdx] = randomWeight;
                }
            }
        }

        InitialiseGradWeights();
        InitialiseGradUnits();
        InitialiseActivations();
    }

    private void InitialiseGradWeights()
    {
        gradWeights = new double[networkWeights.Length][][];

        for (int i = 0; i < networkWeights.Length; i++)
        {
            gradWeights[i] = new double[networkWeights[i].Length][];

            for (int j = 0; j < networkWeights[i].Length; j++)
            {
                gradWeights[i][j] = new double[networkWeights[i][j].Length];
            }
        }
    }

    private void InitialiseActivations()
    {
        activations = new double[layerSizes.Length][][];

        for (int i = 0; i < layerSizes.Length; i++)
        {
            int numNeuronsIncludingBias = layerSizes[i] + (layerContainsBiasNeuron[i] ? 1 : 0);

            activations[i] = new double[numNeuronsIncludingBias][];
            for (int j = 0; j < numNeuronsIncludingBias; j++)
            {
                activations[i][j] = new double[2]; // Unsquashed, squashed
            }
        }
    }

    private void InitialiseGradUnits()
    {
        gradUnits = new double[layerSizes.Length][];

        for (int i = 0; i < layerSizes.Length; i++)
        {
            int numNeuronsIncludingBias = layerSizes[i] + (layerContainsBiasNeuron[i] ? 1 : 0);
            gradUnits[i] = new double[numNeuronsIncludingBias];
        }
    }

    public double[] Forward(double[] input)
    {
        int numLayers = GetNumLayers();

        if (input.Length != layerSizes[0])
        {
            Debug.LogError("ERROR: Input length to network = " + input.Length + ", expecting input of size = " + layerSizes[0]);
            return null;
        }

        // The activations for layer 0 are just the input (and potentially a bias)
        for (int i = 0; i < input.Length; i++)
        {
            activations[0][i][0] = input[i]; // Unsquashed
            activations[0][i][1] = input[i]; // Squashed (squashing is irrelevant here)
        }

        // Set bias activation if applicable
        if (layerContainsBiasNeuron[0])
        {
            activations[0][activations[0].Length - 1][0] = 1.0; // Unsquashed
            activations[0][activations[0].Length - 1][1] = 1.0; // Squashed (squashing is irrelevant here)
        }

        for (int toLayer = 1; toLayer < numLayers; toLayer++)
        {
            int fromLayer = toLayer - 1;
            int numNeuronsThisLayerExludingBias = networkWeights[fromLayer][0].Length;
            int bias = layerContainsBiasNeuron[toLayer] ? 1 : 0;

            for (int toIdx = 0; toIdx < numNeuronsThisLayerExludingBias; toIdx++)
            {
                // Clear old memory
                activations[toLayer][toIdx][0] = 0.0;

                for (int fromIdx = 0; fromIdx < networkWeights[fromLayer].Length; fromIdx++)
                {
                    // Default to a value of 1 (for when there's a bias neuron)
                    double fromValue = 1.0;

                    if (fromIdx < activations[fromLayer].Length)
                    {
                        fromValue = activations[fromLayer][fromIdx][1]; // Use the squashed value from the previous layer
                    }

                    activations[toLayer][toIdx][0] += fromValue * networkWeights[fromLayer][fromIdx][toIdx];
                }
            }

            for (int toIdx = 0; toIdx < numNeuronsThisLayerExludingBias; toIdx++)
            {
                // Calculate squashed value
                activations[toLayer][toIdx][1] = activationFuncs[fromLayer].calculate(activations[toLayer][toIdx][0]);
            }

            // Set bias activations to 1
            if (bias > 0)
            {
                activations[toLayer][activations[toLayer].Length - 1][0] = 1.0; // Unsquashed
                activations[toLayer][activations[toLayer].Length - 1][1] = 1.0; // Squashed (squashing is irrelevant here)
            }
        }

        double[][] outputActivations = activations[activations.Length - 1];

        double[] result = new double[outputActivations.Length];
        for (int i = 0; i < outputActivations.Length; i++)
        {
            // Return squashed values
            result[i] = outputActivations[i][1];
        }

        return result;
    }

    public void Train(double[][] input, double?[][] target, double learningRate, double momentum)
    {
        int numLayers = GetNumLayers();
        int numTrainingSamples = input.Length;

        for (int trainingSample = 0; trainingSample < numTrainingSamples; trainingSample++)
        {
            double[] netOutput = Forward(input[trainingSample]);

            for (int i = 0; i < netOutput.Length; i++)
            {
                if (target[trainingSample][i] != null)
                {
                    gradUnits[numLayers - 1][i] = (double)target[trainingSample][i] - netOutput[i];
                }
                else
                {
                    gradUnits[numLayers - 1][i] = 0.0;
                }
            }

            for (int fromLayer = numLayers - 2; fromLayer >= 0; fromLayer--)
            {
                int toLayer = fromLayer + 1;

                int fromCount = networkWeights[fromLayer].Length;

                for (int fromIdx = 0; fromIdx < fromCount; fromIdx++)
                {
                    // Clear old memory
                    gradUnits[fromLayer][fromIdx] = 0.0;

                    int toCount = networkWeights[fromLayer][fromIdx].Length;

                    for (int toIdx = 0; toIdx < toCount; toIdx++)
                    {
                        // Decay old grad weights according to momentum
                        if (trainingSample == 0)
                        {
                            gradWeights[fromLayer][fromIdx][toIdx] = momentum * gradWeights[fromLayer][fromIdx][toIdx];
                        }

                        double toActivationUnsquashed = activations[toLayer][toIdx][0];
                        double toActivationSquashed = activations[toLayer][toIdx][1];

                        double derivative = activationFuncs[fromLayer].derivative(toActivationUnsquashed, toActivationSquashed);

                        gradUnits[fromLayer][fromIdx] += gradUnits[toLayer][toIdx] * networkWeights[fromLayer][fromIdx][toIdx] * derivative;
                        gradWeights[fromLayer][fromIdx][toIdx] += (1.0f - momentum) * gradUnits[toLayer][toIdx] * activations[fromLayer][fromIdx][1] * derivative;
                    }
                }
            }
        }

        for (int i = 0; i < networkWeights.Length; i++)
        {
            for (int j = 0; j < networkWeights[i].Length; j++)
            {
                for (int k = 0; k < networkWeights[i][j].Length; k++)
                {
                    networkWeights[i][j][k] += learningRate * (1.0 / numTrainingSamples) * gradWeights[i][j][k];
                }
            }
        }
    }
}