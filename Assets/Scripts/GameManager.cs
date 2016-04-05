using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

	public Camera camera;
	public GameObject cubiePrefab;

	public float cameraSmoothing = 1.0f;

	public int dimensions = 3;

	// Use this for initialization
	void Start () {
		for (int x = 0; x < dimensions; x++)
		{
			for (int y = 0; y < dimensions; y++)
			{
				for (int z = 0; z < dimensions; z++)
				{
					float offset = dimensions / -2.0f + 0.5f;

					Vector3 positionVector = new Vector3(x + offset, y + offset, z + offset);

					GameObject cubieObject = (GameObject)Instantiate(cubiePrefab, positionVector, Quaternion.identity);

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

		Transform cameraTransform = camera.transform;

		cameraTransform.RotateAround(Vector3.zero, Vector3.up, 20 * Time.deltaTime);

		cameraTransform.LookAt(this.transform);
	}
}
