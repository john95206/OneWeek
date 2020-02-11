using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseStageController : MonoBehaviour {

	public GameObject player = null;
	public BaseStageController Instance = null;
	bool instanciated = false;

	public void Initialize(GameObject player, BaseStageController Instance)
	{
		if (this.player != null && this.Instance != null)
		{
			player = this.player;
			Instance = this.Instance;
		}else
		{
			this.player = player;
			this.Instance = Instance;
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if(collision.gameObject == player)
		{
			if (!instanciated)
			{
				CreateInstance();
			}
		}
	}

	void CreateInstance()
	{
		for (int i = 1; i <= 2; i++)
		{
			BaseStageController instanceStage = Instantiate(Instance, transform.parent);
			instanceStage.transform.localPosition = new Vector3(transform.localPosition.x - 5 * i, transform.localPosition.y * i, transform.localPosition.z);
			instanceStage.transform.localRotation = Quaternion.Euler(0, 0, 0);
			instanceStage.Initialize(player, Instance);
			instanciated = true;
		}
	}
}
