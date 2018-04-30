using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

public class GameController : MonoBehaviour {

    public bool loadNetworkFromFile;
    public string loadNetworkName;
    public string saveNetworkName;

    public float timeScale;

    public float learningRate;
    public float momentum;

    public int framesPerAction;
    public int framesPerTrainingUpdate;
    public int framesPerNetworkSave;

    public int minibatchSize;

    private MotherFish motherFish;

    private ArrayList currentEpisode;

    private ArrayList experienceCache;
    public int experienceCacheSize;
    public int learningCommenceTime; // This is not equal to the number of frames in the cache. Frames in cache is (roughly) = 2 * learningCommenceTime / framesPerAction
    private int experienceIdx;

    private int frameNum;
    private int episodeAge;
    public int episodeTimeout;

    private NeuralNet net;

    private int numInputs;

    // Use this for initialization
    void Start () {

        frameNum = 0;
        episodeAge = 0;

        experienceCache = new ArrayList();
        currentEpisode = new ArrayList();

        motherFish = FindObjectOfType<MotherFish>();
        SharkAI[] sharks = FindObjectsOfType<SharkAI>();
        LittleFish[] littleFish = FindObjectsOfType<LittleFish>();

        numInputs = 2 + 2 * sharks.Length + 2 * littleFish.Length;

        if (loadNetworkFromFile)
        {
            LoadNet();
        }
        else
        {
            CreateNet();
        }

    }

    private void CreateNet()
    {
        net = new NeuralNet(

            // Layer sizes (6 inputs, 2 hidden layers of size 50, 3 outputs. This could probably be optimised.)
            new int[] { numInputs, 50, 50, 9 },

            // Whether or not each layer contains a bias neuron (inapplicable to the output layer)
            new bool[] { true, true, true },

            // Activations
            new ActivationFunction[] { new ActivationRELU(), new ActivationRELU(), new ActivationSigmoid() }
        );
    }

    private void SaveNet()
    {
        System.Type[] extraTypes = { typeof(ActivationRELU), typeof(ActivationSigmoid), typeof(ActivationLinear) };
        XmlSerializer serializer = new XmlSerializer(typeof(NeuralNet), extraTypes);
        FileStream stream = new FileStream(Application.dataPath + "/SavedNeuralNetworks/" + saveNetworkName + ".dat", FileMode.Create);
        serializer.Serialize(stream, net);
        stream.Close();
    }

    private bool LoadNet()
    {
        System.Type[] extraTypes = { typeof(ActivationRELU), typeof(ActivationSigmoid), typeof(ActivationLinear) };
        XmlSerializer serializer = new XmlSerializer(typeof(NeuralNet), extraTypes);
        FileStream stream = new FileStream(Application.dataPath + "/SavedNeuralNetworks/" + loadNetworkName + ".dat", FileMode.Open);
        net = serializer.Deserialize(stream) as NeuralNet;
        stream.Close();
        return true;
    }

    private double [] GetStateRepresentation()
    {

        SharkAI [] sharks = FindObjectsOfType<SharkAI>();
        LittleFish[] littleFish = FindObjectsOfType<LittleFish>();

        int count = 0;

        double[] result = new double[2 + sharks.Length*2 + littleFish.Length*2];

        
        double motherX = motherFish.transform.position.x;
        double motherY = motherFish.transform.position.y;

        result[count] = motherX;
        result[count + 1] = motherY;

        count = 2;
        foreach(SharkAI s in sharks)
        {
            result[count] = s.transform.position.x;
            count++;
            result[count] = s.transform.position.y;
            count++;
        }
        foreach(LittleFish lf in littleFish)
        {
            result[count] = lf.transform.position.x;
            count++;
            result[count] = lf.transform.position.y;
            count++;
        }

        return result;
    }

    void FixedUpdate()
    {
        frameNum++;
        episodeAge++;

        // Update the time scale if it's been changed in the inspector
        if (timeScale > 0.0f)
        {
            Time.timeScale = timeScale;
        }

        //Save net
        if ((frameNum % framesPerNetworkSave) == 1)
        {
            SaveNet();
        }

        //New action
        if ((frameNum % framesPerAction) == 1)
        {
            GetNewPlayerAction();
        }

        //Train
        if (((frameNum % framesPerTrainingUpdate) == 1) && (experienceCache.Count > learningCommenceTime))
        {
            DoTrainingIteration();
        }

        //Check if game has ended
        CheckGameOver();
    }

    private void DoTrainingIteration()
    {
        double[][] trainingInput = new double[minibatchSize][];
        double?[][] trainingTarget = new double?[minibatchSize][];

        for (int sampleNum = 0; sampleNum < minibatchSize; sampleNum++)
        {
            int sampleIdx = Random.Range(0, experienceCache.Count);

            Experience expItem = (Experience)experienceCache[sampleIdx];

            trainingInput[sampleNum] = expItem.getStateRepresentation();

            double?[] sampleTarget = new double?[] { null, null, null };

            // 1 for a win, 0 for a loss
            sampleTarget[expItem.getActionTaken()] = expItem.getResult();

            trainingTarget[sampleNum] = sampleTarget;
        }

        net.Train(trainingInput, trainingTarget, learningRate, momentum);
    }

    private void AddExperienceToCache(Experience expItem)
    {
        if (experienceCache.Count < experienceCacheSize)
        {
            experienceCache.Add(expItem);
        }
        else
        {
            // Once the cache is full, overwrite the oldest item
            experienceCache[experienceIdx] = expItem;
            experienceIdx = (experienceIdx + 1) % experienceCacheSize;
        }
    }

    private void CheckGameOver()
    {
        bool gameOver = false;

        LittleFish [] littleFish = FindObjectsOfType<LittleFish>();

        int count = 0;
        int dead = 0;
        int safe = 0;
        float result = -1;


        float distToMother = 40.0f;
        foreach (LittleFish lf in littleFish)
        {
            count++;
            if (lf.GetComponent<SpriteRenderer>().enabled == false)
            {
                if(lf.isDead())
                {
                    dead++;
                }
                else if(lf.isGoal())
                {
                    safe++;
                }
            }
            else
            {
                float fishDist = Vector2.Distance(lf.transform.position, motherFish.transform.position);
                if (fishDist < distToMother)
                {
                    distToMother = fishDist;
                }
            }
        }

        result = (float)safe / (float)count;

        /*if(episodeAge >= episodeTimeout)
        {
            result = 40.0f/ distToMother;
            gameOver = true;
        }*/

        if (gameOver)
        {
            foreach (Experience expItem in currentEpisode)
            {
                expItem.SetResult(result);
                AddExperienceToCache(expItem);
            }
            currentEpisode.Clear();

            Reset();
        }
    }

    private void GetNewPlayerAction()
    {
        double[] playerStateRep = GetStateRepresentation();

        int playerAction = motherFish.UpdateAction(net, playerStateRep, frameNum - learningCommenceTime);
       
        Experience player1Experience = new Experience(playerStateRep, playerAction);
   
        currentEpisode.Add(player1Experience);
    }

    private void Reset()
    {
        
    }
}
