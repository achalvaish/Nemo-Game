using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SafeZone : MonoBehaviour {

    private int count;
    public Text countText;
    public Text winText;
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
        countText.text = "Count: " + count;
        if(count>=5)
        {
            winText.text = "You Win!!!";
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
