using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class GestureCircleCreater : MonoBehaviour {

	[SerializeField]
	SpriteRenderer playerSprite;

	Vector2 playerPosMin;
	Vector2 playerposMax;
	float depth;
	CircleCollider2D collider;
	
	void Start ()
	{
		depth = -Camera.main.transform.position.z;
		var playerSize = playerSprite.size;
		playerPosMin = new Vector2(-playerSize.x, -playerSize.y);
		playerposMax = new Vector2(playerSize.x, playerSize.y);

		this.OnMouseDownAsObservable()
			.Subscribe(clicked =>
			{
				if(collider != null)
				{
					Destroy(collider);
				}
				System.IDisposable disposable = null;
				disposable = this.ObserveEveryValueChanged(inputed => Input.GetMouseButtonUp(0))
				.Where(x => x)
				.Subscribe(_ =>
				{

					// タップした位置を3次元ベクトルで取得
					Vector3 tapPointRaw = new Vector3(Input.mousePosition.x, Input.mousePosition.y, depth);

					var pos = Camera.main.ScreenToWorldPoint(tapPointRaw);

					if (!IsIntoPlayerSprite(pos))
					{
						var radius = Vector2.Distance(transform.position, pos);

						collider = gameObject.AddComponent<CircleCollider2D>();
						collider.isTrigger = true;
						collider.radius = radius;
					}
					disposable.Dispose();
				}).AddTo(gameObject);
			}).AddTo(gameObject);
	}

	bool IsIntoPlayerSprite(Vector2 clickedPos)
	{
		return (clickedPos.x >= playerPosMin.x && clickedPos.x <= playerposMax.x) &&
				(clickedPos.y >= playerPosMin.y && clickedPos.y <= playerposMax.y);
	}
}
