using UnityEngine;
using System.Collections;
using LibPDBinding;

public class ProceduralAudioUpdater : MonoBehaviour {
	public GameObject[] levels;
	int curLevel = 0;
	// Use this for initialization
	void Start () {
		curLevel = 0;
	}
	
	// Update is called once per frame
	void Update () {
		int newLevel = 0;
		for (int index = levels.Length - 1; index >= 0; index--) {
			if (transform.position.x < levels[index].transform.position.x) {
				newLevel = index + 1;
				break;
			}
		}
		//Debug.Log (newLevel + " --- " + curLevel);
		if (newLevel != curLevel) {
			curLevel = newLevel;
			LibPD.SendFloat ("level", curLevel);
		}
	}
}
