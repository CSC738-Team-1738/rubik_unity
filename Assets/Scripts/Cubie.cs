using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Cubie : MonoBehaviour {

	public enum Face
	{
		Top,
		Bottom,
		Left,
		Right,
		Front,
		Back,
		None
	};

	public int Axis = 0;
	public int Direction = 0;
	public float DegreesLeft = 0;

	public static Face CurrentFace;

	public static int LockBuffer = 0;

	public void Initialize(Color[] colorArray) {

		Renderer[] faces = {
			transform.Find("Top").GetComponent<Renderer>(),
			transform.Find("Bottom").GetComponent<Renderer>(),
			transform.Find("Left").GetComponent<Renderer>(),
			transform.Find("Right").GetComponent<Renderer>(),
			transform.Find("Front").GetComponent<Renderer>(),
			transform.Find("Back").GetComponent<Renderer>()
		};

		for (int i = 0; i < colorArray.Length; i++)
		{
			if (colorArray[i] == Color.clear)
			{
				faces[i].enabled = false;
				continue;
			}

			faces[i].material.color = colorArray[i];
		}
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
