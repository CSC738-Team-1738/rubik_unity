using UnityEngine;
using System.Collections;

public class Cubie : MonoBehaviour {

	public Cubie(Color faceAColor, Color faceBColor, Color faceCColor) {
		Material materialA, materialB, materialC;

		materialA = GameObject.Find("FaceA").GetComponent<Renderer>().material;
		materialB = GameObject.Find("FaceB").GetComponent<Renderer>().material;
		materialC = GameObject.Find("FaceC").GetComponent<Renderer>().material;

		materialA.color = faceAColor;
		materialB.color = faceBColor;
		materialA.color = faceCColor;
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
