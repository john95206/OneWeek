using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System.Linq;

namespace SpaceGame
{
	public class EnemySpawner : MonoBehaviour
	{
		[SerializeField]
		GameObject enemyPrefab;

		[SerializeField]
		public List<GameObject> spawnedEnemies;

		public FloatReactiveProperty waitTime = new FloatReactiveProperty(2);

		[SerializeField]
		int prePrevSpawnNum = 0;
		[SerializeField]
		int preSpawnNum = 0;
		public IntReactiveProperty spawnNum = new IntReactiveProperty(1);
		public IntReactiveProperty maxEnemyNum = new IntReactiveProperty(100);
		List<GameObject> Enemies = new List<GameObject>();

		// Use this for initialization
		void Start()
		{
			spawnNum
				.Subscribe(num =>
				{
				}).AddTo(gameObject);

			Initialize();
		}

		void Initialize()
		{
			StartCoroutine(SpawnCoroutine());
		}

		void SpawnEnemy(int SpawnNum)
		{
			if (SpawnNum > maxEnemyNum.Value)
			{
				SpawnNum = maxEnemyNum.Value;
				spawnNum.Value = SpawnNum;

				preSpawnNum = 89;
				prePrevSpawnNum = 55;
			}

			// 死んでオブジェクトプールになっている敵を全部取得して配列化
			var disActiveEnemyList = spawnedEnemies.FindAll(enemy => !enemy.activeSelf).ToArray();
			// オブジェクトプールがあれば生成数内でできるだけ復活させる
			for(int i = 0; i < SpawnNum; i++)
			{
				// オブジェクトプールの数を生成数が上回ってしまった場合、新規に敵を生成する
				if (i > disActiveEnemyList.Length - 1)
				{
					CreateEnemy();
					continue;
				}
				CreateEnemy(disActiveEnemyList[i]);
			}
			spawnNum.Value = prePrevSpawnNum + preSpawnNum;
			if (preSpawnNum + prePrevSpawnNum == 0)
			{
				spawnNum.Value = 1;
			}
			prePrevSpawnNum = preSpawnNum;
			preSpawnNum = spawnNum.Value;
			Debug.Log("Spawned " + SpawnNum);
			Debug.Log("Now " + transform.childCount + "Enemies");
		}

		void CreateEnemy(GameObject enemy = null)
		{
			if (enemy == null)
			{
				var e =Instantiate(enemyPrefab);
				e.GetComponent<SpaceEnemy>().spawner = this;
				e.transform.parent = transform;
				Enemies.Add(e);
			}
			else
			{
				enemy.SetActive(true);
			}
		}

		IEnumerator SpawnCoroutine()
		{
			while (true)
			{
				yield return new WaitForSeconds(waitTime.Value);

				if (Enemies.Count(enemy => enemy.activeSelf) < maxEnemyNum.Value)
				{
					SpawnEnemy(spawnNum.Value);
				}
				else
				{
					Debug.Log("Too many enemies to spawn");
				}
			}
		}
	}
}