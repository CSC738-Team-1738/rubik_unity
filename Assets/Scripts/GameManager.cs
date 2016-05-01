using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class GameManager : MonoBehaviour {

	public Camera mainCamera;
	public GameObject cubiePrefab;
	public Text shuffleCountText;

	public float cameraVerticalBound = 5;
	public float cameraHorizontalBound = 10;

	public float cameraSmoothing = 1.0f;

	public int dimensions = 3;

	public float rotationSpeed = 200f;

	private List<GameObject> cubies;
	private List<Vector3> cubiePositions;

	private Queue<KeyValuePair<Cubie.Face, int>> moveQueue;

	private int[] currentState = {
		0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
	};

	private string[] goal = {"UF", "UR", "UB", "UL", "DF", "DR", "DB", "DL", "FR", "FL", "BR", "BL", "UFR", "URB", "UBL", "ULF", "DRF", "DFL", "DLB", "DBR"};

	private int[,] affectedCubies = new int[6, 8] {
		{0,  1,  2,  3,  0,  1,  2,  3},
		{4,  7,  6,  5,  4,  5,  6,  7},
		{0,  9,  4,  8,  0,  3,  5,  4},
		{2, 10,  6, 11,  2,  1,  7,  6},
		{3, 11,  7,  9,  3,  2,  6,  5},
		{1,  8,  5, 10,  1,  0,  4,  7}
	};

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
		ProcessCamera();

		ProcessControls();

		ProcessMoves();
	}

	void ProcessCamera() {
		float percentX = Input.mousePosition.x / Screen.width;
		float percentY = Input.mousePosition.y / Screen.height;

		float cameraX = (-1 + 2 * percentX) * cameraHorizontalBound;
		float cameraY = (-1 + 2 * percentY) * cameraVerticalBound;

		Transform cameraTransform = mainCamera.transform;

		float cameraZ = cameraTransform.position.z;

		if (Input.GetButton("BackCamera")) {
			cameraZ = Math.Abs(cameraZ);
		} else {
			cameraZ = -1 * Math.Abs(cameraZ);
		}

		Vector3 targetPosition = new Vector3(cameraX, cameraY, cameraTransform.position.z);

		float smoothing = Time.deltaTime * Vector3.Distance(cameraTransform.position, targetPosition);

		Vector3 intermediateVector = Vector3.Lerp(cameraTransform.position, targetPosition, smoothing);

		intermediateVector.z = cameraZ;

		cameraTransform.position = intermediateVector;

		cameraTransform.LookAt(this.transform);
	}

	void ProcessControls() {

		int direction = (Input.GetButton("Reverse")) ? -1 : 1;

		if (Input.GetButtonUp("Top")) {
			QueueMove(Cubie.Face.Top, direction);
		} else if (Input.GetButtonUp("Bottom")) {
			QueueMove(Cubie.Face.Bottom, direction);
		} else if (Input.GetButtonUp("Front")) {
			QueueMove(Cubie.Face.Front, direction);
		} else if (Input.GetButtonUp("Back")) {
			QueueMove(Cubie.Face.Back, direction);
		} else if (Input.GetButtonUp("Left")) {
			QueueMove(Cubie.Face.Left, direction);
		} else if (Input.GetButtonUp("Right")) {
			QueueMove(Cubie.Face.Right, direction);
		}
	}

	void ProcessMoves() {
		if (Cubie.LockBuffer == 0) {
			if (moveQueue.Count == 0) {
				return;
			}

			KeyValuePair<Cubie.Face, int> move = moveQueue.Dequeue();

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
			try {
				n = Int32.Parse(shuffleCountText.text.ToString());
			} catch (FormatException e) {
				Debug.Log(e);
				n = 20;
			}
		}

		for (int i = 0; i < n; i++)
		{
			Cubie.Face face = (Cubie.Face)UnityEngine.Random.Range(0, (int)Cubie.Face.None);
			int direction = UnityEngine.Random.Range(0, 2) * 2 - 1;

			QueueMove(face, direction);
		}
	}

	void QueueMove(Cubie.Face face, int direction) {
		moveQueue.Enqueue(new KeyValuePair<Cubie.Face, int>(face, direction));
	}

	void RotateFace (Cubie.Face face, int direction, int layer = -1) {
		int dimension = 0, offset = 0, faceNum = 0;

		switch (face) {
		case Cubie.Face.Front:
			dimension = 2;
			offset = -1;
			faceNum = 2;
			break;
		case Cubie.Face.Back:
			dimension = 2;
			offset = 1;
			faceNum = 3;
			break;
		case Cubie.Face.Bottom:
			dimension = 1;
			offset = -1;
			faceNum = 1;
			break;
		case Cubie.Face.Top:
			dimension = 1;
			offset = 1;
			faceNum = 0;
			break;
		case Cubie.Face.Left:
			dimension = 0;
			offset = -1;
			faceNum = 4;
			break;
		case Cubie.Face.Right:
			dimension = 0;
			offset = 1;
			faceNum = 5;
			break;
		}

		if (layer == -1) {
			layer = dimensions / 2;
		}

		offset *= layer;

		int turns = (direction < 0) ? 2 : 0;
		ApplyMove(faceNum * 3 + turns);

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

	public void Solve () {
		string[] state = new string[20];

		for (int i = 0; i < 20; i++) {
			string s = goal[currentState[i]];

			for (int j = 0; j < currentState[i+20]; j++) {
				if (s.Length == 2) {
					s = s.Substring(1, 1) + s.Substring(0, 1);
				} else {
					s = s.Substring(2, 1) + s.Substring(0, 2);
				}
			}

			state[i] = s;
		}

		string stateString = string.Join(",", state);

		string url = "http://localhost:8888/cube/solver/" + stateString;

		WWW www = new WWW(url);

		StartCoroutine(WaitForRequest(www));
	}

	IEnumerator WaitForRequest(WWW www) {
		yield return www;

		if (www.error == null) {
			ServerResponse serverResponse = new ServerResponse();

			serverResponse = JsonUtility.FromJson<ServerResponse>(www.text);

			string[] moveList = serverResponse.solution.Split(new string[] {","}, StringSplitOptions.None);

			foreach (string move in moveList) {
				Cubie.Face face = Cubie.Face.Top;
				int direction = 1;
				string faceString = "", directionString = "";

				string pattern = @"([A-Z])";
				MatchCollection matches = Regex.Matches(move, pattern);

				if (matches.Count > 0 && matches[0].Groups.Count > 1) {
					faceString = matches[0].Value;
				}

				pattern = @"([^A-Z])";
				matches = Regex.Matches(move, pattern);

				if (matches.Count > 0 && matches[0].Groups.Count > 1) {
					directionString = matches[0].Value;
				}

				switch (faceString) {
				case "U": 
					face = Cubie.Face.Top;
					break;
				case "D":
					face = Cubie.Face.Bottom;
					break;
				case "F":
					face = Cubie.Face.Front;
					break;
				case "B":
					face = Cubie.Face.Back;
					break;
				case "L":
					face = Cubie.Face.Left;
					break;
				case "R":
					face = Cubie.Face.Right;
					break;
				default:
					break;
				}

				if (directionString == "'") {
					direction = -1;
				} else {
					direction = 1;
				}

				QueueMove(face, direction);

				if (directionString == "2") {
					QueueMove(face, direction);
				}
			}
		} else {
			Debug.Log("WWW Error: " + www.error);
		}
	}

	void ApplyMove(int move) {
		int turns = move % 3 + 1;
		int face = move / 3;

		while( turns-- != 0 ){
			int[] oldState = new int[40];
			currentState.CopyTo(oldState, 0);

			for(int i=0; i<8; i++){
				int isCorner = (i > 3) ? 1 : 0;
				int target = affectedCubies[face, i] + isCorner*12;
				int killer = affectedCubies[face, (i & 3) == 3 ? i - 3 : i + 1] + isCorner*12;

				int orientationDelta = 0;
				if (i < 4) {
					orientationDelta = (face > 1 && face < 4) ? 1 : 0;
				} else if (face < 2) { 
					orientationDelta = 0;
				} else {
					orientationDelta = 2 - (i & 1);
				}				

				currentState[target] = oldState[killer];
				currentState[target + 20] = oldState[killer + 20] + orientationDelta;

				if(turns == 0)
					currentState[target + 20] %= 2 + isCorner;
			}
		}


		Debug.Log(currentState);
	}
}
