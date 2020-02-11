using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using DG.Tweening;

public class BowlingCameraController : MonoBehaviour {

	public class CameraPosition
	{
		public GameState gameState;
		public Vector3 cameraPos;
		public Vector3 cameraRotation;
	}

	public Camera subCamera;
	public float cameraDistance = 50;
	[SerializeField]
	BallController ball = null;
	[SerializeField]
	GameObject kegelManager = null;
	SphereCollider ballCollider = null;
	GameManager gameManager = null;
	public List<CameraPosition> cameraPositionList = new List<CameraPosition>()
	{
		new CameraPosition() { gameState = GameState.Title, cameraPos = new Vector3(0, 2, 140), cameraRotation = new Vector3(36, 0, 0)},
		new CameraPosition() { gameState = GameState.Ready, cameraPos = new Vector3(0, 100, -100), cameraRotation = new Vector3(36, 0, 0) },
		new CameraPosition() { gameState = GameState.Bowling, cameraPos = new Vector3(0, 1, 0), cameraRotation = Vector3.zero },
		new CameraPosition() { gameState = GameState.Result, cameraPos = new Vector3(0, 500, 1000), cameraRotation = Vector3.zero }
	};

	private void Awake()
	{
		gameManager = FindObjectOfType<GameManager>();
	}

	private void Start()
	{
		DOTween.Init();
	}

	void Initialize()
	{
		if (ball)
		{
			ballCollider = ball.GetComponent<SphereCollider>();
		}
	}

	public void GetPlayer(BallController ballCtrl)
	{
		if (ball == null)
		{
			ball = ballCtrl;
		}

		Initialize();
	}

	public void MoveCamera(GameState gameState)
	{
		var nextPos = cameraPositionList.FirstOrDefault(list => list.gameState == gameState);
		transform.DOMove(nextPos.cameraPos, 1.0f);
		transform.DOLocalRotate(nextPos.cameraRotation, 1.0f);
	}

	void FixedUpdate ()
	{
		if (gameManager.gameState != GameState.Bowling && gameManager.gameState != GameState.Ready)
		{
			return;
		}

		if (ball)
		{
			transform.LookAt(ball.transform);

			float ballMargin = -ballCollider.radius * cameraDistance * transform.localScale.y;
			float cameraLimitZ = ball.transform.position.z + ballMargin;
			float cameraLimitY = ball.transform.position.y - ballMargin;

			if (kegelManager)
			{
				cameraLimitZ = Mathf.Clamp(cameraLimitZ, transform.position.z, kegelManager.transform.position.z + ballMargin);
			}

			transform.position = new Vector3(ball.transform.position.x, cameraLimitY, cameraLimitZ);
		}
	}

	public void CameraChange()
	{
		if (ball.launchType == BallLaunchType.EARTH)
		{
			subCamera.gameObject.SetActive(true);
			this.gameObject.SetActive(false);
		}
	}
}