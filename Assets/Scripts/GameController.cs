using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

public class GameController : MonoBehaviour {

    public bool loadNetworkFromFile;
    public string loadLowerNetworkName;
    public string saveLowerNetworkName;
    public string loadHigherNetworkName;
    public string saveHigherNetworkName;

    public float timeScale;

    public float learningRate;
    public float momentum;

    public int framesPerAction;
    public int framesPerTrainingUpdate;
    public int framesPerNetworkSave;

    public int minibatchSize;

    private MotherFish motherFish;

    private ArrayList lowerNetEpisode;
    private ArrayList higherNetEpisode;

    private ArrayList lowerExperienceCache;
    public int lowerExperienceCacheSize;
    public int learningCommenceTime; // This is not equal to the number of frames in the cache. Frames in cache is (roughly) = 2 * learningCommenceTime / framesPerAction
    private int lowerExperienceIdx;

    private ArrayList higherExperienceCache;
    public int higherExperienceCacheSize;
    private int higherExperienceIdx;

    private int frameNum;
    private int episodeAge;
    public int episodeTimeout;

    private NeuralNet lowerNet;
    private NeuralNet higherNet;

    private int lowerNetInputs;
    private int higherNetInputs;
    private int higherNetOutputs;

    public Vector2 goalLoc;

