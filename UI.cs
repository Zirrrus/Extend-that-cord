using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour {

    public Text levelName;
    public Text levelDescription;

    Level level;

	void Start () {
        level = FindObjectOfType<Level>();
        //set up the menu with the correct text
        levelName.text = level.levelName;
        levelDescription.text = level.levelDescription;
	}
	
	void Update () {
		
	}
}
