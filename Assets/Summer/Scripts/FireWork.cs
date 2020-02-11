using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Summer;
using System;
using DG.Tweening;

public class FireWork : MonoBehaviour {

	[SerializeField]
	AudioSource burstSE;
	Action onBloom;
	Renderer render;
	Tweener colorTween;
	Tweener raiseTween;

	FireWorksManager manager;

	public void OpeningFireInitialize(FireWorksManager manager = null)
	{
		if (manager != null)
		{
			this.manager = manager;
		}
		render = GetComponent<Renderer>();
		Fire(true);
	}

	public void Initialize(Action onBloom)
	{
		this.onBloom = onBloom;
		render = GetComponent<Renderer>();
		Fire(false);
	}

	// Use this for initialization
	void Fire (bool isOpening)
	{
		var y = isOpening ? 3 : UnityEngine.Random.Range(3, 6);

		// 発火するまで上昇
		raiseTween = transform.DOLocalMoveY(y, 2.5f)
			.SetEase(Ease.Linear)
			.OnComplete(() =>
			{
				if (isOpening)
				{
					raiseTween.Pause();
					colorTween.Pause();
					render.material.color = Color.white;
					GetComponent<Animator>().SetTrigger("Bloom");
				}
				else
				{
					Bloom();
				}
			});

		// 発火前に赤くして発火直前のアピール
		colorTween = render.material.DOColor(Color.red, 0.1f)
			.SetEase(Ease.InOutBounce)
			.SetLoops(-1, LoopType.Restart)
			.SetDelay(1.5f);
	}
	
	/// <summary>
	/// 花火発火アニメーション開始時に呼ぶ。
	/// </summary>
	public void Bloom()
	{
		onBloom();
		raiseTween.Pause();
		colorTween.Pause();
		render.material.color = Color.white;
		GetComponent<Animator>().SetTrigger("Bloom");
		Destroy(this.gameObject, DataMaster.fireLifeTime);
	}

	public void InitializeGame()
	{
		if(manager == null)
		{
			Debug.Log("Manager null");
			return;
		}
		manager.Initialize();
		Destroy(gameObject);
	}

	public void PlayBurstSE()
	{
		burstSE.Play();
	}
}
