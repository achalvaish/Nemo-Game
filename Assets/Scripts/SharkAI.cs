using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SharkAI : MonoBehaviour {

    public Transform[] patrolPoints;
    public float speed;
    Transform currentPatrolPoint;
    int currentPatrolIndex;
    
	// Use this for initialization
	void Start () {
        currentPatrolIndex = 0;
        currentPatrolPoint = patrolPoints[currentPatrolIndex];
	}

    // Update is called once per frame
    void Update()
    {

        Patrol();

    }

    void Patrol()
    {
        
        // Check if shark has reached patrol point
        if (Vector3.Distance(transform.position, currentPatrolPoint.position) < 1f)
        {
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
        }

        // Turn to face the current patrol point
        // Finding the direction Vector that points to the patrol point
        Vector3 patrolPointDir = currentPatrolPoint.position - transform.position;
        Vector3 newScale;

        // Figure out if the patrol point is to the left or right of the shark
        if (patrolPointDir.x < 0f) {
            transform.Translate(Vector3.left * Time.deltaTime * speed);
            // Get shark to face left
            newScale = new Vector3(-0.5f, 0.5f, 1);
            transform.localScale = newScale;
        }
        if (patrolPointDir.x > 0f)
        {
            transform.Translate(Vector3.right * Time.deltaTime * speed);
            // Get shark to face right
            newScale = new Vector3(0.5f, 0.5f, 1);
            transform.localScale = newScale;
        }
    }

}
