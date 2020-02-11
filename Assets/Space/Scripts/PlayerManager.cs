using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class PlayerManager : MonoBehaviour {

	FloatReactiveProperty StaminaObservale = new FloatReactiveProperty(100);
	BoolReactiveProperty IsStandingByObservable = new BoolReactiveProperty(false);

	[SerializeField]
	GameObject StaminaGauge;
	float staminaMax = 100.0f;

	public float Stamina
	{
		get
		{
			return StaminaObservale.Value;
		}
		set
		{
			if (IsStandBy)
			{
				if (Stamina > 0)
				{
					StaminaObservale.Value = value;
				}
				else
				{
					IsStandBy = false;
				}
			}
		}
	}

	public bool IsStandBy
	{
		get
		{
			return IsStandingByObservable.Value;
		}
		set
		{
			IsStandingByObservable.Value = value;
		}
	}

	// Use this for initialization
	void Start ()
	{
		StaminaObservale.Subscribe(value =>
			{
				StaminaGauge.transform.localScale = new Vector3(value / staminaMax, StaminaGauge.transform.localScale.y, StaminaGauge.transform.localScale.z);
			}).AddTo(gameObject);

		IsStandingByObservable
			.Skip(1)
			.Subscribe(value =>
			{
				if (value)
				{
					Stamina -= Time.fixedDeltaTime * 2;
				}
			}).AddTo(gameObject);
	}
}
