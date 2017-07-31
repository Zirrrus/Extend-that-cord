using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainZone : MonoBehaviour {

    public Utility.Terrain terrainType;

	void Start () {
		
	}
	
	void Update () {
		
	}

    void OnTriggerEnter(Collider other) {
        Debug.Log("OnTriggerEnter!");
        if (other.tag == "Player") {
            Debug.Log("Player has entered our zone.");
            Movement.currentTerrain = terrainType;
        }
    }

    void OnTriggerExit(Collider other) {
        Debug.Log("OnTriggerExit");
        if (other.tag == "Player") {
            Movement.currentTerrain = Utility.Terrain.normal;
        }
    }
}
