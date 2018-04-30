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

    enum fishStates
    {
        Idle, searchForMother, followMother, Flee, Dead, Goal
    }

    private fishStates fishState;
    private float randomTime;
    private float randomVal;

	// Use this for initialization
	void Start () {
        motherFish = FindObjectOfType<MotherFish>();
        fishState = fishStates.Idle;
        randomTime = 0;
        startPos = this.transform.position;

        otherFish = FindObjectsOfType<LittleFish>();

        // shark = GameObject.FindGameObjectWithTag("Shark").transform;
	}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        motherFish.numFish++;
        this.transform.position = new Vector2(100, 100);
    }

    public void reset()
    {
        fishState = fishStates.Idle;
        float Xpos = Random.Range(-6, 6);
        float ypos = Random.Range(-6, 6);


        this.transform.position = new Vector2(Xpos, ypos);
        startPos = this.transform.position;
    }

    public Vector2 getPos()
    {
        return startPos;
    }

}
