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
    int numFish;
    int numSharks;
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
        numFish = 3;
        numSharks = 1;
        gridSize = 13;

        if (actorObjs != null)
        {
            foreach (GameObject actor in actorObjs)
            {
                DestroyImmediate(actor);
            }
        }
        
        SetUp();
        agent = GameObject.FindGameObjectWithTag("agent").GetComponent<MotherFishQ>();
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
            action_descriptions = new List<string>() { "Up", "Up-Right", "Right", "Down-Right", "Down", "Down-Left", "Left", "Up-Left", "Do Nothing" },
            action_size = 9,
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
        for (int i = 0; i < numFish; i++)
        {
            playersList.Add("fish");
        }
        for (int i = 0; i < numSharks; i++)
        {
            playersList.Add("shark");
        }

        players = playersList.ToArray();

    }

    // Update is called once per frame
    void Update()
    {
        RunMdp();
    }

    // Gets the agent's current position and transforms it into a discrete integer state
    public override List<float> collectState()
    {
        List<float> state = new List<float>();
        foreach (GameObject actor in actorObjs)
        {
            if (actor.tag == "agent")
            {
                float point = (gridSize * actor.transform.position.x) + actor.transform.position.z;
                state.Add(point);
            }
        }
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

        /*for (int i = 0; i < players.Length; i++)
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
        }*/
        episodeReward = 0;
        EndReset();
    }

    // Allows the agent to take actions and set rewards accordingly
    public override void MiddleStep(int action)
    {
        reward = -0.05f;
        // 0 - Up, 1 - Up-Right, 2 - Right, 3 - Down-Right, 4 - Down, 5 - Down-Left, 6 - Left, 7 - Up-Left, 8 - Do Nothing
        
        // Up
        if (action == 0)
        {
            visualAgent.transform.position = new Vector2(visualAgent.transform.position.x, visualAgent.transform.position.y + 1);
        }

        // Up-Right
        if (action == 1)
        {
            visualAgent.transform.position = new Vector2(visualAgent.transform.position.x + 1, visualAgent.transform.position.y + 1).normalized;
        }

        // Right
        if (action == 2)
        {
            visualAgent.transform.position = new Vector2(visualAgent.transform.position.x + 1, visualAgent.transform.position.y);
        }

        // Down-Right
        if (action == 3)
        {
            visualAgent.transform.position = new Vector2(visualAgent.transform.position.x + 1, visualAgent.transform.position.y - 1).normalized;
        }

        // Down
        if (action == 4)
        {
            visualAgent.transform.position = new Vector2(visualAgent.transform.position.x, visualAgent.transform.position.y - 1);
        }

        // Down-Left
        if (action == 5)
        {
            visualAgent.transform.position = new Vector2(visualAgent.transform.position.x - 1, visualAgent.transform.position.y - 1).normalized;
        }

        // Left
        if (action == 6)
        {
            visualAgent.transform.position = new Vector2(visualAgent.transform.position.x - 1, visualAgent.transform.position.y);
        }

        // Up-Left
        if (action == 7)
        {
            visualAgent.transform.position = new Vector2(visualAgent.transform.position.x - 1, visualAgent.transform.position.y + 1).normalized;
        }

        // Do Nothing
        if (action == 8)
        {
            visualAgent.transform.position = new Vector2(visualAgent.transform.position.x, visualAgent.transform.position.y);
        }

        Collider[] hitObjects = Physics.OverlapBox(visualAgent.transform.position, new Vector3(0.3f, 0.3f, 0.3f));
        if (hitObjects.Where(col => col.gameObject.tag == "goal").ToArray().Length == 1)
        {
            reward = 1;
            done = true;
        }
        if (hitObjects.Where(col => col.gameObject.tag == "shark").ToArray().Length == 1)
        {
            reward = -1;
            done = true;
        }
        if (hitObjects.Where(col => col.gameObject.tag == "fish").ToArray().Length == 1)
        {
            reward = 0.5f;
        }
        
        episodeReward += reward;
        // GameObject.Find("RTxt").GetComponent<Text>().text = "Episode Reward: " + episodeReward.ToString("F2");

    }
}
