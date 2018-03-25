using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LittleFish : MonoBehaviour {

    public float speed;

    private PlayerController motherFish;

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
	}
	
	// Update is called once per frame
	void Update () {

        //Timer for how often fish get a new speed.
        randomTime -= Time.deltaTime;

        //Make the fish face the direction its travelling
        if (this.GetComponent<Rigidbody2D>().velocity.x > 0)
        {
            this.transform.localScale = new Vector3(-0.3f, 0.3f, 1);
        }
        else if (this.GetComponent<Rigidbody2D>().velocity.x < 0)
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

                dirVector = motherFish.transform.position - this.transform.position;
                dirVector = Vector3.Normalize(dirVector);
                dirVector *= speed;

                //Scale the fishes speed based on how far from the mother it is
                distToMother = Vector3.Distance(this.transform.position, motherFish.transform.position);
                float scaleVal = distToMother / 3f;

                //Add in some randomness to fish speed so they look like they swim a bit less robotically.
                if(randomTime <= 0)
                {
                    randomVal = Random.value;
                    randomVal += 0.5f;
                    randomTime = 2;
                }



                dirVector *= randomVal * scaleVal;

                this.GetComponent<Rigidbody2D>().velocity = dirVector;
                break;

        }

	}
}
