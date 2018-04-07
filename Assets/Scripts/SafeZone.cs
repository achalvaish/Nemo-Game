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

        
    }

    void setCountText()
    {
        countText.text = "Score: " + count;
        if (SharkAI.deadFish + count == fishes.Length)
        {
            if (count >= 5)
            {
                winText.text = "You Win!!!";
            }
            else
            {
                winText.text = "Game Over !!!";
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.layer==LayerMask.NameToLayer("Fish"))
        {
            other.gameObject.SetActive(false);
            count++;
            setCountText();
        }
    }
}
