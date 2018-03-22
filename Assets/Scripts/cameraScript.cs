using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraScript : MonoBehaviour {

    public float scrollTime;

    private bool isScrolling;
    private float lerpVal;
    private float t;

	// Use this for initialization
	void Start () {
        this.transform.position = new Vector3(this.transform.position.x, 30.0f, this.transform.position.z);
        isScrolling = true;
        lerpVal = 0;
        t = 0;
	}
	
	// Update is called once per frame
	void Update () {

        t += Time.deltaTime;

        if(isScrolling)
        {
            float camY;

            camY = 30 - ((t / scrollTime) * 30);
            this.transform.position = new Vector3(this.transform.position.x, camY, this.transform.position.z);
            if(camY <= 0 )
            {
                isScrolling = false;
            }
        }

	}

    public void sayHello()
    {
        Debug.Log("Hello!");
    }

}