    // Use this for initialization
    void Start () {

        frameNum = 0;
        episodeAge = 0;

        lowerExperienceCache = new ArrayList();
        lowerNetEpisode = new ArrayList();

        higherExperienceCache = new ArrayList();
        higherNetEpisode = new ArrayList();

        motherFish = FindObjectOfType<MotherFish>();
        SharkAI[] sharks = FindObjectsOfType<SharkAI>();
        LittleFish[] littleFish = FindObjectsOfType<LittleFish>();
        goalLoc = motherFish.transform.position;

        //Num inputs per net
        lowerNetInputs = 6;  //mother x and y, shark x and y, goal x and y
        higherNetInputs = 2 + 2 + littleFish.Length * 3 + 2;   //mother x and y, shark x  and y, each fishes x and y and bool for caught, anemone x and y

        //higher net outputs
        higherNetOutputs = littleFish.Length + 1;    //An output for each fish and one for the anemone

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
        //Lower net handles path finding to a goal
        lowerNet = new NeuralNet(

            // Layer sizes (6 inputs, 2 hidden layers of size 50, 3 outputs. This could probably be optimised.)
            new int[] { lowerNetInputs, 50, 50, 9 },

            // Whether or not each layer contains a bias neuron (inapplicable to the output layer)
            new bool[] { true, true, true },

            // Activations
            new ActivationFunction[] { new ActivationRELU(), new ActivationRELU(), new ActivationSigmoid() }
        );


        // higher net handlees selecting a goal
        higherNet = new NeuralNet(

            // Layer sizes (6 inputs, 2 hidden layers of size 50, 3 outputs. This could probably be optimised.)
            new int[] { higherNetInputs, 50, 50, higherNetOutputs },

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

        //Save lower
        FileStream stream = new FileStream(Application.dataPath + "/SavedNeuralNetworks/" + saveLowerNetworkName + ".dat", FileMode.Create);
        serializer.Serialize(stream, lowerNet);
        stream.Close();

        //Save upper
        stream = new FileStream(Application.dataPath + "/SavedNeuralNetworks/" + saveHigherNetworkName + ".dat", FileMode.Create);
        serializer.Serialize(stream, higherNet);
        stream.Close();
    }

    private bool LoadNet()
    {
        System.Type[] extraTypes = { typeof(ActivationRELU), typeof(ActivationSigmoid), typeof(ActivationLinear) };
        XmlSerializer serializer = new XmlSerializer(typeof(NeuralNet), extraTypes);

        //Load lower
        FileStream stream = new FileStream(Application.dataPath + "/SavedNeuralNetworks/" + loadLowerNetworkName + ".dat", FileMode.Open);
        lowerNet = serializer.Deserialize(stream) as NeuralNet;
        stream.Close();

        //Load higher
        stream = new FileStream(Application.dataPath + "/SavedNeuralNetworks/" + loadHigherNetworkName + ".dat", FileMode.Open);
        higherNet = serializer.Deserialize(stream) as NeuralNet;
        stream.Close();

        return true;
    }

    private double [] GetLowerStateRepresentation()
    {

        SharkAI shark = FindObjectOfType<SharkAI>();

        int count = 0;

        double[] result = new double[lowerNetInputs];
       
        //First inputs are the mothers position
        double motherX = motherFish.transform.position.x/13.0f;
        double motherY = motherFish.transform.position.y/13.0f;

        result[count] = motherX;
        result[count + 1] = motherY;

        count = 2;

        //Second inputs are the position of the shark
        result[count] = shark.transform.position.x/13.0f;
        count++;
        result[count] = shark.transform.position.y/13.0f;
        count++;

        //Third inputs are position of the target goal
        result[count] = goalLoc.x/13.0f;
        count++;
        result[count] = goalLoc.y/13.0f;

        return result;
    }

    private double[] GetHigherStateRepresentation()
    {

        SharkAI shark = FindObjectOfType<SharkAI>();
        LittleFish[] littlefish = FindObjectsOfType<LittleFish>();
        SafeZone anemone = FindObjectOfType<SafeZone>();

        int count = 0;

        double[] result = new double[higherNetInputs];

        //First inputs are the mothers position (0, 1)
        double motherX = motherFish.transform.position.x / 13.0f;
        double motherY = motherFish.transform.position.y / 13.0f;

        result[count] = motherX;
        result[count + 1] = motherY;

        count = 2;

        //Second inputs are the position of the shark (2, 3)
        result[count] = shark.transform.position.x / 13.0f;
        count++;
        result[count] = shark.transform.position.y / 13.0f;
        count++;

        //Third inputs are position of all the little fish and whether they have been caught (4,5, 6,7, 8,9)
        foreach(LittleFish lf in littlefish)
        {
            result[count] = lf.getPos().x / 13.0f;
            count++;
            result[count] = lf.getPos().y / 13.0f;
            count++;
            if(lf.isCaught())
            {
                result[count] = 1;
            }
            else
            {
                result[count] = 0;
            }
            count++;
        }

        //Fourth inputs are position of anemone (10,11)
        result[count] = anemone.transform.position.x / 13.0f;        
        count++;
        result[count] = anemone.transform.position.y / 13.0f;
        count++;

        return result;
    }

    void Update()
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
        if (((frameNum % framesPerTrainingUpdate) == 1) && (lowerExperienceCache.Count > learningCommenceTime))
        {
            DoTrainingIteration();
        }

        //Check if goal is reached
        if(Vector2.Distance(motherFish.transform.position, goalLoc)  < 1)
        {
            GetNewGoal();
        }

        //Check if game has ended
        CheckGameOver();
    }

    private void DoTrainingIteration()
    {
        double[][] trainingInput = new double[minibatchSize][];
        double?[][] trainingTarget = new double?[minibatchSize][];


        //Train lower net
        for (int sampleNum = 0; sampleNum < minibatchSize; sampleNum++)
        {
            int sampleIdx = Random.Range(0, lowerExperienceCache.Count);

            Experience expItem = (Experience)lowerExperienceCache[sampleIdx];

            trainingInput[sampleNum] = expItem.getStateRepresentation();

            double?[] sampleTarget = new double?[] { null, null, null, null, null, null, null, null, null };

            // 1 for a win, 0 for a loss
            sampleTarget[expItem.getActionTaken()] = expItem.getResult();

            trainingTarget[sampleNum] = sampleTarget;
        }

        lowerNet.Train(trainingInput, trainingTarget, learningRate, momentum);

        //Train higher net
        for (int sampleNum = 0; sampleNum < minibatchSize; sampleNum++)
        {
            int sampleIdx = Random.Range(0, higherExperienceCache.Count);

            Experience expItem = (Experience)higherExperienceCache[sampleIdx];

            trainingInput[sampleNum] = expItem.getStateRepresentation();

            double?[] sampleTarget = new double?[] { null, null, null, null};

            // 1 for a win, 0 for a loss
            sampleTarget[expItem.getActionTaken()] = expItem.getResult();

            trainingTarget[sampleNum] = sampleTarget;
        }

        higherNet.Train(trainingInput, trainingTarget, learningRate, momentum);
    }

