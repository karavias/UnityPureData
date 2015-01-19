using UnityEngine;
using System.Collections;
using LibPDBinding;

public class ChangeTerrain : MonoBehaviour {
	//remember the previous terrain I have been, 
	//in case it was grass or snow.
	//I will need this to recover the terrain, when I leave a wooden area.
	string previous = "dirt";

	/**
	 * When the character changes terrain, update the pure data
	 * with the new terrain info.
	 **/
	void OnTriggerEnter(Collider col) {
		Debug.Log ("triggered with " + col.gameObject.tag);
		if (col.gameObject.tag == "grass") {
			previous = "grass";
			//when inside the grass, be sure to enable the
			//procedural audio.
			LibPD.SendFloat ("proVolume", 0.5f);
			LibPD.SendMessage ("terrain", "grass", new object[0]);
		} else if (col.gameObject.tag == "snow") {
			previous = "snow";
			LibPD.SendMessage ("terrain", "snow", new object[0]);
		} else if (col.gameObject.tag == "wood") {
			LibPD.SendMessage ("terrain", "wood", new object[0]);
		}
	}




	/**
	 * When character exits a terrain, recover to the last
	 * known terrain.
	 **/
	void OnTriggerExit(Collider col) {
		Debug.Log ("exit triggered with " + col.gameObject.tag);

		if (col.gameObject.tag == "none") {
			return;
		}
		if (col.gameObject.tag == "wood") {
			LibPD.SendMessage ("terrain", previous, new object[0]);

			
		} else {
			LibPD.SendMessage ("terrain", "dirt", new object[0]);
		}
		if (col.gameObject.tag == "grass") {
			//when leaving the grass, be sure to mute the 
			//procedural audio.
			LibPD.SendFloat ("proVolume", 0);
		}
	}
}
