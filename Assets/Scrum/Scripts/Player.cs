using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Async;
using UniRx.Triggers;

namespace Scrum
{
    public class Player : BasePerson
    {
        [SerializeField]
        private FloatReactiveProperty _hpObservable = new FloatReactiveProperty();
        public IReadOnlyReactiveProperty<float> Hp => _hpObservable;
        [SerializeField]
        private float maxHp = 100.0f;
        [SerializeField]
        private float healHp = 2.0f;
        [SerializeField]
        private float lossF = 2.0f;
        [SerializeField]
        private BoolReactiveProperty isSayingHelloObservable = new BoolReactiveProperty(false);
        public IReadOnlyReactiveProperty<bool> IsSayingHelloObservable => isSayingHelloObservable;
        [SerializeField]
        private float sayHelloInterval = 6.0f;
        [SerializeField]
        private float sayHelloRadius = 1.0f;
        [SerializeField]
        public float weakedHp = 50.0f;
        [SerializeField]
        private GameObject healingBG;
        [SerializeField]
        private AudioSource se;

        private void Start()
        {
            _hpObservable.Value = maxHp * 0.7f;
            _hpObservable
                .TakeUntil(_hpObservable.Where(hp => hp <= 0))
                .Subscribe(hp =>
                {
                    var colorSeed = hp * 0.01f;
                    renderer.color = new Color(1, 1, 1, colorSeed);
                },
                () => 
                {
                    DiedAsync.OnNext(Unit.Default);
                    DiedAsync.OnCompleted();
                    Debug.Log("Dead");
                }).AddTo(gameObject);
            _hpObservable.First(x => x < weakedHp).Subscribe(x => ResetAnim());

            this.UpdateAsObservable()
                .TakeUntil(_hpObservable.Where(hp => hp <= 0))
                .Where(_ => !isSayingHelloObservable.Value)
                .Select(_ => Input.GetMouseButtonDown(0)).Where(x => x)
                .Subscribe(_ =>
                {
                    SayHello();
                }).AddTo(gameObject);
        }

        private async void SayHello()
        {
            se.PlayOneShot(se.clip);
            isSayingHelloObservable.Value = true;
            anim.Play("Hello");
            var r = collider2D.radius;
            collider2D.radius = sayHelloRadius;
            await UniTask.Delay(System.TimeSpan.FromSeconds(sayHelloInterval));
            collider2D.radius = r;
            isSayingHelloObservable.Value = false;
            ResetAnim();
        }

        private void Update()
        {
            var cursorPosition = Input.mousePosition;
            cursorPosition -= Vector3.forward * Camera.main.transform.position.z;
            transform.position = Camera.main.ScreenToWorldPoint(cursorPosition) - Vector3.down;

            _hpObservable.Value -= Time.deltaTime * lossF;
        }

        protected override void OnMitsu(List<Collider2D> people)
        {
            base.OnMitsu(people);
            if(_hpObservable.Value < 1)
            {
                return;
            }
            healingBG.SetActive(true);
            _hpObservable.Value += healHp;
            _hpObservable.Value = Mathf.Clamp(_hpObservable.Value, 0, maxHp);
        }

        protected override void OnSocialDistance()
        {
            base.OnSocialDistance();
            healingBG.SetActive(false);
        }

        private void ResetAnim()
        {
            if(_hpObservable.Value < weakedHp)
            {
                anim.Play("Weaked");
            }
            else
            {
                anim.Play("Idle");
            }
        }
    }
}