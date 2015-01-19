using UnityEngine;
using System.Collections;
using LibPDBinding;

public class PDComponent : MonoBehaviour {

	public enum RolloffMode {
		Logarithmic,
		Linear
	}

	public string patchName;
	int patchId = 0;
	GameObject audioListener;
	public RolloffMode volumeRolloff;
	public float minDistance = 1;
	public float maxDistance = 500;
	public float panLevel = 0.75F;

	/**
	 * Initialize variables.
	 * Request audio from libpd.
	 **/
	void Start () {
		audioListener = GameObject.FindWithTag ("Player");
		patchId = Camera.main.GetComponent<LibPDMain>().createSound (patchName);
	}
	
	/**
	 * On every frame, if the position of the character has changed,
	 * or the position of the audio source has changed,
	 * calculate new 3D attributes and update Pure Data.
	 **/
	void Update () {
		if (patchId != 0 && CheckForChanges()) {
			const float fullFrequencyRange = 20000;
			const float hrfFactor = 1500;
			const float curveDepth = 3.5F;
			
			Vector3 listenerToSource = transform.position - audioListener.transform.position;
			float angle = Vector3.Angle(audioListener.transform.right, listenerToSource);
			float panLeft = (1 - panLevel) + panLevel * Mathf.Sin(Mathf.Max(180 - angle, 90) * Mathf.Deg2Rad);
			float panRight = (1 - panLevel) + panLevel * Mathf.Sin(Mathf.Max(angle, 90) * Mathf.Deg2Rad);
			
			float behindFactor = 1 + 4 * (Mathf.Clamp(Vector3.Angle(listenerToSource, audioListener.transform.forward), 90, 135) - 90) / (135 - 90);
			float hrfLeft = Mathf.Pow(panLeft, 2) * (fullFrequencyRange - hrfFactor) / behindFactor + hrfFactor;
			float hrfRight = Mathf.Pow(panRight, 2) * (fullFrequencyRange - hrfFactor) / behindFactor + hrfFactor;
			float distance = Vector3.Distance(transform.position, audioListener.transform.position);
			float adjustedDistance = Mathf.Clamp01(Mathf.Max(distance - minDistance, 0) / Mathf.Max(maxDistance - minDistance, 0.001F));
			
			float attenuation;
			if (volumeRolloff == RolloffMode.Linear) {
				attenuation = 1F - adjustedDistance;
			}
			else {
				attenuation = Mathf.Pow((1F - Mathf.Pow(adjustedDistance, 1F / curveDepth)), curveDepth);
			}
			
			SendValue(patchId + "_HRFLeft", hrfLeft);
			SendValue(patchId + "_HRFRight", hrfRight);
			SendValue(patchId + "_PanLeft", panLeft);
			SendValue(patchId + "_PanRight", panRight);
			SendValue(patchId + "_Attenuation", attenuation);
		}
	}
	
	
	/**
	 * Check if either audio source, or audio listener has moved
	 * in this frame.
	 **/
	public bool CheckForChanges() {
		bool changed = false;
		if (gameObject.transform.hasChanged || audioListener.transform.hasChanged) {
			changed = true;
		}
		return changed;
	}

	/**
	 * Normalizes values for Pure Data.
	 * Was needed in some cases at the beginning, 
	 * but for the final project I don't use it.
	 * It's good to be here however.
	 **/
	public bool SendValue(string receiverName, object toSend) {
		int success = -1;
		
		if (toSend is int)
			success = LibPD.SendFloat(receiverName, (float)((int)toSend));
		else if (toSend is int[])
			success = LibPD.SendList(receiverName, ((int[])toSend));
		else if (toSend is float)
			success = LibPD.SendFloat(receiverName, (float)toSend);
		else if (toSend is float[])
			success = LibPD.SendList(receiverName, (float[])toSend);
		else if (toSend is double)
			success = LibPD.SendFloat(receiverName, (float)((double)toSend));
		else if (toSend is double[])
			success = LibPD.SendList(receiverName, ((double[])toSend));
		else if (toSend is bool)
			success = LibPD.SendFloat(receiverName, (float)((bool)toSend).GetHashCode());
		else if (toSend is bool[])
			success = LibPD.SendList(receiverName, ((bool[])toSend));
		else if (toSend is char)
			success = LibPD.SendSymbol(receiverName, ((char)toSend).ToString());
		else if (toSend is char[])
			success = LibPD.SendSymbol(receiverName, new string((char[])toSend));
		else if (toSend is string)
			success = LibPD.SendSymbol(receiverName, (string)toSend);
		else if (toSend is string[])
			success = LibPD.SendList(receiverName, (string[])toSend);
		else if (toSend is System.Enum)
			success = LibPD.SendFloat(receiverName, (float)(toSend.GetHashCode()));
		else if (toSend is System.Enum[])
			success = LibPD.SendList(receiverName, ((System.Enum[])toSend));
		else if (toSend is Vector2)
			success = LibPD.SendList(receiverName, ((Vector2)toSend).x, ((Vector2)toSend).y);
		else if (toSend is Vector3) {
			success = LibPD.SendList(receiverName, new object[] {((Vector3)toSend).x, ((Vector3)toSend).y, ((Vector3)toSend).z});
		}else if (toSend is Vector4)
			success = LibPD.SendList(receiverName, ((Vector4)toSend).x, ((Vector4)toSend).y, ((Vector4)toSend).z, ((Vector4)toSend).w);
		else if (toSend is Quaternion)
			success = LibPD.SendList(receiverName, ((Quaternion)toSend).x, ((Quaternion)toSend).y, ((Quaternion)toSend).z, ((Quaternion)toSend).w);
		else if (toSend is Rect)
			success = LibPD.SendList(receiverName, ((Rect)toSend).x, ((Rect)toSend).y, ((Rect)toSend).width, ((Rect)toSend).height);
		else if (toSend is Bounds)
			success = LibPD.SendList(receiverName, ((Bounds)toSend).center.x, ((Bounds)toSend).center.y, ((Bounds)toSend).size.x, ((Bounds)toSend).size.y);
		else if (toSend is Color)
			success = LibPD.SendList(receiverName, ((Color)toSend).r, ((Color)toSend).g, ((Color)toSend).b, ((Color)toSend).a);
		else {
			Debug.LogError("Invalid type to send to Pure Data: " + toSend);
		}
		return success == 0;
	}
}
