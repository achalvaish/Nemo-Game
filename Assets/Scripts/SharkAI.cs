using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SharkAI : MonoBehaviour {

    public Transform[] patrolPoints;
    public float speed;
    Transform currentPatrolPoint;
    int currentPatrolIndex;

    private float t, maxT, distance;
    private Vector3 lerpStart;
    private Vector3 lerpDir;
    
	// Use this for initialization
	void Start () {
        this.transform.position = patrolPoints[0].transform.position;
        currentPatrolIndex = 0;
        currentPatrolPoint = patrolPoints[currentPatrolIndex];
	}

    // Update is called once per frame
    void Update()
    {
        t += Time.deltaTime;
        Patrol();

    }

    void Patrol()
    {
        
        // Check if shark has reached patrol point
        if (Vector3.Distance(transform.position, currentPatrolPoint.position) < 1f)
        {

            int a = currentPatrolIndex;

            // Shark has reached the patrol point - get the next one
            // Check to see if we have any more patrol points - if not go back to the beginning
            if (currentPatrolIndex + 1 < patrolPoints.Length)
            {
                currentPatrolIndex++;
            }
            else
            {
                currentPatrolIndex = 0;
            }
            currentPatrolPoint = patrolPoints[currentPatrolIndex];

            t = 0;

            //Calculate the distance between the previous patrol point and the new one
           
            int b = currentPatrolIndex;
            if (a < 0)
            {
                b = patrolPoints.Length-1;
            }
            distance = Vector3.Distance(patrolPoints[a].position, patrolPoints[b].position);

            //Calculate the legngth of time to cover that distance
            maxT = distance / speed;

            lerpStart = patrolPoints[a].transform.position;
            lerpDir = patrolPoints[b].transform.position - lerpStart;
        }

        // Turn to face the current patrol point
        // Finding the direction Vector that points to the patrol point
        Vector3 patrolPointDir = currentPatrolPoint.position - transform.position;
        Vector3 newScale;



        //Begin Lerp
        this.transform.position = lerpStart + (t / maxT) * lerpDir;


        
        
        // Figure out if the patrol point is to the left or right of the shark
        if (patrolPointDir.x < 0f) {
            // Get shark to face left
            newScale = new Vector3(-0.5f, 0.5f, 1);
            transform.localScale = newScale;
        }
        if (patrolPointDir.x > 0f)
        {
            // Get shark to face right
            newScale = new Vector3(0.5f, 0.5f, 1);
            transform.localScale = newScale;
        }
    }

}
