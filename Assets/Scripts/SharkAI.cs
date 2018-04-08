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
    private PathfinderManager pathFinder;
    private Vector2 [] calculatedPath;
    private int pathNum = 0;

    private Transform target;
    public Transform[] fishes;
    public float chaseRange;
    public static int deadFish;

    enum sharkStates
    {
        Patrol, chaseFish, returnToPatrolPath
    }

    private sharkStates sharkState;

    // Use this for initialization
    void Start () {
        this.transform.position = patrolPoints[0].transform.position;
        currentPatrolIndex = 0;
        currentPatrolPoint = patrolPoints[currentPatrolIndex];
        pathFinder = FindObjectOfType<PathfinderManager>();
       
       sharkState = sharkStates.Patrol;
	}

    // Update is called once per frame
    void Update()
    {
        target = findClosestTarget(fishes);
        float distanceToTarget;
        t += Time.deltaTime;

        switch (sharkState)
        {

            // If there are no fish nearby, the shark will patrol
            case sharkStates.Patrol:
                GetComponent<SpriteRenderer>().material.color = Color.white;

                if (target != null)
                {
                    distanceToTarget = Vector3.Distance(transform.position, target.position);
                    Vector2 rayDirection = target.position - transform.position;
                    RaycastHit2D hit = Physics2D.Raycast(transform.position, rayDirection, distanceToTarget, 1 << LayerMask.NameToLayer("Terrain"));

                    // The shark will only chase the little fish if they aren't in an idle state
                    if (distanceToTarget < chaseRange && LittleFish.fishState != LittleFish.fishStates.Idle)
                    {
                        if (!hit)
                        {
                            sharkState = sharkStates.chaseFish;
                            break;
                        }
                    }
                }

                // there is something obstructing the view. 
                Patrol();
                break;
               
        
            // If the shark detects a fish, it will chase it
            case sharkStates.chaseFish:
                GetComponent<SpriteRenderer>().material.color = Color.red;

                if (target != null)
                {
                    distanceToTarget = Vector3.Distance(transform.position, target.position);
                    Vector2 rayDirection = target.position - transform.position;
                    RaycastHit2D hit = Physics2D.Raycast(transform.position, rayDirection, distanceToTarget, 1 << LayerMask.NameToLayer("Terrain"));

                    if (hit || distanceToTarget > chaseRange)
                    {
                        calculatedPath = pathFinder.getPath(this.transform.position, currentPatrolPoint.position);
                        pathNum = 0;
                        sharkState = sharkStates.returnToPatrolPath;
                        break;
                    }
                }

                if (target == null)
                {
                    calculatedPath = pathFinder.getPath(this.transform.position, currentPatrolPoint.position);
                    pathNum = 0;
                    sharkState = sharkStates.returnToPatrolPath;
                    break;
                }

                chaseFish();
                break;

            // If the shark catches the fish, it will eat it
            case sharkStates.returnToPatrolPath:
                GetComponent<SpriteRenderer>().material.color = Color.white;

                if (target != null)
                {
                    distanceToTarget = Vector3.Distance(transform.position, target.position);
                    Vector2 rayDirection = target.position - transform.position;
                    RaycastHit2D hit = Physics2D.Raycast(transform.position, rayDirection, distanceToTarget, 1 << LayerMask.NameToLayer("Terrain"));

                    // The shark will only chase the little fish if they aren't in an idle state
                    if (distanceToTarget < chaseRange && LittleFish.fishState != LittleFish.fishStates.Idle)
                    {
                        if (!hit)
                        {
                            sharkState = sharkStates.chaseFish;
                            break;
                        }
                    }
                }

                returnToPatrolPath();
                break;
        }

        // X axis
        if (transform.position.x <= -19f)
        {
            transform.position = new Vector2(-19f, transform.position.y);
        }
        else if (transform.position.x >= 19f)
        {
            transform.position = new Vector2(19f, transform.position.y);
        }

        // Y axis
        if (transform.position.y <= -10f)
        {
            transform.position = new Vector2(transform.position.x, -10f);
        }
        else if (transform.position.y >= 10f)
        {
            transform.position = new Vector2(transform.position.x, 10f);
        }

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
            newScale = new Vector3(-0.7f, 0.7f, 1);
            transform.localScale = newScale;
        }
        if (patrolPointDir.x > 0f)
        {
            // Get shark to face right
            newScale = new Vector3(0.7f, 0.7f, 1);
            transform.localScale = newScale;
        }
    }

    void chaseFish()
    {
        // Turn to face the target
        // Finding the direction Vector that points to the target
        Vector3 targetDir = target.position - transform.position;
        Vector3 newScale;

        // Move towards the target
        transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
         //  transform.LookAt(target);
        // Figure out if target is to the left or right of the shark
        if (targetDir.x < 0f)
        {
            // Get shark to face left
            newScale = new Vector3(-0.7f, 0.7f, 1);
            transform.localScale = newScale;
        }
        if (targetDir.x > 0f)
        {
            // Get shark to face right
            newScale = new Vector3(0.7f, 0.7f, 1);
            transform.localScale = newScale;
        }
    }

    void returnToPatrolPath()
    {
        if (Vector2.Distance(this.transform.position, currentPatrolPoint.position) > 1f)
        {
            Vector2 targetPosition = pathFinding();

            transform.position = Vector2.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
            Vector3 targetDir = targetPosition - (Vector2)transform.position;

            Vector3 newScale;
            // Figure out if target is to the left or right of the shark
            if (targetDir.x < 0f)
            {
                // Get shark to face left
                newScale = new Vector3(-0.7f, 0.7f, 1);
                transform.localScale = newScale;
            }
            if (targetDir.x > 0f)
            {
                // Get shark to face right
                newScale = new Vector3(0.7f, 0.7f, 1);
                transform.localScale = newScale;
            }


        }
        else
        {
            sharkState = sharkStates.Patrol;
        }
    }

    private Vector2 pathFinding()
    {
        for (int i = calculatedPath.Length - 1; i >= pathNum; i--)
        {
            //Check for line of sight to the node
            Vector2 dir = calculatedPath[i] - (Vector2)this.transform.position;
            RaycastHit2D hit = Physics2D.Raycast(this.transform.position, dir, dir.magnitude, 1 << LayerMask.NameToLayer("Terrain"));

            if (!hit)
            {

                //This calculates a sub point between the last point the player can see and the first the player cant see. This means the 
                //path found is absolutely optimal.
                if (i < calculatedPath.Length - 1)
                {
                    dir = calculatedPath[i + 1] - (Vector2)this.transform.position;
                    Vector2 diffVec = calculatedPath[i + 1] - calculatedPath[i];
                    while (Physics2D.Raycast(this.transform.position, dir, dir.magnitude, 1 << LayerMask.NameToLayer("Terrain")))
                    {
                        dir -= diffVec * 0.01f;
                    }
                }

                pathNum = i;
                return dir + (Vector2)this.transform.position;
            }
        }
        return this.transform.position;
    }

    Transform findClosestTarget (Transform[] targets)
    {
        Transform closestTarget = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;

        // Find the distance from the shark for each fish
        foreach (Transform fish in fishes)
        {
            if(fish.gameObject.activeSelf)
            {
                Vector3 directionToTarget = fish.position - currentPosition;
                float dSqrToTarget = directionToTarget.sqrMagnitude;
                if (dSqrToTarget < closestDistanceSqr)
                {
                    closestDistanceSqr = dSqrToTarget;
                    closestTarget = fish;
                }
            }      
        }

       return closestTarget;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // If the little fish is in an idle state it won't be eaten
        if(other.gameObject.layer == LayerMask.NameToLayer("Fish") && LittleFish.fishState != LittleFish.fishStates.Idle)
        {
            other.gameObject.SetActive(false);
            deadFish++;
        }
    }

}
