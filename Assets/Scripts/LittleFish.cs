using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LittleFish : MonoBehaviour {

    public float speed;
    public float nearbyRange;
    public float cohesionStrength;
    public float separationStength;
    public float alignmentStrength;

    private MotherFish motherFish;
    private LittleFish [] otherFish;
    private Transform shark;
    public Transform[] sharks;
    public float chaseRange;

    public Vector2 startPos;

    private GameController gc;

    enum fishStates
    {
        Idle, searchForMother, followMother, Flee, Dead, Goal
    }

    private fishStates fishState;
    private float randomTime;
    private float randomVal;
    private bool caught;

	// Use this for initialization
	void Start () {
        motherFish = FindObjectOfType<MotherFish>();
        fishState = fishStates.Idle;
        randomTime = 0;
        startPos = this.transform.position;
        caught = false;

        otherFish = FindObjectsOfType<LittleFish>();
        gc = FindObjectOfType<GameController>();

        // shark = GameObject.FindGameObjectWithTag("Shark").transform;
	}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        motherFish.numFish++;
        caught = true;
        gc.GetNewGoal();
        this.transform.position = new Vector2(100, 100);
    }

    public void reset()
    {
        fishState = fishStates.Idle;
        caught = false;
        float Xpos = Random.Range(1, 13);
        float ypos = Random.Range(1, 13);


        this.transform.position = new Vector2(Xpos, ypos);
        startPos = this.transform.position;
    }

    public bool isCaught()
    {
        return caught;
    }

    public Vector2 getPos()
    {
        return startPos;
    }

}
