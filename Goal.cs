using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnTriggerEnter(Collider other) {
        Debug.Log("Something has entered the socket zone! " + other.name);
        if(other.tag == "PlugTines") {
            Debug.Log("The player has made it to the goal zone, yay!");
            FindObjectOfType<Energy>().PlayerHasReachedSocket(this.transform.position);
        }
    }
}
