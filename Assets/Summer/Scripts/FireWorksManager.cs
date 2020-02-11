using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine;
using DG.Tweening;
using Zenject;
using UniRx;
using UniRx.Triggers;
using UnityEngine.UI;

namespace Summer
{
	public class FireWorksManager : MonoBehaviour
	{
		[System.Serializable]
		public class InitStatus
		{
			public bool isDebugMode = false;
			/// <summary>
			/// 難易度倍率
			/// </summary>
			public float difficultyRateEasy = 1.0f;
			public float difficultyRateNormal = 1.3f;
			public float difficultyRateHard = 1.8f;
			/// <summary>
			/// 花火の発射インターバル
			/// </summary>
			public float initFireInterval = 1.5f;
			/// <summary>
			/// フィーバー確率。花火が上がるごとに確率が上がる。
			/// </summary>
			public float InitFeverRate = 1.5f;
			public AudioSource endingTheme;
		};

		[System.Serializable]
		public class FieldStatus
		{
			public float nowDifficultyRate = 0.0f;
			public float fireInterval = 0.0f;
			public float nowFeverRate = 0.0f;
		}
		
		public InitStatus gameStatus;
		public PanelTextController textController;
		public FireWork[] FireWorks;
		Camera nowCamera;
		Target target;
		public Color[] flashColor;
		public Text fireRemainText;
		
		IntReactiveProperty fireNum = new IntReactiveProperty(DataMaster.fireWorkMaxNum);

		public bool isFeverTime = false;

		public FieldStatus fieldStatus;

		public FireWork titleFire;

		void Start()
		{
			OpeningFire(titleFire, new Vector2(0, 0), this);
		}

		/// <summary>
		/// OpeningFireのアニメーションから呼び出す
		/// </summary>
		public void Initialize()
		{
			nowCamera = Camera.main;
			target = FindObjectOfType<Target>();
			if (textController)
			{
				textController.Initialize(this);
			}
			Invoke("Init", 2.0f);
		}

		void Init()
		{
			DataMaster.TutorialStart();
			fireNum
				.Subscribe(n =>
				{
					if (n < 0 || DataMaster.gameState == GAME_STATE.Ending)
					{
						return;
					}
					FetchGameState();
#if UNITY_EDITOR
					fireRemainText.text = "終了まで\n" + fireNum.ToString() + " 発";
#endif
				});
			fieldStatus.nowFeverRate = gameStatus.InitFeverRate;
			fieldStatus.nowDifficultyRate = gameStatus.difficultyRateEasy;

			if (FireWorks.Length > 0)
			{
				if (gameStatus.isDebugMode)
				{
					// デバッグモードの時はスペースキーを押したときに花火を発射する
					this.UpdateAsObservable()
						.Select(inputed => Input.GetKeyDown("space") || Input.GetKeyDown("a"))
						.Where(inputed => inputed)
						.Subscribe(_ =>
						{
							FireAfterSetPosition();
						}).AddTo(gameObject);
				}
				else
				{
					StartCoroutine(FireTimeLine());
				}
			}
		}

		void Update()
		{
			CheckRetry();
		}

		/// <summary>
		/// 花火発射の時間管理コルーチン
		/// </summary>
		/// <returns></returns>
		IEnumerator FireTimeLine()
		{
			while (FireEnabled())
			{
				fieldStatus.fireInterval *= Random.Range(0.8f, 1.5f);

				var fireInterval = isFeverTime ? 7.0f : fieldStatus.fireInterval / fieldStatus.nowDifficultyRate;

				yield return new WaitForSeconds(fireInterval);

				FireAfterSetPosition();

				yield return null;
			}

			yield break;
		}

		/// <summary>
		/// デバッグモード中ならスペースキーを押したときに花火発射
		/// 通常モード時なら自動で花火発射
		/// </summary>
		/// <returns></returns>
		bool FireEnabled()
		{
			return IsFireNumRemain() || DataMaster.gameState == GAME_STATE.Ending;
		}

		void FetchGameState()
		{
			// 花火の残り回数に応じて花火の発射間隔を変える
			switch (fireNum.Value)
			{
				case 280:
					DataMaster.gameState = GAME_STATE.Normal;
					fieldStatus.nowDifficultyRate = gameStatus.difficultyRateNormal;
					Debug.Log("Difficult : Normal");
					textController.ForceBufferText("あと " + fireNum + "発");
					break;
				case 100:
					Debug.Log("Difficult : Hard");
					DataMaster.gameState = GAME_STATE.Hard;
					fieldStatus.nowDifficultyRate = gameStatus.difficultyRateHard;
					textController.ForceBufferText("あと " + fireNum + "発");
					break;
			}
		}

