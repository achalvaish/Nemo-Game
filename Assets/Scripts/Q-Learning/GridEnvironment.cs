using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.IO;
using UnityEditor;

public class GridEnvironment : Environment
{

    public List<GameObject> actorObjs;
    public string[] players;
    public GameObject visualAgent;
    int gridSize;
    int[] objectPositions;
    float episodeReward;
    int fishCount;

    void Start()
    {
        BeginNewGame();
    }

    // Restarts the learning process with a new grid
    public void BeginNewGame()
    {
        gridSize = 13;
        fishCount = 0;

        WipeFile();
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
        // float point = (gridSize * agent.transform.position.x) + agent.transform.position.y;
        float point = agent.transform.position.x + agent.transform.position.y;
        state.Add(point);

        return state;
    }

    // Resets the episode
    public override void Reset()
    {
        base.Reset();

        foreach (GameObject actor in actorObjs)
        {
            DestroyImmediate(actor);
        }
        
        agent.transform.position = new Vector2(1.0f, 1.0f);
        episodeReward = 0;
        fishCount = 0;
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
            // Check if there is a terrain block in the position where the agent would be moving to
            Collider2D[] blockTest = Physics2D.OverlapBoxAll(new Vector2(agent.transform.position.x, agent.transform.position.y + 1), new Vector2(0.3f, 0.3f), 0.0f);
            if (blockTest.Where(col => col.gameObject.tag == "terrain").ToArray().Length == 0)
            {
                agent.transform.position = new Vector2(agent.transform.position.x, agent.transform.position.y + 1);
            }                          
        }

        // Up-Right
        if (action == 1)
        {
            // Check if there is a terrain block in the position where the agent would be moving to
            Collider2D[] blockTest = Physics2D.OverlapBoxAll(new Vector2(agent.transform.position.x + 1, agent.transform.position.y + 1).normalized, new Vector2(0.3f, 0.3f), 0.0f);
            if (blockTest.Where(col => col.gameObject.tag == "terrain").ToArray().Length == 0)
            {
                agent.transform.position = new Vector2(agent.transform.position.x + 1, agent.transform.position.y + 1).normalized;
            }
                            
        }

        // Right
        if (action == 2)
        {
            // Check if there is a terrain block in the position where the agent would be moving to
            Collider2D[] blockTest = Physics2D.OverlapBoxAll(new Vector2(agent.transform.position.x + 1, agent.transform.position.y), new Vector2(0.3f, 0.3f), 0.0f);
            if (blockTest.Where(col => col.gameObject.tag == "terrain").ToArray().Length == 0)
            {
                agent.transform.position = new Vector2(agent.transform.position.x + 1, agent.transform.position.y);
            }
                           
        }

        // Down-Right
        if (action == 3)
        {
            // Check if there is a terrain block in the position where the agent would be moving to
            Collider2D[] blockTest = Physics2D.OverlapBoxAll(new Vector2(agent.transform.position.x + 1, agent.transform.position.y - 1).normalized, new Vector2(0.3f, 0.3f), 0.0f);
            if (blockTest.Where(col => col.gameObject.tag == "terrain").ToArray().Length == 0)
            {
                agent.transform.position = new Vector2(agent.transform.position.x + 1, agent.transform.position.y - 1).normalized;
            }      
        }

        // Down
        if (action == 4)
        {
            // Check if there is a terrain block in the position where the agent would be moving to
            Collider2D[] blockTest = Physics2D.OverlapBoxAll(new Vector2(agent.transform.position.x, agent.transform.position.y - 1), new Vector2(0.3f, 0.3f), 0.0f);
            if (blockTest.Where(col => col.gameObject.tag == "terrain").ToArray().Length == 0)
            {
                agent.transform.position = new Vector2(agent.transform.position.x, agent.transform.position.y - 1);
            }    
        }

        // Down-Left
        if (action == 5)
        {
            // Check if there is a terrain block in the position where the agent would be moving to
            Collider2D[] blockTest = Physics2D.OverlapBoxAll(new Vector2(agent.transform.position.x - 1, agent.transform.position.y - 1).normalized, new Vector2(0.3f, 0.3f), 0.0f);
            if (blockTest.Where(col => col.gameObject.tag == "terrain").ToArray().Length == 0)
            {
                agent.transform.position = new Vector2(agent.transform.position.x - 1, agent.transform.position.y - 1).normalized;
            }
        }

        // Left
        if (action == 6)
        {
            // Check if there is a terrain block in the position where the agent would be moving to
            Collider2D[] blockTest = Physics2D.OverlapBoxAll(new Vector2(agent.transform.position.x - 1, agent.transform.position.y), new Vector2(0.3f, 0.3f), 0.0f);
            if (blockTest.Where(col => col.gameObject.tag == "terrain").ToArray().Length == 0)
            {
                agent.transform.position = new Vector2(agent.transform.position.x - 1, agent.transform.position.y);
            }   
        }

        // Up-Left
        if (action == 7)
        {
            // Check if there is a terrain block in the position where the agent would be moving to
            Collider2D[] blockTest = Physics2D.OverlapBoxAll(new Vector2(agent.transform.position.x - 1, agent.transform.position.y + 1).normalized, new Vector2(0.3f, 0.3f), 0.0f);
            if (blockTest.Where(col => col.gameObject.tag == "terrain").ToArray().Length == 0)
            {
                agent.transform.position = new Vector2(agent.transform.position.x - 1, agent.transform.position.y + 1).normalized;
            }
        }

        // Do Nothing
        if (action == 8)
        {
            agent.transform.position = new Vector2(agent.transform.position.x, agent.transform.position.y);
        }

        // If the agent finds the goal
        if (agent.GetComponent<Collider2D>().IsTouching(GameObject.FindGameObjectWithTag("goal").GetComponent<Collider2D>()))
        {
            reward = 1;
            // Debug.Log("Goal reward");
            done = true;
        }

        // If the agent hits the shark
        if (agent.GetComponent<Collider2D>().IsTouching(GameObject.FindGameObjectWithTag("shark").GetComponent<Collider2D>()))
        {
            reward = -1;
            // Debug.Log("Shark reward");
            done = true;

        }

        // If the agent finds a fish
        GameObject[] littleFish = GameObject.FindGameObjectsWithTag("fish");
        foreach (GameObject fish in littleFish)
        {
            if (agent.GetComponent<Collider2D>().IsTouching(fish.GetComponent<Collider2D>()))
            {
                reward = 1;
                fishCount++;
                // Debug.Log("Fish reward");
            }
        }

        episodeReward += reward;

        if (done == true)
        {
            // Debug.Log("Episode reward " + episodeReward);
            WriteResultsToFile();
        }
    }

    public void WriteResultsToFile()
    {
        string path = "Assets/Resources/results.txt";

        //Write the episode number and reward to the results.txt file
        StreamWriter writer = new StreamWriter(path, true);
        writer.WriteLine((episodeCount - 1) + "\t" + fishCount + "\t " + episodeReward);
        writer.Close();

        //Re-import the file to update the reference in the editor
        AssetDatabase.ImportAsset(path);
        TextAsset asset = (TextAsset)Resources.Load("results.txt");
    }

    public void WipeFile()
    {
        string path = "Assets/Resources/results.txt";

        //Erase the text in the results.txt file from the previous game
        StreamWriter writer = new StreamWriter(path, false);
        writer.WriteLine("Episode Fish \t Reward");
        writer.Close();

        //Re-import the file to update the reference in the editor
        AssetDatabase.ImportAsset(path);
        TextAsset asset = (TextAsset)Resources.Load("results.txt");
    }


}
