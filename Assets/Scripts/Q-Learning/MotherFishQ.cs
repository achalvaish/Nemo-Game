﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class MotherFishQ : Agent {

    public float[][] q_table;   // The matrix containing the values estimates.
    float learning_rate = 0.5f; // The rate at which to update the value estimates given a reward.
    int action = -1;
    float gamma = 0.99f; // Discount factor for calculating Q-target.
    float e = 1; // Initial epsilon value for random action selection.
    float eMin = 0.1f; // Lower bound of epsilon.
    int annealingSteps = 2000; // Number of steps to lower e to eMin.
    int lastState;
    GridEnvironment gridEnvironment;

    public override void SendParameters(EnvironmentParameters env)
    {
        q_table = new float[env.state_size][];
        action = 0;
        for (int i = 0; i < env.state_size; i++)
        {
            q_table[i] = new float[env.action_size];
            for (int j = 0; j < env.action_size; j++)
            {
                q_table[i][j] = 0.0f;
            }
        }
    }

    // Picks an action to take from its current state
    // Returns the action choosen by the agent's policy
    public override float[] GetAction()
    {
        action = q_table[lastState].ToList().IndexOf(q_table[lastState].Max());
        if (Random.Range(0f, 1f) < e) { action = Random.Range(0, 3); }
        if (e > eMin) { e = e - ((1f - eMin) / (float)annealingSteps); }
        float currentQ = q_table[lastState][action];
        return new float[1] { action };
    }

    // Gets the values stored within the Q table
    public override float[] GetValue()
    {
        float[] value_table = new float[q_table.Length];
        for (int i = 0; i < q_table.Length; i++)
        {
            value_table[i] = q_table[i].Average();
        }
        return value_table;
    }

    // Updates the value estimate matrix given a new experience (state, action, reward)
    // state = the environment state the experience happened in
    // reward = the reward recieved by the agent from the environment for it's action
    // done = whether the episode has ended
    public override void SendState(List<float> state, float reward, bool done)
    {
        int nextState = Mathf.FloorToInt(state.First());
        if (action != -1)
        {
            if (done == true)
            {
                q_table[lastState][action] += learning_rate * (reward - q_table[lastState][action]);
            }
            else
            {
                q_table[lastState][action] += learning_rate * (reward + gamma * q_table[nextState].Max() - q_table[lastState][action]);
            }
        }
        lastState = nextState;
    }
}
