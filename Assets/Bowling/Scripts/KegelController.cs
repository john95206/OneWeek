using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class KegelController : MonoBehaviour {

	[SerializeField]
	AudioSource hitSE;

	private void Start()
	{
		this.UpdateAsObservable()
			.Select(y => transform.position.y)
			.Where(y => y < -15)
			.Subscribe(_ => 
			{
				Destroy(transform.parent.gameObject);
			}).AddTo(gameObject);

		//this.OnCollisionEnterAsObservable()
		//	.Select(x => x.rigidbody != null)
		//	.Where(x => x)
		//	.ThrottleFirst(System.TimeSpan.FromSeconds(0.3f))
		//	.Subscribe(_ =>
		//	{
		//	}).AddTo(gameObject);
	}

	private void OnCollisionEnter(Collision collision)
	{
		if(collision.gameObject.tag == "Player")
		{
			hitSE.Play();
		}
	}
}
