using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using UniRx;
using DG.Tweening;

namespace Beyond
{
    public class PowerGauge : MonoBehaviour
    {
        public IReadOnlyReactiveProperty<float> RushScore => _rushScore;
        /// <summary>
        /// 現在の連打スコア
        /// </summary>
        [SerializeField]
        private FloatReactiveProperty _rushScore = new FloatReactiveProperty();
        /// <summary>
        /// 連打一回ごとで得られるRushScoreの累積ボーナス
        /// </summary>
        [SerializeField]
        public float RushBonus { get { return _rushScore.Value * RushBonusE; } }
        /// <summary>
        /// RushBonusの係数
        /// </summary>
        public float RushBonusE = 1.2f;

        public IReadOnlyReactiveProperty<float> HoldFrame => _holdFrame;
        /// <summary>
        /// 現在の累計押下フレーム数
        /// </summary>
        [SerializeField]
        private FloatReactiveProperty _holdFrame = new FloatReactiveProperty(0.0f);
        /// <summary>
        /// 押下状態を継続している間、1フレームごとに加算されるボーナス
        /// </summary>
        public float HoldBonus = 1.01f;
        public IReadOnlyReactiveProperty<float> CurrentForce => currentForce;
        /// <summary>
        /// 現在の推進力
        /// </summary>
        [SerializeField]
        private FloatReactiveProperty currentForce = new FloatReactiveProperty();
        /// <summary>
        /// 現在の抵抗力
        /// </summary>
        public IReadOnlyReactiveProperty<float> CurrentPuls => _currentPower.Select(p => Mathf.Clamp(p, 1, Mathf.Max(p, MaxPower)) * pulsE).ToReadOnlyReactiveProperty<float>();
        /// <summary>
        /// 抵抗力係数
        /// </summary>
        [SerializeField]
        private float pulsE = 0.5f;

        private GameManager gameManager;
        public IReadOnlyReactiveProperty<float> CurrentPower => _currentPower;
        [SerializeField]
        private FloatReactiveProperty _currentPower = new FloatReactiveProperty(0.0f);
        [SerializeField]
        private float merginTime = 2.0f;
        /// <summary>
        /// 現在のパワーゲージの長さ
        /// </summary>
        public IReadOnlyReactiveProperty<float> CurrentGaugeScale => _currentGaugeScale;
        private FloatReactiveProperty _currentGaugeScale = new FloatReactiveProperty();
        public IObservable<Unit> IsReadyLaunch => _isReadyLaunch;
        private Subject<Unit> _isReadyLaunch = new Subject<Unit>();

        [SerializeField]
        public float MaxPower = 100.0f;

        [SerializeField]
        private float gaugeDureation = 0.5f;

        private void Awake()
        {
            gameManager = GetComponent<GameManager>();

            gameManager.OnInitialized
                .Subscribe(_ => OnInitialized());
        }

        private void OnInitialized()
        {
            #region 推進力の追加
            // ボタンを押したらRushScore加算
            var onButtonDown = gameManager.InputProvider
                .OnButtonDown
                .Where(_ => gameManager.IsPlayable.Value)
                //.Do(x => Debug.Log($"Is ButtonDownable: {x}"))
                .Where(x => x)
                .Share();

            onButtonDown
                .Subscribe(_ =>
                {
                    _rushScore.Value += 1 + RushBonus;
                    //var value = _rushScore.Value + 1 + RushBonus;
                    //_rushScore.Value = value > MaxPower ? MaxPower - 1 : value;
                });

            // ホールド時間を加算
            gameManager.InputProvider
                .OnHold
                .Where(x => gameManager.IsPlayable.Value)
                //.Do(x => Debug.Log($"Is Holdable: {x}"))
                .Where(x => x)
                .Subscribe(_ =>
                {
                    _holdFrame.Value += 1 + HoldBonus;
                });

            // ボタン押してないときは減衰

            Observable.IntervalFrame(1)
                .Where(_ => gameManager.IsPlayable.Value)
                .Where(_ => _currentPower.Value >= 0)
                .Subscribe(_ => _currentPower.Value -= 1)
                .AddTo(gameObject);

            gameManager.OnLounch
                .Subscribe(_ =>
                {
                    Observable.IntervalFrame(1)
                        .Where(x => _currentPower.Value >= 0)
                        .Subscribe(x => 
                        {
                            _currentPower.Value -= 0.8f;
                        });
                });
            #endregion


            #region ゲージ量（内部値）の反映
            // 推進力を合算
            Observable.CombineLatest(_rushScore, _holdFrame)
                .Select(x => x.Sum())
                .Select(value => value / CurrentPuls.Value)
                .Subscribe(x => currentForce.Value = x);

            // Animation
            currentForce
                .Subscribe(force =>
                {
                    var power = gameManager.IsPlayable.Value ? 
                    Mathf.Min(_currentPower.Value + force, MaxPower * 2.5f) :
                    _currentPower.Value + force;
                    DOTween
                        .To(
                            () => _currentPower.Value,
                            value => _currentPower.Value = value,
                            power,
                            gaugeDureation);
                });
            #endregion

            #region ゲージのUIへの反映
            var currentGauge = _currentPower
                // ゲージの最大値を超えないようにする
                .Select(power => Mathf.Clamp(power / MaxPower, 0.0f, 1.0f))
                .Do(value => _currentGaugeScale.Value = value)
                .Share();
            currentGauge
                .Subscribe(power =>
                {
                    var gauge = gameManager.UiProvider.Gauge;
                    gauge.transform.localScale =
                    new Vector3(
                        gauge.transform.localScale.x,
                        power,
                        gauge.transform.localScale.z);
                    if (power >= 1.0f)
                    {
                        gauge.color = Color.red;
                    }
                    else
                    {
                        gauge.color = Color.yellow;
                    }
                }).AddTo(gameObject);
            currentGauge
                .Where(g => g >= 1.0f)
                .SelectMany(Observable.Timer(TimeSpan.FromSeconds(merginTime)))
                .TakeUntil(currentGauge.Where(g => g < 1.0f))
                .RepeatUntilDestroy(gameObject)
                .Subscribe(_ =>
                {
                    _isReadyLaunch.OnNext(Unit.Default);
                });
            #endregion
        }

        public void Launch(float power)
        {
            _currentPower.Value = power;
        }
    }
}