    private void AddExperienceToLowerCache(Experience expItem)
    {
        if (lowerExperienceCache.Count < lowerExperienceCacheSize)
        {
            lowerExperienceCache.Add(expItem);
        }
        else
        {
            // Once the cache is full, overwrite the oldest item
            lowerExperienceCache[lowerExperienceIdx] = expItem;
            lowerExperienceIdx = (lowerExperienceIdx + 1) % lowerExperienceCacheSize;
        }
    }

    private void AddExperienceToHigherCache(Experience expItem)
    {
        if (higherExperienceCache.Count < higherExperienceCacheSize)
        {
            higherExperienceCache.Add(expItem);
        }
        else
        {
            // Once the cache is full, overwrite the oldest item
            higherExperienceCache[higherExperienceIdx] = expItem;
            higherExperienceIdx = (higherExperienceIdx + 1) % higherExperienceCacheSize;
        }
    }

    private void CheckGameOver()
    {
        bool gameOver = false;

        float result = -1;


        if(motherFish.isDead)
        {
            result = 0; 
            gameOver = true;
        }
        else if(motherFish.isSafe)
        {
            result = 1 - (float)episodeAge/(float)episodeTimeout;
            gameOver = true;
            Debug.Log(result);
        }

        if(episodeAge >= episodeTimeout)
        {
            result = 0f;
            gameOver = true;
        }

        if (gameOver)
        {
            foreach (Experience expItem in lowerNetEpisode)
            {
                expItem.SetResult(result);
                AddExperienceToLowerCache(expItem);
            }

            foreach(Experience expItem in higherNetEpisode)
            {
                expItem.SetResult(result);
                AddExperienceToHigherCache(expItem);
            }
            lowerNetEpisode.Clear();

            reset();
        }
    }

    private void GetNewPlayerAction()
    {
        double[] playerStateRep = GetLowerStateRepresentation();

        int playerAction = motherFish.UpdateAction(lowerNet, playerStateRep, frameNum - learningCommenceTime);
       
        Experience player1Experience = new Experience(playerStateRep, playerAction);
   
        lowerNetEpisode.Add(player1Experience);
    }

    private void GetNewGoal()
    {
        double[] playerStateRep = GetHigherStateRepresentation();

        int goal = motherFish.UpdateGoal(higherNet, playerStateRep, frameNum - learningCommenceTime);

        switch(goal)
        {
            case 0:
                goalLoc = new Vector2((float)playerStateRep[4], (float)playerStateRep[5]);
                break;
            case 1:
                goalLoc = new Vector2((float)playerStateRep[6], (float)playerStateRep[7]);
                break;
            case 2:
                goalLoc = new Vector2((float)playerStateRep[8], (float)playerStateRep[9]);
                break;
            case 3:
                goalLoc = new Vector2((float)playerStateRep[10], (float)playerStateRep[11]);
                break;
        }


        Experience player1Experience = new Experience(playerStateRep, goal);

        higherNetEpisode.Add(player1Experience);
    }

    private void reset()
    {
        FindObjectOfType<SharkAI>().reset();
        motherFish.reset();

        episodeAge = 0;

        LittleFish[] littleFish = FindObjectsOfType<LittleFish>();
        foreach(LittleFish lf in littleFish)
        {
            lf.reset();
        }
    }
}
