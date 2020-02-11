using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using System;
using System.Linq;

namespace Candle
{
    public class Candle : MonoBehaviour
    {
        public IReadOnlyReactiveProperty<bool> IsPlayable => _isPlayable;
        [SerializeField]
        private BoolReactiveProperty _isPlayable = new BoolReactiveProperty();
        public IReadOnlyReactiveProperty<bool> IsAlive => _isAlive;
        public IReadOnlyReactiveProperty<bool> IsStop => _isStop;
        [SerializeField]
        private BoolReactiveProperty _isStop = new BoolReactiveProperty();
        [SerializeField]
        Animator anim;
        [SerializeField]
        BoxCollider2D col;
        [SerializeField]
        private BoolReactiveProperty _isAlive = new BoolReactiveProperty();
        public IReadOnlyReactiveProperty<int> BodyCount => _bodyCount;
        private IntReactiveProperty _bodyCount = new IntReactiveProperty();
        [SerializeField]
        private GameObject fire;
        [SerializeField]
        private Transform bodyRoot;
        [SerializeField]
        private GameObject bodyPrefab;
        public List<GameObject> BodyList => body;
        private List<GameObject> body = new List<GameObject>();
        [SerializeField]
        private GameObject deadFoot;

        [SerializeField]
        private GameObject GuidIcon;

        private List<SpriteRenderer> spriteList = new List<SpriteRenderer>();

        private bool lit;

        private void Awake()
        {
            spriteList.Add(deadFoot.GetComponent<SpriteRenderer>());
        }

        private void Start()
        {
            _isPlayable
                .Subscribe(isPlay =>
                {
                    OnPlayableChanged(isPlay);
                }).AddTo(gameObject);

            _isAlive
                .Subscribe(x =>
                {
                    if (!x)
                    {
                        Die();
                    }
                }).AddTo(gameObject);

            col.OnCollisionEnter2DAsObservable()
                .Subscribe(_ => _isStop.Value = true);
            col.OnCollisionExit2DAsObservable()
                .Subscribe(_ => _isStop.Value = false);

            transform.ObserveEveryValueChanged(t => t.position)
                .Where(_ => IsPlayable.Value)
                .Subscribe(pos => 
                {
                    if(pos != Vector3.zero)
                    {
                        anim.SetTrigger("Walk");
                    }
                    else
                    {
                        anim.SetTrigger("Idle");
                    }
                }).AddTo(gameObject);
        }

        /// <summary>
        /// CandleProviderから呼ばれる
        /// </summary>
        /// <param name="num">追加するBodyの個数</param>
        public void AddBody(int num)
        {
            for(int n = 0;n < num; n++)
            {
                var b = Instantiate(bodyPrefab, bodyRoot);
                body.Add(b);
                b.transform.position += Vector3.up * n;
                spriteList.Add(b.GetComponent<SpriteRenderer>());
            }

            _bodyCount.Value = num;
        }

        private void OnPlayableChanged(bool isPlayable)
        {
            // 歩きモーションにチェンジ
            deadFoot.SetActive(!isPlayable);
            anim.gameObject.SetActive(isPlayable);

            if (isPlayable)
            {
                Observable.IntervalFrame(1)
                    // 死ぬまで
                    .TakeUntil(IsAlive.Where(x => !x))
                    .Buffer(60)
                    // コントロール権が移るまで
                    .TakeUntil(IsPlayable.Where(x => !x))
                    .Subscribe(_ =>
                    {
                        var meltedBody = body.LastOrDefault();
                        if(meltedBody != null)
                        {
                            body.Remove(meltedBody);
                            if (spriteList.Contains(meltedBody?.GetComponent<SpriteRenderer>()))
                            {
                                spriteList.Remove(meltedBody.GetComponent<SpriteRenderer>());
                            }
                            Destroy(meltedBody);
                            _bodyCount.Value--;
                        }
                        else
                        {
                            _isAlive.Value = false;
                        }
                    }).AddTo(gameObject);

                spriteList.ForEach(s => s.color = Color.white);
                lit = true;
            }
            else
            {
                if (!lit)
                {
                    spriteList.ForEach(s => s.color = new Color(0.8f, 0.8f, 0.8f));
                }
            }

            fire.transform.localPosition = 
                body.LastOrDefault()?.transform.localPosition + Vector3.down * 7 
                ?? Vector3.down * 7;
            fire.gameObject.SetActive(!isPlayable && lit);
        }

        public void OnLit()
        {
            _isPlayable.Value = true;
        }

        public void OnDeactivate()
        {
            _isPlayable.Value = false;
        }

        private void Die()
        {
            _isPlayable.Value = false;
            //Destroy(gameObject);
        }

        public void SwitchIcon(bool isOn)
        {
            GuidIcon?.SetActive(isOn && !lit);
        }
    }
}