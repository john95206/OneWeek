using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using UniRx;
using UniRx.Triggers;
using DG.Tweening;

public class UiManager : MonoBehaviour {

	[SerializeField]
	GameObject[] balls;
	[SerializeField]
	Button[] ballButtons;

	[Inject][SerializeField]
	GameManager gameManger;
	[SerializeField]
	GameObject title;
	[SerializeField]
	Button titleStartButton;
	[SerializeField]
	GameObject readyUi;
	[SerializeField]
	Button readyButton;
	[SerializeField]
	Text countText;
	[SerializeField]
	Text resultText;
	[SerializeField]
	Text highScoreText;
	[SerializeField]
	Button retryButton;
	[SerializeField]
	Button speedUpButton;
	[SerializeField]
	Button ballSelectButton;
	[SerializeField]
	GameObject ballButtonsGo;
	[SerializeField]
	Button customizeButton;

	BallController nowBall;

	private void Start()
	{
		TitleInitialize();
	}

	public void TitleInitialize()
	{
		title.SetActive(true);

		var onClickTitle = titleStartButton.OnClickAsObservable()
			.FirstOrDefault()
			.Subscribe(_ =>
			{
				gameManger.gameState = GameState.Ready;
				ReadyUiInitialize();
			}).AddTo(title);

		var onClickReady = readyButton.OnClickAsObservable()
			.FirstOrDefault()
			.Subscribe(_ =>
			{
				if(nowBall == null)
				{
					Debug.LogError("ボールがないのにスタートボタンが押された");
				}
				nowBall.BallStart();
				gameManger.gameState = GameState.Bowling;
			}).AddTo(title);

		ballSelectButton.OnClickAsObservable()
			.Subscribe(_ =>
			{
				ballButtonsGo.transform.DOLocalMoveY(0, 0.3f);
				customizeButton.interactable = true;
			}).AddTo(gameObject);

		customizeButton.OnClickAsObservable()
			.Subscribe(_ =>
			{
				OnCustomButton();
			}).AddTo(gameObject);
	}

	public void ReadyUiInitialize()
	{
		title.SetActive(false);
		readyUi.SetActive(true);

		ballButtons[1].interactable = GameManager.curveUnlocked;
		ballButtons[2].interactable = GameManager.bombUnLocked;
		ballButtons[3].interactable = GameManager.earthUnlocked;

		var onClickReady = readyButton.OnClickAsObservable()
			.FirstOrDefault()
			.Subscribe(_ =>
			{
				speedUpButton.gameObject.SetActive(true);
				gameManger.gameState = GameState.Bowling;
				BowlingUiInitialize();
				Camera.main.GetComponent<BowlingCameraController>().CameraChange();
			}).AddTo(readyUi);
	}

	public void BowlingUiInitialize()
	{
		readyUi.SetActive(false);
		countText.gameObject.SetActive(true);
	}

	public void OnBallButton(int ballType)
	{
		GameObject ball = null;
		BallController ballCtrl = null;

		switch (ballType)
		{
			case (int)BallLaunchType.NONE:
				if(nowBall != null)
				{
					if(nowBall.launchType == BallLaunchType.NONE)
					{
						break;
					}
					DestroyImmediate(nowBall.gameObject);
				}
				ball = Instantiate(balls[0]);
				ballCtrl = ball.GetComponent<BallController>();
				nowBall = ballCtrl;
				break;
			case (int)BallLaunchType.CURVE:
				if (nowBall != null)
				{
					if (nowBall.launchType == BallLaunchType.CURVE)
					{
						break;
					}
					DestroyImmediate(nowBall.gameObject);
				}
				ball = Instantiate(balls[1]);
				ballCtrl = ball.GetComponent<BallController>();
				nowBall = ballCtrl;
				break;
			case (int)BallLaunchType.BOMB:
				if (nowBall != null)
				{
					if (nowBall.launchType == BallLaunchType.BOMB)
					{
						break;
					}
					DestroyImmediate(nowBall.gameObject);
				}
				ball = Instantiate(balls[2]);
				ballCtrl = ball.GetComponent<BallController>();
				nowBall = ballCtrl;
				break;
			case (int)BallLaunchType.EARTH:
				if (nowBall != null)
				{
					if (nowBall.launchType == BallLaunchType.EARTH)
					{
						break;
					}
					DestroyImmediate(nowBall.gameObject);
				}
				ball = Instantiate(balls[3]);
				ballCtrl = ball.GetComponentInChildren<BallController>();
				nowBall = ballCtrl;
				break;
		}

		Camera.main.GetComponent<BowlingCameraController>().GetPlayer(nowBall);
		readyButton.interactable = nowBall != null ? true : false;
	}

	public void ResultUiInitialize()
	{
		if (GameManager.earthUnlocked && nowBall.launchType == BallLaunchType.EARTH)
		{
			retryButton.GetComponentInChildren<Text>().text = "The End";
		}
		Time.timeScale = 1;
		highScoreText.text = "HighScore: " + GameManager.highScore.ToString();
		highScoreText.gameObject.SetActive(true);
		resultText.gameObject.SetActive(true);
		retryButton.gameObject.SetActive(gameObject);
		retryButton.OnClickAsObservable()
			.FirstOrDefault()
			.Subscribe(_ =>
			{
				if (GameManager.earthUnlocked && nowBall.launchType == BallLaunchType.EARTH)
				{
					gameManger.GameDelete();
				}

				GameManager.curveUnlocked = true;
				if (GameManager.highScore > 9)
				{
					GameManager.bombUnLocked = true;
				}
				if (GameManager.highScore > 500)
				{
					GameManager.earthUnlocked = true;
				}
				gameManger.Restart();
			});
	}

	public void OnSpeedUpButton()
	{
		if(Time.timeScale < 2)
		{
			Time.timeScale *= 5;
		}
	}

	public void OnCustomButton()
	{
		nowBall.CustomBall();
	}
}
