using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using DG.Tweening;

public enum BallLaunchType
{
	NONE,
	CURVE,
	BOMB,
	EARTH,
}

public class BallController : MonoBehaviour {

	Rigidbody rb = null;
	public BallLaunchType launchType;
	[SerializeField]
	Vector3 addForceVector;
	[SerializeField]
	Vector3 addTorqueVector;
	[SerializeField]
	GameObject explosion;
	GameManager gameManager;
	[SerializeField]
	AudioSource ballSE;
	[SerializeField]
	AudioSource hitSE;
	Vector3 initLocalScale;
	float initMass;
	Vector3 initAddForceVector;

	public BoolReactiveProperty isLaunched = new BoolReactiveProperty(false);

	private void Awake()
	{
		gameManager = FindObjectOfType<GameManager>();
		initLocalScale = transform.localScale;
		initAddForceVector = addForceVector;
	}

	// Use this for initialization
	void Start ()
	{
		rb = GetComponent<Rigidbody>();
		initMass = rb.mass;

		DOTween.Init();

		isLaunched
			.Subscribe(value =>
			{
				if (value)
				{
					gameManager.gameState = GameState.Bowling;

					if(launchType == BallLaunchType.EARTH)
					{
						GetComponentInParent<SpinFree>().spin = false;

						StartCoroutine("ShakeCamera");
					}
				}
			});
	}

	private void FixedUpdate()
	{
		if (launchType == BallLaunchType.CURVE)
		{
			if (isLaunched.Value)
			{
				rb.AddTorque(addTorqueVector * rb.mass * Time.fixedTime);
			}
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if(collision.gameObject.tag == "CanDestroy")
		{
			if (launchType == BallLaunchType.BOMB)
			{
				CameraShake();
				explosion.SetActive(true);
				transform.DetachChildren();
				Destroy(gameObject);
			}
		}
	}

	public void BallStart()
	{
		rb.AddForce(addForceVector * rb.mass);
		isLaunched.Value = true;
		ballSE.Play();
	}

	IEnumerator ShakeCamera()
	{
		while (isLaunched.Value)
		{
			yield return new WaitForSeconds(0.2f);

			CameraShake();
		}
	}

	void CameraShake()
	{
		foreach (Camera c in Camera.allCameras)
		{
			c.DOShakePosition(0.2f, 3, 3, 30, true);
		}
	}

	public void CustomBall()
	{
		int level = (int)Random.Range(0.5f, 3.5f);
		ChangeScale(level);
		level = (int)Random.Range(0.5f, 3.5f);
		ChangeDirection(level);
	}

	void ChangeScale(int level)
	{
		if(level == 1)
		{
			transform.localScale = initLocalScale;
			rb.mass = initMass;
		}
		else if(level == 2)
		{
			transform.localScale = initLocalScale * 2;
			rb.mass = 10 * initMass;
		}
		else
		{
			transform.localScale = initLocalScale * 4;
			rb.mass = 50 * initMass;
		}
	}

	void ChangeDirection(int level)
	{
		if (level == 1)
		{
			addForceVector.x = initAddForceVector.x - 60;
		} else if(level == 2)
		{
			addForceVector.x = initAddForceVector.x;
		}else
		{
			addForceVector.x = initAddForceVector.x + 60;
		}
	}

	void ChangeForce(int level)
	{
		if (level == 1)
		{
			addForceVector.z = initAddForceVector.z;
		}
		else if (level == 2)
		{
			addForceVector.z = initAddForceVector.z * 8;
		}
		else
		{
			addForceVector.z = initAddForceVector.z * 20;
		}
	}
}