		/// <summary>
		/// 花火がまだ残っていれば数を消費してTrueを返す
		/// </summary>
		/// <returns></returns>
		bool IsFireNumRemain()
		{
			if(fireNum.Value > 0)
			{
				return true;
			}

			GameOver(false);
			return false;
		}

		/// <summary>
		/// フィーバーチェックして花火を発射
		/// </summary>
		void FireAfterSetPosition()
		{
			if (!isFeverTime && DataMaster.gameState > GAME_STATE.Tutorial)
			{
				var feverRateRaw = Random.Range(0, 100);
				// 20%の確率で花火フィーバーを起こす
				if (feverRateRaw < fieldStatus.nowFeverRate)
				{
					if (DataMaster.gameState != GAME_STATE.Ending)
					{
						textController.ForceBufferText("フィーバータイム！！！");
					}
					StartCoroutine(FeverCoroutine());
					return;
				}
			}

			// フィーバーじゃなかったらフィーバー率を上げて発射
			fieldStatus.nowFeverRate *= gameStatus.InitFeverRate * fieldStatus.nowDifficultyRate;
			Fire();
		}

		IEnumerator FeverCoroutine()
		{
			// フィーバー中にする
			isFeverTime = true;
			var _fireNum = (int)Random.Range(30, 60);
			
			while (_fireNum > 0)
			{
				--_fireNum;

				yield return new WaitForSeconds(0.1f);

				if (FireEnabled())
				{
					Fire();
				}
				else
				{
					StopAllCoroutines();
					yield break;
				}
			}
			// フィーバー率を初期化
			fieldStatus.nowFeverRate = gameStatus.InitFeverRate;
			isFeverTime = false;
		}

		/// <summary>
		/// 発射位置をランダムで決めて花火を発射する
		/// </summary>
		void Fire()
		{
			var x = Random.Range(-2.5f, 4.5f);
			var y = Random.Range(-4, 0);

			var randomMax = 100.0f;
			var fireIndexRaw = Random.Range(0, randomMax);
			var fireIndex = Mathf.CeilToInt(fireIndexRaw * FireWorks.Length / randomMax) - 1;

			Fire(FireWorks[fireIndex], new Vector2(x, y));
		}

		/// <summary>
		/// オープニング花火を生み出す
		/// </summary>
		public void OpeningFire(FireWork FireWork, Vector2 FirePosition, FireWorksManager manager)
		{
			--fireNum.Value;
			fieldStatus.fireInterval = gameStatus.initFireInterval;
			var Fire = Instantiate(FireWork, FirePosition, Quaternion.identity) as FireWork;
			Fire.OpeningFireInitialize(manager);
		}

		/// <summary>
		/// 花火を生み出す
		/// </summary>
		public void Fire(FireWork FireWork, Vector2 FirePosition)
		{
			--fireNum.Value;
			fieldStatus.fireInterval = gameStatus.initFireInterval;
			var Fire = Instantiate(FireWork, FirePosition, Quaternion.identity) as FireWork;
			Fire.Initialize(Flash);
		}

		/// <summary>
		/// 画面フラッシュ
		/// </summary>
		void Flash()
		{
			if (flashColor.Length > 0)
			{
				var randomMax = 100.0f;
				var flashIndexRaw = Random.Range(0, randomMax);
				var flashIndex = Mathf.CeilToInt(flashIndexRaw * flashColor.Length / randomMax) - 1;
				nowCamera.backgroundColor = flashColor[flashIndex];
				// 画面をフラッシュさせた後黒画面にフェードアウト。終了時にisFlashingフラグをfalseにする
				DOTween.To(
					() => nowCamera.backgroundColor,
					color => nowCamera.backgroundColor = color,
					Color.black,
					DataMaster.fireLifeTime / 2)
					.OnComplete(() =>
					{
						DataMaster.isFlashing = false;
					});
			}

			DataMaster.isFlashing = true;
		}

		public void GameClear()
		{
			DataMaster.GameClear();
			textController.GameClear();
			StartCoroutine(FeverCoroutine());
			gameStatus.endingTheme.Play();
		}

		public void GameOver(bool isRemain)
		{
			if (DataMaster.isGameOver || DataMaster.isGameClear)
			{
				return;
			}

			DataMaster.GameOver();
			StartCoroutine(FeverCoroutine());
			textController.GameOver(isRemain);
			target.GameOverAnim(isRemain);
		}

		void CheckRetry()
		{
			if (Input.GetKeyDown("r"))
			{
				DataMaster.RefreshFlag();
				SceneManager.LoadScene(SceneManager.GetActiveScene().name);
			}

#if UNITY_EDITOR
			if (Input.GetKeyDown("enter"))
			{
				fireNum.Value = 0;
			}
#endif
		}

		public void ForceTextUpdate()
		{
			textController.ForceUpdateText();
		}
	}
}