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

    enum fishStates
    {
        searchForMother, foundMother, Dead
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


        //State machine for little fish
        switch (fishState)
        {

            //While searching for mother it moves at max speed towards the mothers position.
            case fishStates.searchForMother:
                float distToMother = Vector3.Distance(this.transform.position, motherFish.transform.position);

                if(distToMother < 1f)
                {
                    fishState = fishStates.foundMother;
                    this.GetComponent<Rigidbody2D>().velocity = Vector3.zero;
                    break;
                }

                Vector3 dirVector = motherFish.transform.position - this.transform.position;
                dirVector = Vector3.Normalize(dirVector);
                dirVector *= speed;
                this.GetComponent<Rigidbody2D>().velocity = dirVector;
                break;

            //Once it has found the mother, it moves at a more relaxed speed, following the mother.
            case fishStates.foundMother:

                Vector2 cohesionVec = cohesion();
                Vector2 alignmentVec = alignment();
                Vector2 separationVec = separation();
                dirVector = Vector2.zero;
                if(Vector2.Distance(this.transform.position, motherFish.transform.position) > 2f)
                {
                    dirVector = motherFish.transform.position - this.transform.position;
                    dirVector = dirVector.normalized * speed;
                }
                

                float scaleVal = Vector2.Distance(this.transform.position, motherFish.transform.position) / 5f;
                dirVector *= scaleVal;

                this.GetComponent<Rigidbody2D>().velocity = ((Vector2)dirVector + cohesionVec + alignmentVec + separationVec);
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
}
