using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UniRx;
using UniRx.Triggers;
using UnityEngine.UI;
using Zenject;

public enum GameState
{
	Title,
	Ready,
	Bowling,
	Result,
}

public class GameManager : MonoBehaviour {

	public static bool curveUnlocked = false;
	public static bool bombUnLocked = false;
	public static bool earthUnlocked = false;
	public static int highScore = 0;
	public float limitTime = 10;
	public IntReactiveProperty aliveNum = new IntReactiveProperty(0);
	public LayerMask targetLayer;
	public Text scoreText;
	public GameState gameState = GameState.Title;
	[Inject][SerializeField]
	UiManager uiManager;
	BowlingCameraController cameraCtrl;

	private void Start()
	{
		cameraCtrl = Camera.main.GetComponent<BowlingCameraController>();

		var stateObservable = this.UpdateAsObservable()
			.Select(state => gameState)
			.DistinctUntilChanged()
			.Publish()
			.RefCount();

		stateObservable
			.Subscribe(_ => 
			{
				cameraCtrl.MoveCamera(gameState);
			}).AddTo(gameObject);

		stateObservable
			.Where(state => state == GameState.Bowling)
			.Subscribe(_ =>
			{
				GameInitialize();
			}).AddTo(gameObject);
	}

	void GameInitialize()
	{
		aliveNum
			.Timeout(System.TimeSpan.FromSeconds(limitTime))
			.Subscribe
			(
			value =>
			{
				int nowScore = 900 - value;
				scoreText.text = nowScore.ToString();
				if(nowScore > highScore)
				{
					highScore = nowScore;
				}
			},
			onError =>
			{
				gameState = GameState.Result;
				uiManager.ResultUiInitialize();
			}).AddTo(gameObject);
	}

	private void OnTriggerEnter(Collider other)
	{
		aliveNum.Value++;
	}

	private void OnTriggerExit(Collider other)
	{
		aliveNum.Value--;
	}

	public void Restart()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

	public void GameDelete()
	{
		highScore = 0;
		curveUnlocked = false;
		bombUnLocked = false;
		earthUnlocked = false;
	}
}
