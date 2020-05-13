using UnityEngine;
using UniRx;
using UniRx.Async;
using UniRx.Triggers;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

namespace Scrum
{
    public class Person : BasePerson
    {
        PersonProvider provider;
        private Rigidbody2D rb2D;
        [SerializeField]
        private AudioSource se;
        /// <summary>
        /// 進む方向＆スピード
        /// </summary>
        [SerializeField]
        private ReactiveProperty<Vector2> direction = new ReactiveProperty<Vector2>(Vector2.zero);
        private float killSelfTime;
        [SerializeField]
        private float velocityDefaultE = 1.0f;
        private float currentVelocityBonus = 1.0f;
        [SerializeField]
        private float sanedVelocityE = 2.0f;
        [SerializeField]
        private float insanedVelocityE = 5.0f;
        [SerializeField]
        public bool isFriendly = false;
        [SerializeField]
        public BoolReactiveProperty isCatastorophy = new BoolReactiveProperty(false);
        [SerializeField]
        private FloatReactiveProperty inMitsuTime = new FloatReactiveProperty(1.0f);
        private BoolReactiveProperty InMitsu = new BoolReactiveProperty(false);

        protected override void Awake()
        {
            base.Awake();
            rb2D = GetComponent<Rigidbody2D>();
        }

        public override void Initialize(PersonProvider provider)
        {
            base.Initialize(provider);
            this.provider = provider;
            killSelfTime = provider.KillTime;
            InMitsu.Subscribe(x =>
            {
                if (!x)
                {
                    currentVelocityBonus = velocityDefaultE;
                    direction.Value = new Vector2(Random.Range(-1, 1), Random.Range(-1, 1));
                }
            }).AddTo(gameObject);

            direction
                .Select(x => x * currentVelocityBonus)
                .Subscribe(dir => rb2D.velocity = dir).AddTo(gameObject);

            DiedAsync.First().Subscribe(async _ =>
            {
                anim.Play("Die");
                provider.people.Remove(this);
                await UniTask.Delay(System.TimeSpan.FromMilliseconds(1100));
                Destroy(gameObject);
            },
            () =>
            {
            }).AddTo(gameObject);

            transform.ObserveEveryValueChanged(t => t.position)
                .Where(p => p.x > 32 || p.x < -32 || p.y > 24 || p.y < -24)
                .Subscribe(_ =>
                {
                    // エリアから出たら、原点(0. 0)あたりにもどる
                    var pos = new Vector2(Random.Range(-20, 20), Random.Range(-10, 10));
                    direction.Value = (pos - (Vector2)transform.position).normalized;
                }).AddTo(gameObject);

            isCatastorophy.Where(x => x).Subscribe(_ => transform.DOLocalMove(Vector3.zero, 10)).AddTo(gameObject);
        }

        protected override void OnMitsu(List<Collider2D> people)
        {
            base.OnMitsu(people);
            // 密である
            inMitsuTime.Value += Time.deltaTime;
            // 密です
            InMitsu.Value = true;

            // 人々の座標を全件取得
            var positions = people.Select(p => p.transform.position);
            // 人々の座標から平均値を割り出し、そこを中心地とする
            var centerPosition = new Vector2(positions.Average(x => x.x), positions.Average(y => y.y));
            // 密ですポイント送信
            provider.MitsuPositionAsync.OnNext(centerPosition);
            if (isCatastorophy.Value)
            {
                currentVelocityBonus = velocityDefaultE * insanedVelocityE;
                direction.Value = centerPosition - (Vector2)transform.position;
            }
            var player = people.FirstOrDefault(p => p.tag == "Player");
            if (player != null)
            {
                // 呼ばれた
                if (player.GetComponent<Player>().IsSayingHelloObservable.Value)
                {
                    // プレイヤーに近づく
                    direction.Value = player.transform.position - transform.position;
                    isFriendly = true;
                    return;
                }
                // 普通にプレイヤーが近くにいる
                else
                {
                    // プレイヤーから逃げる
                    currentVelocityBonus = velocityDefaultE * insanedVelocityE;
                    direction.Value = transform.position - player.transform.position;
                    return;
                }
            }

            if (isFriendly)
            {
                // 人々に近づく
                currentVelocityBonus = velocityDefaultE * insanedVelocityE;
                direction.Value = (centerPosition - (Vector2)transform.position);
                return;
            }
            // 長く密状態であればあるほど逃げる速度が上がる
            currentVelocityBonus = velocityDefaultE * sanedVelocityE * inMitsuTime.Value;
            direction.Value = ((Vector2)transform.position - centerPosition);
        }

        protected override void OnSocialDistance()
        {
            base.OnSocialDistance();
            InMitsu.Value = false;
            inMitsuTime.Value = 1.0f;
        }

        public void Die()
        {
            DiedAsync.OnNext(Unit.Default);
            DiedAsync.OnCompleted();
        }

        public void PlaySe()
        {
            se.PlayOneShot(se.clip);
        }
    }
}