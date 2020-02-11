using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Summer;

namespace Summer
{
	public class Player : MonoBehaviour
	{
		Animator anim;
		private bool isActive = false;

		SpriteRenderer sprite;
		[SerializeField]
		Sprite[] gameOverSprite;

		FireWorksManager manager;

		public bool IsActive
		{
			get
			{
				if (DataMaster.gameState < GAME_STATE.Tutorial ||
					   DataMaster.gameState > GAME_STATE.Hard)
				{
					return false;
				}
				else
				{
					return isActive;
				}
			}

			set
			{
				isActive = value;
			}
		}

		void Start()
		{
			manager = FindObjectOfType<FireWorksManager>();
			sprite = GetComponent<SpriteRenderer>();
			anim = GetComponent<Animator>();
			IsActive = true;
		}
		
		void Update()
		{
			if (IsActive)
			{

				if (DataMaster.gameState <= GAME_STATE.Tutorial)
				{
					if (MoveInputed())
					{
						manager.ForceTextUpdate();
					}
					if(transform.position.x < -0.2f)
					{
						DataMaster.GameStart();
					}
				}

				var moveflag = false;
				if (MoveCheck())
				{
					if (CheckGameOver())
					{
						return;
					}
					moveflag = true;
				}
				else
				{
					moveflag = false;
				}
				anim.SetBool("Moving", moveflag);
			}
		}
		bool MoveInputed()
		{
			return Input.GetKeyDown("left") || Input.GetKeyDown("a");
		}

		bool MoveCheck()
		{
			return Input.GetKey("left") || Input.GetKey("a");
		}

		/// <summary>
		/// アニメーションから呼び出す
		/// </summary>
		public void Move(float moveAmount)
		{
			transform.position = new Vector3(transform.position.x + moveAmount, transform.position.y, transform.position.z);
		}

		bool CheckGameOver()
		{
			if (DataMaster.isFlashing && (anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.5f
				&& anim.GetCurrentAnimatorStateInfo(0).normalizedTime <= 0.9f))
			{
				StartCoroutine(GameOver());
				return true;
			}
			return false;
		}

		void OnTriggerEnter2D(Collider2D collision)
		{
			if (collision)
			{
				manager.GameClear();
				var target = collision.GetComponent<Target>();
				StartCoroutine(GameClear(target));
				IsActive = false;
			}
		}

		IEnumerator GameClear(Target target)
		{
			target.GameClear();

			yield return null;

			anim.Play("Boy_Dance_headBang");
		}

		IEnumerator GameOver()
		{
			if(DataMaster.gameState <= GAME_STATE.Tutorial)
			{
				yield break;
			}
			manager.GameOver(true);
			IsActive = false;
#if UNITY_EDITOR
			Debug.Log("Game Over");
#endif
			anim.speed = 0;

			yield return new WaitForSeconds(0.8f);

			anim.speed = 1.0f;
			anim.SetTrigger("GameOver");
		}
	}
}
