using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UniRx;

namespace Candle
{
    public class CandleProvider : MonoBehaviour
    {

        [SerializeField]
        public ReactiveCollection<Candle> LitCandleList = new ReactiveCollection<Candle>();
        [SerializeField]
        private List<int> bodyLengthContainer = new List<int>();
        public List<Candle> CandleList => candleList;
        private List<Candle> candleList = new List<Candle>();
        public ISubject<int> OnLit => _onLit;
        private Subject<int> _onLit = new Subject<int>();
        public ISubject<Unit> OnGameOver => _onGameOver;
        private Subject<Unit> _onGameOver = new Subject<Unit>();
        public IReadOnlyReactiveProperty<float> BgmPitch => _bgmPitch;
        private FloatReactiveProperty _bgmPitch = new FloatReactiveProperty(1);

        private AudioSource audioSource;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();

            for(var i = 0; i < transform.childCount; i++ )
            {
                var c = transform.GetChild(i).GetComponent<Candle>();
                candleList.Add(c);

                var r = (int)Random.Range(0, bodyLengthContainer.Count());
                c.AddBody(bodyLengthContainer.ElementAt(r));

                c.IsPlayable.Where(x => x)
                    .Subscribe(onPlayable =>
                    {
                        if (!LitCandleList.Contains(c))
                        {
                            LitCandleList.Add(c);
                        }

                        PlaySE();

                        c.BodyCount
                           .TakeUntil(c.BodyCount.Where(x => x == 0))
                           .Subscribe(count => _bgmPitch.Value = Mathf.InverseLerp(0.2f, 10f, 5 / Mathf.Clamp(count, 1, 5)));
                    });
                c.IsPlayable.Where(x => !x)
                    .Subscribe(deActivate =>
                    {
                        c.transform.SetParent(transform);
                    });
            }
        }

        private void Start()
        {
            Observable.Amb(
                candleList.ObserveEveryValueChanged(list => list.All(c => c.IsAlive.Value)).Where(x => !x),
                LitCandleList.ObserveAdd().Select(_ => candleList.Count == LitCandleList.Count).Where(x => x)
                ).First()
                .Subscribe(_ => _onGameOver.OnNext(Unit.Default)).AddTo(gameObject);

            LitCandleList.ObserveAdd()
                .Select(add => add.Index)
                .Subscribe(index => _onLit.OnNext(index));
        }

        private void PlaySE()
        {
            var pitch = Mathf.InverseLerp(0, 18, LitCandleList.Count);
            audioSource.pitch = pitch + 1;
            //Debug.Log($"pitch {pitch}");
            audioSource.PlayOneShot(audioSource.clip);
        }
    }
}