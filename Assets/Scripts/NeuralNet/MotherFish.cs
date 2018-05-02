using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotherFish : MonoBehaviour {

    public float startEpsilon;
    public float endEpsilon;
    public float endEpsilonTime;

    public bool isDead;
    public bool isSafe;

    public double currentActionValue; // Just for debugging

    public float epsilon; // Made public for debugging, shouldn't actually be set though

    private Vector2 action;

    // Floating point variable to store the player's movement speed.
    public float speed;
    public float maxYposition;

    // Store a reference to the Rigidbody2D component required to use 2D Physics.
    private Rigidbody2D rb2d;

    public int numFish;

    // Use this for initialization
    void Start()
    {
        // Get and store a reference to the Rigidbody2D component
        // so that we can access it.
        rb2d = GetComponent<Rigidbody2D>();
        isDead = false;
        isSafe = false;
        numFish = 0;

        action = new Vector2(0.0f, 0.0f);
    }

    public int UpdateAction(NeuralNet net, double[] stateRepresentation, int framesSinceLearningCommenced)
    {
        epsilon = Mathf.Lerp(startEpsilon, endEpsilon, Mathf.Clamp(framesSinceLearningCommenced / endEpsilonTime, 0.0f, 1.0f));

        int bestAction = -1;

        if (Random.Range(0.0f, 1.0f) < epsilon)
        {
            bestAction = Random.Range(0, 9);
        }
        else
        {
            double[] netOutput = net.Forward(stateRepresentation);
            double maxOutput = double.MinValue;

            for (int i = 0; i < netOutput.Length; i++)
            {
                if (netOutput[i] > maxOutput)
                {
                    bestAction = i;
                    maxOutput = netOutput[i];
                }
            }

            currentActionValue = maxOutput;
        }

        switch(bestAction )
        {
            // Do nothing
            case 0:
                action = new Vector2(0, 0);
                break;
            //Up
            case 1:
                action = new Vector2(0, 1);
                break;
            //Up-Right
            case 2:
                action = new Vector2(1, 1).normalized;
                break;
            //Right
            case 3:
                action = new Vector2(1, 0);
                break;
            //Down-Right
            case 4:
                action = new Vector2(1, -1).normalized;
                break;
            //Down
            case 5:
                action = new Vector2(0, -1);
                break;
            //Down-Left
            case 6:
                action = new Vector2(-1, -1).normalized;
                break;
            //Left
            case 7:
                action = new Vector2(-1, 0);
                break;
            //Up-Left
            case 8:
                action = new Vector2(-1, 1).normalized;
                break;
        }

        return bestAction;
    }

    void FixedUpdate()
    {
        rb2d.velocity = action * speed;
    }

    public void reset()
    {
        float Xpos = Random.Range(1, 13);
        float ypos = Random.Range(1, 13);
        isDead = false;
        isSafe = false;
        numFish = 0;

        this.transform.position = new Vector2(Xpos, ypos);
    }

    public void die()
    {
        isDead = true;
        numFish = 0;
    }

    public void safe()
    {
        isSafe = true;
    }
}
