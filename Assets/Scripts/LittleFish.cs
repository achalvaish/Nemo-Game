using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LittleFish : MonoBehaviour {

    public Vector2 startPos;

	// Use this for initialization
	void Start () {
        startPos = this.transform.position;
	}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        this.transform.position = new Vector2(100, 100);
    }

    public void reset()
    {
        float Xpos = Random.Range(1, 13);
        float ypos = Random.Range(1, 13);


        this.transform.position = new Vector2(Xpos, ypos);
        startPos = this.transform.position;
    }

}
