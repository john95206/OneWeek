using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace SpaceGame
{
	public class EnemyAttack : MonoBehaviour
	{
		SpriteRenderer sprite;
		
		void Start()
		{
			sprite = GetComponent<SpriteRenderer>();
		}

		// Use this for initialization
		void OnEnable()
		{
			transform.DOScale(new Vector3(8, 8, 8), 1.5f)
				.OnComplete(
				()=>
				{
					gameObject.SetActive(false);
				});
			DOTween.To(
				() => sprite.color,
				color => sprite.color = color,
				Color.clear,
				1.5f)
				.OnComplete(
				()=> 
				{
					sprite.color = Color.white;
				});
		}

		void OnDisable()
		{
			transform.localScale = new Vector3(1, 1, 1);
		}
	}
}