using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using UniRx;

namespace Beyond
{
    public class Rocket : MonoBehaviour
    {
        /// <summary>
        /// 0~100
        /// </summary>
        public IReadOnlyReactiveProperty<float> Height => height;
        private FloatReactiveProperty height = new FloatReactiveProperty();
        public Subject<bool> Goaled = new Subject<bool>();
        private FloatReactiveProperty speed = new FloatReactiveProperty();
        [SerializeField]
        private GameObject BG;
        [SerializeField]
        private GameObject sun;
        private Rigidbody2D bgRb2D;
        private Rigidbody2D rb2D;
        private BoxCollider2D boxCol;
        [SerializeField]
        float force = 25;
        const float maxHeight = 3500;

        private Animator anim;

        private void Awake()
        {
            anim = GetComponent<Animator>();
            rb2D = GetComponent<Rigidbody2D>();
            bgRb2D = BG.GetComponent<Rigidbody2D>();
        }

        private void Start()
        {
            height
                .Where(h => h >= 30.0f)
                .First()
                .Subscribe(h =>
                {
                    // TakeOff
                    PlayAnimation("TakeOff");
                }).AddTo(gameObject);

            BG.transform.ObserveEveryValueChanged(t => t.position)
                .Select(p => p.y)
                // 100分率にする
                .Select(y => Mathf.InverseLerp(0, maxHeight, -y) * 100)
                //.Do(h => Debug.Log(h))
                .Subscribe(y => height.Value = y)
                .AddTo(gameObject);

            speed
                .Skip(1)
                .TakeUntil(Goaled)
                .Subscribe(speed =>
                {
                    speed = (speed * force / 10) - force;
                    bgRb2D.velocity = Vector2.down * speed;
                },
                () => 
                {
                    // ロケットの移動が終わったら背景のスクロールを止める
                    bgRb2D.velocity = Vector2.zero;
                }).AddTo(gameObject);

            Goaled.Where(x => !x)
                .Subscribe(failed =>
                {
                    rb2D.velocity = Vector2.down * 100.0f;
                    Destroy(gameObject, 3.0f);
                }).AddTo(gameObject);
                
        }

        public void PlayAnimation(string name)
        {
            Debug.Log($"<color=blue>anim: {name}</color>");
            anim.SetTrigger(name);
        }

        public void AddPower(float power)
        {
            speed.Value = power;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if(collision.gameObject == sun)
            {
                Goaled.OnNext(true);
            }
        }
    }
}