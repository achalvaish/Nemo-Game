using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class GridEnvironment : Environment
{

    public List<GameObject> actorObjs;
    public string[] players;
    public GameObject visualAgent;
    int numGoals;
    int gridSize;
    int[] objectPositions;
    float episodeReward;

    void Start()
    {
        maxSteps = 100;
        waitTime = 0.001f;
        BeginNewGame();
    }

    // Restarts the learning process with a new grid
    public void BeginNewGame()
    {
        numGoals = 1;
        gridSize = 15;

        SetUp();
        agent = new MotherFishQ();
        agent.SendParameters(envParameters);
        Reset();

    }

    // Set up the grid
    public override void SetUp()
    {
        envParameters = new EnvironmentParameters()
        {
            observation_size = 0,
            state_size = gridSize * gridSize,
            action_descriptions = new List<string>() { "Up", "Down", "Left", "Right" },
            action_size = 4,
            env_name = "GridWorld",
            action_space_type = "discrete",
            state_space_type = "discrete",
            num_agents = 1
        };

        List<string> playersList = new List<string>();
        actorObjs = new List<GameObject>();
        playersList.Add("agent");

        for (int i = 0; i < numGoals; i++)
        {
            playersList.Add("goal");
        }
        players = playersList.ToArray();
    }

    // Update is called once per frame
    void Update()
    {
        // waitTime = 1.0f - GameObject.Find("Slider").GetComponent<Slider>().value;
        RunMdp();
    }

    // Gets the agent's current position and transforms it into a discrete integer state
    public override List<float> collectState()
    {
        List<float> state = new List<float>();
        float point = (gridSize * agent.transform.position.x) + agent.transform.position.z;
        state.Add(point);
        /*foreach (GameObject actor in actorObjs)
        {
            if (actor.tag == "agent")
            {
                float point = (gridSize * actor.transform.position.x) + actor.transform.position.z;
                state.Add(point);
            }
        }*/
        return state;
    }

    // Resets the episode by placing the objects in their original positions
    public override void Reset()
    {
        base.Reset();

        foreach (GameObject actor in actorObjs)
        {
            DestroyImmediate(actor);
        }
        actorObjs = new List<GameObject>();

        for (int i = 0; i < players.Length; i++)
        {
            int x = (objectPositions[i]) / gridSize;
            int y = (objectPositions[i]) % gridSize;
            GameObject actorObj = (GameObject)GameObject.Instantiate(Resources.Load(players[i]));
            actorObj.transform.position = new Vector3(x, 0.0f, y);
            actorObj.name = players[i];
            actorObjs.Add(actorObj);
            if (players[i] == "agent")
            {
                visualAgent = actorObj;
            }
        }
        episodeReward = 0;
        EndReset();
    }

    /// <summary>
    /// Allows the agent to take actions, and set rewards accordingly.
    /// </summary>
    /// <param name="action">Action.</param>
    public override void MiddleStep(int action)
    {
        reward = -0.05f;
        // 0 - Forward, 1 - Backward, 2 - Left, 3 - Right
        if (action == 3)
        {
            Collider[] blockTest = Physics.OverlapBox(new Vector3(visualAgent.transform.position.x + 1, 0, visualAgent.transform.position.z), new Vector3(0.3f, 0.3f, 0.3f));
            if (blockTest.Where(col => col.gameObject.tag == "wall").ToArray().Length == 0)
            {
                visualAgent.transform.position = new Vector3(visualAgent.transform.position.x + 1, 0, visualAgent.transform.position.z);
            }
        }

        if (action == 2)
        {
            Collider[] blockTest = Physics.OverlapBox(new Vector3(visualAgent.transform.position.x - 1, 0, visualAgent.transform.position.z), new Vector3(0.3f, 0.3f, 0.3f));
            if (blockTest.Where(col => col.gameObject.tag == "wall").ToArray().Length == 0)
            {
                visualAgent.transform.position = new Vector3(visualAgent.transform.position.x - 1, 0, visualAgent.transform.position.z);
            }
        }

        if (action == 0)
        {
            Collider[] blockTest = Physics.OverlapBox(new Vector3(visualAgent.transform.position.x, 0, visualAgent.transform.position.z + 1), new Vector3(0.3f, 0.3f, 0.3f));
            if (blockTest.Where(col => col.gameObject.tag == "wall").ToArray().Length == 0)
            {
                visualAgent.transform.position = new Vector3(visualAgent.transform.position.x, 0, visualAgent.transform.position.z + 1);
            }
        }

        if (action == 1)
        {
            Collider[] blockTest = Physics.OverlapBox(new Vector3(visualAgent.transform.position.x, 0, visualAgent.transform.position.z - 1), new Vector3(0.3f, 0.3f, 0.3f));
            if (blockTest.Where(col => col.gameObject.tag == "wall").ToArray().Length == 0)
            {
                visualAgent.transform.position = new Vector3(visualAgent.transform.position.x, 0, visualAgent.transform.position.z - 1);
            }
        }

        Collider[] hitObjects = Physics.OverlapBox(visualAgent.transform.position, new Vector3(0.3f, 0.3f, 0.3f));
        if (hitObjects.Where(col => col.gameObject.tag == "goal").ToArray().Length == 1)
        {
            reward = 1;
            done = true;
        }
        if (hitObjects.Where(col => col.gameObject.tag == "pit").ToArray().Length == 1)
        {
            reward = -1;
            done = true;
        }

        episodeReward += reward;
        GameObject.Find("RTxt").GetComponent<Text>().text = "Episode Reward: " + episodeReward.ToString("F2");

    }
}
