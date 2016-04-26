using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

	public Camera mainCamera;
	public GameObject cubiePrefab;
	public Text shuffleCountText;

	public float cameraSmoothing = 1.0f;

	public int dimensions = 3;

	public float rotationSpeed = 200f;

	private List<GameObject> cubies;
	private List<Vector3> cubiePositions;

	private Queue<KeyValuePair<Cubie.Face, int>> moveQueue;

	// Use this for initialization
	void Start () {
		cubies = new List<GameObject>();
		cubiePositions = new List<Vector3>();
		moveQueue = new Queue<KeyValuePair<Cubie.Face, int>>();

		for (int x = 0; x < dimensions; x++)
		{
			for (int y = 0; y < dimensions; y++)
			{
				for (int z = 0; z < dimensions; z++)
				{
					float offset = dimensions / -2.0f + 0.5f;

					Vector3 positionVector = new Vector3(x + offset, y + offset, z + offset);

					GameObject cubieObject = (GameObject)Instantiate(cubiePrefab, positionVector, Quaternion.identity);

					cubies.Add(cubieObject);

					offset = (int)(offset - .5f);

					cubiePositions.Add(new Vector3(x + offset, y + offset, z + offset));

					Cubie cubie = cubieObject.GetComponent<Cubie>();

					Color[] colors = new Color[6];

					if (x == 0)
					{
						colors[(int)Cubie.Face.Left] = Color.white;
					}
					else if (x == dimensions - 1)
					{
						colors[(int)Cubie.Face.Right] = Color.yellow;
					}

					if (y == 0)
					{
						colors[(int)Cubie.Face.Bottom] = Color.green;
					}
					else if (y == dimensions - 1)
					{
						colors[(int)Cubie.Face.Top] = Color.blue;
					}

					if (z == 0)
					{
						colors[(int)Cubie.Face.Front] = Color.red;
					}
					else if (z == dimensions - 1)
					{
						colors[(int)Cubie.Face.Back] = new Color(0.94F, 0.49F, 0.16F) ;//orange
					}

					cubie.Initialize(colors);
				}
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		ProcessControls();

		ProcessMoves();

		Transform cameraTransform = mainCamera.transform;

		//cameraTransform.RotateAround(Vector3.zero, Vector3.up, 20 * Time.deltaTime);

		cameraTransform.LookAt(this.transform);
	}

	void ProcessControls() {
		if (Cubie.LockBuffer > 0) {
			return;
		}

		int direction = (Input.GetButton("Reverse")) ? -1 : 1;

		if (Input.GetButtonUp("Top")) {
			RotateFace(Cubie.Face.Top, direction);
		} else if (Input.GetButtonUp("Bottom")) {
			RotateFace(Cubie.Face.Bottom, direction);
		} else if (Input.GetButtonUp("Front")) {
			RotateFace(Cubie.Face.Front, direction);
		} else if (Input.GetButtonUp("Back")) {
			RotateFace(Cubie.Face.Back, direction);
		} else if (Input.GetButtonUp("Left")) {
			RotateFace(Cubie.Face.Left, direction);
		} else if (Input.GetButtonUp("Right")) {
			RotateFace(Cubie.Face.Right, direction);
		}
	}

	void ProcessMoves() {
		if (Cubie.LockBuffer == 0) {
			if (moveQueue.Count == 0) {
				return;
			}

			KeyValuePair<Cubie.Face, int> move = moveQueue.Dequeue();

			Debug.Log("moves in queue: " + moveQueue.Count);

			RotateFace(move.Key, move.Value);
			//Cubie.LockBuffer++; // Uncommenting this line prevents future moves from being dequeued.

		}

		for (int i = 0; i < cubies.Count; i++) { 
			Cubie currentCubie = cubies[i].GetComponent<Cubie>();

			if (currentCubie.DegreesLeft == 0) {
				continue;
			}

			Vector3 axisVector = Vector3.zero;
			axisVector[currentCubie.Axis] = 1;

			float angle = Time.deltaTime * currentCubie.Direction * rotationSpeed;

			if (currentCubie.DegreesLeft - Mathf.Abs(angle) < 0)
			{
				int direction = angle < 0 ? -1 : 1;

				angle = currentCubie.DegreesLeft * direction;
			}
			cubies[i].transform.RotateAround(Vector3.zero, axisVector, angle);

			currentCubie.DegreesLeft -= Mathf.Abs(angle);

			if (currentCubie.DegreesLeft == 0) {
				Cubie.LockBuffer--;
			}
		}
	}

	public void Shuffle(int n) {
		if (n == 0) {
			n = Int32.Parse(shuffleCountText.text.ToString());
		}

		for (int i = 0; i < n; i++)
		{
			Cubie.Face face = (Cubie.Face)UnityEngine.Random.Range(0, (int)Cubie.Face.None);
			int direction = UnityEngine.Random.Range(0, 2) * 2 - 1;

			moveQueue.Enqueue(new KeyValuePair<Cubie.Face, int>(face, direction));
		}
	}

	void RotateFace (Cubie.Face face, int direction, int layer = -1) {
		int dimension = 0, offset = 0;

		switch (face) {
		case Cubie.Face.Front:
			dimension = 2;
			offset = -1;
			break;
		case Cubie.Face.Back:
			dimension = 2;
			offset = 1;
			break;
		case Cubie.Face.Bottom:
			dimension = 1;
			offset = -1;
			break;
		case Cubie.Face.Top:
			dimension = 1;
			offset = 1;
			break;
		case Cubie.Face.Left:
			dimension = 0;
			offset = -1;
			break;
		case Cubie.Face.Right:
			dimension = 0;
			offset = 1;
			break;
		}

		if (layer == -1) {
			layer = dimensions / 2;
		}

		offset *= layer;

		for (int i = 0; i < cubiePositions.Count; i++) {
			Vector3 cubiePosition = cubiePositions[i];

			if (cubiePosition[dimension] == offset && (Cubie.CurrentFace == face || Cubie.LockBuffer == 0))
			{
				float sin = Mathf.Sin(Mathf.PI / 2);
				float cos = Mathf.Cos(Mathf.PI / 2);

				switch (dimension) {
				case 0:
					cubiePositions[i] = new Vector3(
						cubiePosition.x, 
						Mathf.Round(direction * -offset * (cubiePosition.y * cos + cubiePosition.z * sin)), 
						Mathf.Round(direction * -offset * (cubiePosition.z * cos - cubiePosition.y * sin)));
					break;
				case 1:
					cubiePositions[i] = new Vector3(
						Mathf.Round(direction * offset * (cubiePosition.x * cos + cubiePosition.z * sin)), 
						cubiePosition.y, 
						Mathf.Round(direction * offset * (cubiePosition.z * cos - cubiePosition.x * sin)));
					break;
				case 2:
					cubiePositions[i] = new Vector3(
						Mathf.Round(direction * -offset * (cubiePosition.x * cos + cubiePosition.y * sin)), 
						Mathf.Round(direction * -offset * (cubiePosition.y * cos - cubiePosition.x * sin)), 
						cubiePosition.z);
					break;
				}

				GameObject cubieObject = cubies[i];
				Cubie currentCubie = cubieObject.GetComponent<Cubie>();

				currentCubie.Axis = dimension;
				currentCubie.Direction = direction * offset;
				currentCubie.DegreesLeft += 90;

				Cubie.CurrentFace = face;
				Cubie.LockBuffer++;
			}

		}
	}
}
