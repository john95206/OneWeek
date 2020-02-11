using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceGame
{

	public class SpaceEnemy : MonoBehaviour
	{
		public EnemySpawner spawner;
		Animator anim;
		[SerializeField]
		GameObject slash;
		[SerializeField]
		GameObject attack;

		[SerializeField]
		float maxX;
		[SerializeField]
		float maxY;

		// Use this for initialization
		void Start()
		{
			anim = GetComponent<Animator>();
		}

		// Update is called once per frame
		void OnEnable()
		{
			StartCoroutine(Move());
		}
		
		IEnumerator Move()
		{
			MoveInternal();

			while (gameObject.activeSelf)
			{
				yield return new WaitForSeconds(1.5f);

				MoveInternal();

				Attack();
			}
		}

		void Attack()
		{
			attack.SetActive(true);
		}

		public void Die()
		{
			spawner.spawnedEnemies.Add(gameObject);
			anim.Play("EnemyDie");
			slash.SetActive(true);
		}

		void Dead()
		{
			slash.SetActive(false);
			gameObject.SetActive(false);
		}

		void MoveInternal()
		{
			var newX = Random.Range(-maxX, maxX);
			var newY = Random.Range(-maxY, maxY);

			transform.position = new Vector2(newX, newY);
		}
	}
}