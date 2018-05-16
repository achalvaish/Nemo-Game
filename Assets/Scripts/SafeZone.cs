using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SafeZone : MonoBehaviour {

    private int count;
    public Text countText;
    public Text winText;
    public Transform[] fishes;
    // Use this for initialization
    void Start () {
        count = 0;
        setCountText();
        winText.text = "";
	}
	
	// Update is called once per frame
	void Update () {


        //setCountText();
    }

    void setCountText()
    {
        countText.text = "Score: " + count;

        /*if (SharkAI.deadFish + count == fishes.Length)
        {
            if (count == 6)
            {
                winText.text = "You win !!!";
            }
            else
            {
                winText.text = "Game over !!!";
            }
        }*/
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.GetComponent<MotherFish>() != null)
        {
            other.gameObject.GetComponent<MotherFish>().safe();
            count = other.gameObject.GetComponent<MotherFish>().numFish;
        }
    }
}
