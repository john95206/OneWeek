using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Summer;
using UniRx;
using UniRx.Triggers;

namespace Summer
{
	public class Target : MonoBehaviour {

		Animator anim;

		void Start()
		{
			anim = GetComponent<Animator>();
		}
		
		void Update()
		{
		}

		public void Move(float moveAmount)
		{
			transform.position = new Vector2(transform.position.x + moveAmount, transform.position.y);
		}

		public void GameOverAnim(bool isRemain)
		{
			if (!isRemain)
			{
				anim.SetBool("Walk", true);
			}
			else
			{
				anim.SetTrigger("Grance");
				anim.SetBool("Run", true);
			}
		}

		public void GameClear()
		{
			anim.Play("Girl_Dance_HeadBang");
		}
	}
}