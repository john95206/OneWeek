using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using DG.Tweening;

namespace Candle
{
    public class LightCore : MonoBehaviour
    {
        private GameManager gameManager;
        Rigidbody2D rb;
        CircleCollider2D col;
        [SerializeField]
        private float speedV = 30.0f;
        private const float initSpeedV = 30.0f;
        [SerializeField]
        private Candle targetCandle;
        [SerializeField]
        private Candle litCandle;
        public System.IObservable<Unit> Initialized => _initialized;
        private Subject<Unit> _initialized = new Subject<Unit>();

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<CircleCollider2D>();
        }

        public void Initalize(GameManager gm)
        {
            this.gameManager = gm;

            col.OnTriggerEnter2DAsObservable()
                .Where(c => c.GetComponent<Candle>() != null)
                .Select(c => c.GetComponent<Candle>())
                .Do(c => c.SwitchIcon(true))
                .Subscribe(c => targetCandle = c);
            col.OnTriggerExit2DAsObservable()
                .Where(col => col.GetComponent<Candle>() != null)
                .Do(c => c.GetComponent<Candle>().SwitchIcon(false))
                .Subscribe(_ => targetCandle = null);

            gameManager.InputProvider.OnHorizontal
                .Select(h => (litCandle?.IsStop.Value ?? true && (!litCandle?.IsAlive.Value ?? false)) ? 0 : h)
                .Select(h => gameManager.OnGameState.Value == GameState.GameOver ? 0 : h)
                .Subscribe(h => rb.velocity = new Vector2(h * speedV, rb.velocity.y));
            gameManager.InputProvider.OnVertical
                .Select(v => (litCandle?.IsStop.Value ?? true && (!litCandle?.IsAlive.Value ?? false)) ? 0 : v)
                .Select(h => gameManager.OnGameState.Value == GameState.GameOver ? 0 : h)
                .Subscribe(v => rb.velocity = new Vector2(rb.velocity.x, v * speedV));

            var fire = gameManager.InputProvider.OnFire
                .Where(_ => targetCandle != null)
                .Share();
            fire.Subscribe(_ => BonfireLit()).AddTo(gameObject);
            fire
                .DelayFrame(10).First()
                .Subscribe(start => 
                {
                    gameManager.ChangeState(GameState.InGame);
                });

            gameManager.OnGameState
                .Where(x => x == GameState.GameOver)
                .First()
                .Subscribe(_ => 
                {
                    litCandle.transform.SetParent(gameManager.candleProvider.transform);
                    transform.DOScale(Vector3.zero, 3);
                    rb.velocity = Vector2.zero;
                });

            _initialized.OnNext(Unit.Default);
        }

        private void BonfireLit()
        {
            litCandle?.OnDeactivate();
            litCandle = targetCandle;
            targetCandle = null;


            transform.position = litCandle.transform.position;
            litCandle.transform.SetParent(transform);

            speedV = initSpeedV;
            litCandle.OnLit();
            litCandle.BodyCount
                .TakeUntil(litCandle.BodyCount.Where(x => x == 0))
                .TakeUntil(litCandle.IsPlayable.Where(x => !x))
                .Subscribe(count =>
                {
                    speedV += 13 / count;
                    //Debug.Log($"Speed: {speedV}");
                });
        }
    }
}