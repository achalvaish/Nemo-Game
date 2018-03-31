using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LittleFish : MonoBehaviour {

    public float speed;
    public float nearbyRange;
    public float cohesionStrength;
    public float separationStength;
    public float alignmentStrength;

    private PlayerController motherFish;
    private LittleFish [] otherFish;
    private Transform Shark;
    public float chaseRange;

    enum fishStates
    {
        Idle, searchForMother, followMother, Evade, Dead
    }

    private fishStates fishState;
    private float randomTime;
    private float randomVal;

	// Use this for initialization
	void Start () {
        motherFish = FindObjectOfType<PlayerController>();
        fishState = fishStates.searchForMother;
        randomTime = 0;

        otherFish = FindObjectsOfType<LittleFish>();

        Shark = GameObject.FindGameObjectWithTag("Shark").transform;
	}
	
	// Update is called once per frame
	void Update () {

        //Timer for how often fish get a new speed.
        randomTime -= Time.deltaTime;

        //Make the fish face the direction its travelling
        if (this.GetComponent<Rigidbody2D>().velocity.x > 0.5f)
        {
            this.transform.localScale = new Vector3(-0.3f, 0.3f, 1);
        }
        else if (this.GetComponent<Rigidbody2D>().velocity.x < -0.5f)
        {
            this.transform.localScale = new Vector3(0.3f, 0.3f, 1);
        }

        float distToMother = Vector3.Distance(this.transform.position, motherFish.transform.position);
        float distToShark = Vector3.Distance(transform.position, Shark.position);

        //State machine for little fish
        switch (fishState)
        {
            case fishStates.Idle:
                Idle();
                break;

            //While searching for mother it moves at max speed towards the mothers position.
            case fishStates.searchForMother:
                
                if (distToMother < 1f)
                {
                    fishState = fishStates.followMother;
                    this.GetComponent<Rigidbody2D>().velocity = Vector3.zero;
                    break;
                }

                searchForMother();
                break;

            //Once it has found the mother, it moves at a more relaxed speed, following the mother.
            case fishStates.followMother:
                if (distToShark < chaseRange)
                {
                    fishState = fishStates.Evade;
                    break;
                }

                followMother();
                break;

            // If a fish is detected by a shark, it will attempt to evade the shark
            case fishStates.Evade:
                if (distToShark > chaseRange)
                {
                    fishState = fishStates.followMother;
                    break;
                }

                Evade();
                break;

            // If a fish is caught by a shark, it will be eaten and is out of the game
            case fishStates.Dead:
                Dead();
                break;

        }

	}

    //Returns the position of all the fish that are within the range to be grouped.
    private LittleFish[] getNearbyFish()
    {
        List<LittleFish> nearbyFish = new List<LittleFish>();
        for(int i = 0; i < otherFish.Length; i++)
        {
            if (otherFish[i] == this.GetComponent<LittleFish>())
            {
                continue;
            }
            if(Vector2.Distance(otherFish[i].transform.position, this.transform.position) < nearbyRange)
            {
                nearbyFish.Add(otherFish[i]);
            }
        }

        return nearbyFish.ToArray();
    }
    
    //Returns the net cohesion vector
    private Vector2 cohesion()
    {
        Vector2 cohesionVector = new Vector2();
        LittleFish[] nearbyFish = getNearbyFish();
        if (nearbyFish.Length == 0)
        {
            return Vector2.zero;
        }
        for (int i = 0; i < nearbyFish.Length; i++)
        {
            cohesionVector += (Vector2)nearbyFish[i].transform.position;
        }
        cohesionVector /= nearbyFish.Length;
        cohesionVector -= (Vector2)this.transform.position;
        cohesionVector *= cohesionStrength;

        return cohesionVector;
    }

    //Returns the net separation vector
    private Vector2 separation()
    {

        Vector2 separationVector = new Vector2();
        LittleFish[] nearbyFish = getNearbyFish();
        if (nearbyFish.Length == 0)
        {
            return Vector2.zero;
        }
        for (int i = 0; i < nearbyFish.Length; i++)
        {
            Vector2 sepVec = ((Vector2)this.transform.position - (Vector2)nearbyFish[i].transform.position);
            sepVec *= 1 / sepVec.sqrMagnitude;
            separationVector += sepVec;
        }

        separationVector *= separationStength;
        return separationVector;
    }

    //Returns the net alignment vector
    private Vector2 alignment()
    {
        Vector2 alignmentVector = new Vector2();
        LittleFish[] nearbyFish = getNearbyFish();
        if(nearbyFish.Length == 0)
        {
            return Vector2.zero;
        }
        for(int i = 0; i < nearbyFish.Length; i++)
        {
            alignmentVector += nearbyFish[i].GetComponent<Rigidbody2D>().velocity.normalized;
        }

        alignmentVector /= nearbyFish.Length;
        alignmentVector -= this.GetComponent<Rigidbody2D>().velocity.normalized;
        alignmentVector *= alignmentStrength;

        return alignmentVector;
    }

    void Idle()
    {

    }

    void searchForMother()
    {
        Vector3 dirVector = motherFish.transform.position - this.transform.position;
        dirVector = Vector3.Normalize(dirVector);
        dirVector *= speed;
        this.GetComponent<Rigidbody2D>().velocity = dirVector;
    }

    void followMother()
    {
        Vector2 cohesionVec = cohesion();
        Vector2 alignmentVec = alignment();
        Vector2 separationVec = separation();
        Vector3 dirVector = Vector2.zero;

        if (Vector2.Distance(this.transform.position, motherFish.transform.position) > 2f)
        {
            dirVector = motherFish.transform.position - this.transform.position;
            dirVector = dirVector.normalized * speed;
        }

        float scaleVal = Vector2.Distance(this.transform.position, motherFish.transform.position) / 5f;
        dirVector *= scaleVal;
        this.GetComponent<Rigidbody2D>().velocity = ((Vector2)dirVector + cohesionVec + alignmentVec + separationVec);
    }

    void Evade()
    {
        // Find direction fish should move to get away from shark
        Vector3 moveDirection = transform.position - Shark.transform.position;
        
        // Move away from shark
        transform.Translate(moveDirection.normalized * speed * Time.deltaTime);

    }

    void Dead()
    {

    }
}
