using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

	public GameObject player = null;

	private void FixedUpdate()
	{
		transform.position = new Vector3(player.transform.position.x, player.transform.position.y, transform.position.z);
	}
}