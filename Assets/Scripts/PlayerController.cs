﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public float maxSpeed;

    private Rigidbody2D rigidBody;
    private Vector2 clickPos;
    private Vector2[] calculatedPath;
    private PathfinderManager pathFinder;
    private int pathNum = 0;

	// Use this for initialization
	void Start () {
        rigidBody = this.GetComponent<Rigidbody2D>();
        pathFinder = FindObjectOfType<PathfinderManager>();
        clickPos = this.transform.position;
	}
	
	// Update is called once per frame
	void Update () {

        //Runs when the mouse is clicked.
        if(Input.GetMouseButtonDown(0))
        {
            clickPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            RaycastHit2D hit = Physics2D.Raycast(clickPos, Vector2.zero, 1f, 1 << LayerMask.NameToLayer("Terrain"));
            if(!hit)
            {
                calculatedPath = pathFinder.getPath(this.transform.position, clickPos);
                pathNum = 0;
            }
            else
            {
                clickPos = this.transform.position;
            }
        }

        //If the player is not yet at the mouse click position, move towards it following the path finder
        if (Vector2.Distance(this.transform.position, clickPos) > 0.2f)
        {
            Vector2 targetPosition = pathFinding();

            Vector2 velVector = targetPosition - (Vector2)this.transform.position;
            velVector = velVector.normalized;
            velVector *= maxSpeed;
            

            rigidBody.velocity = velVector;
        }
        else
        {
            rigidBody.velocity = Vector2.zero;
        }


        //Face the direction you are moving
        if(rigidBody.velocity.x > 0)
        {
            this.transform.localScale = new Vector3(-1, 1, 1);
        }
        else if(rigidBody.velocity.x < 0)
        {
            this.transform.localScale = new Vector3(1, 1, 1);
        }


	}

    //Function to find which point of the found path the player is moving towards.
    private Vector2 pathFinding()
    {
        for(int i = calculatedPath.Length - 1; i >= pathNum; i--)
        {
            //Check for line of sight to the node
            Vector2 dir = calculatedPath[i] - (Vector2)this.transform.position;
            RaycastHit2D hit = Physics2D.Raycast(this.transform.position, dir, dir.magnitude, 1 << LayerMask.NameToLayer("Terrain"));

            if(!hit)
            {
                if(i < calculatedPath.Length-1)
                {
                    dir = calculatedPath[i+1] - (Vector2)this.transform.position;
                    Vector2 diffVec = calculatedPath[i + 1] - calculatedPath[i];
                    while(Physics2D.Raycast(this.transform.position, dir, dir.magnitude, 1 << LayerMask.NameToLayer("Terrain")))
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
}
