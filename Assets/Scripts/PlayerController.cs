using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public float maxSpeed;

    private Rigidbody2D rigidBody;

	// Use this for initialization
	void Start () {
        rigidBody = this.GetComponent<Rigidbody2D>();
	}
	
	// Update is called once per frame
	void Update () {

        float vert = Input.GetAxis("Vertical");
        float horz = Input.GetAxis("Horizontal");

        Vector3 movementVec = new Vector3(horz, vert, 0);
        movementVec = Vector3.Normalize(movementVec);
        movementVec *= maxSpeed;

        rigidBody.velocity = movementVec;

        if(rigidBody.velocity.x > 0)
        {
            this.transform.localScale = new Vector3(-1, 1, 1);
        }
        else if(rigidBody.velocity.x < 0)
        {
            this.transform.localScale = new Vector3(1, 1, 1);
        }


	}
}
