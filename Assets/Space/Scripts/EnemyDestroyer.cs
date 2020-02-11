using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using System.Linq;
using System;

namespace SpaceGame
{
	public class EnemyDestroyer : MonoBehaviour
	{

		[SerializeField]
		float timeOutTime = 0.2f;
		ReactiveProperty<List<SpaceEnemy>> DetectedEnemyList = new ReactiveProperty<List<SpaceEnemy>>();
		BoolReactiveProperty DestroyEnemiesEnabled = new BoolReactiveProperty(false);
		[SerializeField]
		PlayerManager playerManager;

		// Use this for initialization
		void Start()
		{
			DetectedEnemyList.Value = new List<SpaceEnemy>();
		}

		public void BeginDetectionByCollider()
		{
			var collider = GetComponent<PolygonCollider2D>();

			playerManager.IsStandBy = true;

			IDisposable disposable = null;
			disposable = this.OnTriggerEnter2DAsObservable()
				// 0.2秒間敵が検出されなければ敵探索終了
				.TakeUntil(DetectedEnemyList.Where(x => x.Count() > 0).Timeout(System.TimeSpan.FromSeconds(timeOutTime)))
				.Subscribe(collision =>
				{
					if (collision.gameObject.tag == "Player")
					{
						DestroyEnemiesEnabled.Value = true;
					}

					if (collision.gameObject.tag == "Enemy")
					{
						var enemy = collision.gameObject.GetComponent<SpaceEnemy>();
						if (enemy != null)
						{
							DetectedEnemyList.Value.Add(enemy);
						}
						else
						{
							Debug.Log("SpaceEnemyがアタッチされていない");
						}
					}
				},
				onComplete =>
				{
					if (!DestroyEnemiesEnabled.Value)
					{
						// プレイヤーを囲う円でなければ無効
					}
					else
					{
						// 索敵したリストを誰かに渡して殺してもらう
						DetectedEnemyList.Value.ForEach(enemy => enemy.Die());
					}

					// 初期化
					if (collider != null)
					{
						Destroy(collider);
					}
					DetectedEnemyList.Value.Clear();
					DestroyEnemiesEnabled.Value = false;
					disposable.Dispose();
				}).AddTo(gameObject);
		}
	}
}