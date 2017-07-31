using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlugTines : MonoBehaviour {

    Movement plug;

	void Start () {
        plug = FindObjectOfType<Movement>();
	}

    void OnCollisionEnter(Collision collision) {
        //Debug.Log("Tines have collided with " + collision.gameObject.name);
        plug.OnTinesCollision(collision);
    }

    void OnTriggerEnter(Collider other) {
        //Debug.Log("Something has entered out trigger? " + other.name);
        plug.OnTinesTrigger(other);
    }
}
