using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentParameters
{
    public int state_size { get; set; }
    public int action_size { get; set; }
    public int observation_size { get; set; }
    public List<string> action_descriptions { get; set; }
    public string env_name { get; set; }
    public string action_space_type { get; set; }
    public string state_space_type { get; set; }
    public int num_agents { get; set; }
}

public abstract class Environment : MonoBehaviour
{
    public float reward;
    public bool done;
    //public int maxSteps;
    //public int currentStep;
    public bool begun;
   // public bool acceptingSteps;

    public Agent agent;
    public int comPort;
    public int frameToSkip;
    public int framesSinceAction;
    public string currentPythonCommand;
    public bool skippingFrames;
    public float[] actions;
    //public float waitTime;
    public int episodeCount;
    
    public EnvironmentParameters envParameters;

    public virtual void SetUp()
    {
        envParameters = new EnvironmentParameters()
        {
            observation_size = 0,
            state_size = 0,
            action_descriptions = new List<string>(),
            action_size = 0,
            env_name = "Null",
            action_space_type = "discrete",
            state_space_type = "discrete",
            num_agents = 1
        };
        begun = false;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public virtual List<float> collectState()
    {
        List<float> state = new List<float>();
        return state;
    }

    public virtual void Step()
    {
        reward = 0;
        actions = agent.GetAction();
        framesSinceAction = 0;

        int sendAction = Mathf.FloorToInt(actions[0]);
        MiddleStep(sendAction);
        EndStep();
    }

    public virtual void MiddleStep(int action)
    {

    }

    public virtual void MiddleStep(float[] action)
    {

    }

    public virtual void EndStep()
    {
        agent.SendState(collectState(), reward, done);
        skippingFrames = false;
    }

    public virtual void Reset()
    {
        reward = 0;
        // Debug.Log("Episode count " + episodeCount);
        episodeCount++;
        done = false;
    }

    public virtual void EndReset()
    {
        agent.SendState(collectState(), reward, done);
        skippingFrames = false;
        begun = true;
        framesSinceAction = 0;
    }

    public virtual void RunMdp()
    {
        if (done == false)
        {
            Step();
        }
        else
        {
            // WriteResultsToFile();
            Reset();
            Debug.Log("End episode");
        }
    }

    



}
