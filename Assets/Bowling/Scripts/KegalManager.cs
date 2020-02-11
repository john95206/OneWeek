using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KegalManager : MonoBehaviour {

	[SerializeField]
	float distance = 2.0f;
	[SerializeField]
	int rawNum = 100;
	[SerializeField]
	GameObject kegelPrefab = null;

	void Start ()
	{
		if(kegelPrefab == null)
		{
			Debug.LogError("Prefab Attatch");
			return;
		}

		for(int x = -rawNum / 2; x < rawNum / 2; x++)
		{
			float transformX = x * distance;
			for(int z = 0; z < rawNum; z++)
			{
				float transformZ = z * distance;
				GameObject kegel = Instantiate(kegelPrefab, transform);
				kegel.transform.position = new Vector3(transform.position.x + transformX, transform.position.y, transform.position.z + transformZ);
			}
		}
	}
}
