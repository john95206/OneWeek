using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UniRx;
using UniRx.Triggers;

namespace Beyond
{
    public class GameManager : MonoBehaviour
    {
        public InputProvider InputProvider => _inputProvider;
        private InputProvider _inputProvider;
        public UiProvider UiProvider => _uiProvider;
        [SerializeField]
        private UiProvider _uiProvider;
        public PowerGauge PowerGauge => powerGauge;
        private PowerGauge powerGauge;
        [SerializeField]
        private Narrative narrative;
        public Rocket Rocket => rocket;
        [SerializeField]
        private Rocket rocket;
        public IObservable<Unit> OnInitialized => _onInitialized;
        private Subject<Unit> _onInitialized = new Subject<Unit>();
        public IReadOnlyReactiveProperty<GameState> GameStateObservable => _gameState;
        private ReactiveProperty<GameState> _gameState = new ReactiveProperty<GameState>(GameState.None);

        public IReadOnlyReactiveProperty<bool> IsPlayable => _isPlayable;
        private BoolReactiveProperty _isPlayable = new BoolReactiveProperty();
        public IObservable<Unit> OnLounch => _onLounch;
        private Subject<Unit> _onLounch = new Subject<Unit>();

        private void Awake()
        {
            if (Debug.isDebugBuild)
            {
                this.UpdateAsObservable()
                    .Select(_ => Input.GetKeyDown(KeyCode.R))
                    .Where(x => x)
                    .Subscribe(_ => SceneManager.LoadScene(SceneManager.GetActiveScene().name));
            }

            _inputProvider = GetComponent<InputProvider>();
            powerGauge = GetComponent<PowerGauge>();

            _gameState
                .Do(state =>
                {
                    _isPlayable.Value =
                        state == GameState.Title ||
                        state == GameState.InGame ||
                        state == GameState.Result;
                })
                .Subscribe(state =>
                {
                    switch (state)
                    {
                        case GameState.Initialize:
                            OnInitialize();
                            break;
                        case GameState.Title:
                            if (Debug.isDebugBuild)
                            {
                                Debug.Log("<color=yellow>Title</color>");
                            }
                            OnStart();
                            break;
                        case GameState.Introduction:
                            if (Debug.isDebugBuild)
                            {
                                Debug.Log("<color=yellow>Introduction</color>");
                            }
                            OnIntroduction();
                            break;
                        case GameState.InGame:
                            if (Debug.isDebugBuild)
                            {
                                Debug.Log("<color=yellow>InGame</color>");
                            }
                            OnGameStart();
                            break;
                        case GameState.Ending:
                            if (Debug.isDebugBuild)
                            {
                                Debug.Log("<color=yellow>Ending</color>");
                            }
                            OnLaunch();
                            break;
                        case GameState.Result:
                            if (Debug.isDebugBuild)
                            {
                                Debug.Log("<color=yellow>Result</color>");
                            }
                            rocket.PlayAnimation("Finish");
                            break;
                    }
                }).AddTo(gameObject);

            _onInitialized
                .Subscribe(complete =>
                {
                    if (Debug.isDebugBuild)
                    {
                        Debug.Log("Initialized");
                    }
                });

            rocket.Goaled.Where(x => x)
                .Subscribe(_ => _gameState.Value = GameState.Result);

            //narrative.ExecutingBlock
            //    .DelayFrame(30)
            //    .Subscribe(block =>
            //    {
            //        // Fungusのナレーションが終わったらGameStateを更新
            //        Observable.Return(Unit.Default)
            //            .SkipWhile(_ => block.IsExecuting())
            //            .Subscribe(_ => 
            //            {
            //                switch (block.BlockName)
            //                {
            //                    case "Intro":
            //                        _gameState.Value = GameState.InGame;
            //                        break;
            //                    case "InGame":
            //                        _gameState.Value = GameState.Ending;
            //                        break;
            //                    case "Ending":
            //                        _gameState.Value = GameState.Result;
            //                        break;
            //                    default:
            //                        Debug.LogError("Unknown Block");
            //                        break;
            //                }
            //            });
            //    });
        }

        private void Start()
        {
            _gameState.Value = GameState.Initialize;
        }

        private void OnInitialize()
        {
            _onInitialized.OnNext(Unit.Default);
            _onInitialized.OnCompleted();

            // GameStateの更新は最後に
            _gameState.Value = GameState.Title;
        }

        private void OnStart()
        {
            this.UpdateAsObservable()
                .Select(_ => Input.GetKeyDown(KeyCode.Space))
                .Where(x => x)
                .Take(1)
                .Subscribe(_ => _gameState.Value = GameState.Introduction).AddTo(gameObject);
        }

        private void OnIntroduction()
        {
            // FixMe
            //_gameState.Value = GameState.InGame;
        }

        private void OnGameStart()
        {
            powerGauge.IsReadyLaunch
                .Take(1)
                .Subscribe(_ => _gameState.Value = GameState.Ending);
        }

        private void OnLaunch()
        {
            if (Debug.isDebugBuild)
            {
                Debug.Log($"<color=red>OnLaounchPower: {powerGauge.CurrentPower}</color>");
            }
            // ロケット発射
            rocket.PlayAnimation("Boost");

            var powerObservable = powerGauge.CurrentPower;
            powerObservable
                .TakeUntil(powerObservable.Where(p => p <= 0))
                .Subscribe(power =>
                {
                    rocket.AddPower(power);
                },
                () =>
                {
                    if (Debug.isDebugBuild)
                    {
                        Debug.Log("<color=red>Run Out</color>");
                    }

                    Observable.IntervalFrame(1)
                        .Subscribe(_ => rocket.AddPower(powerObservable.Value)).AddTo(gameObject);

                    Observable.Timer(TimeSpan.FromSeconds(1.0f)).First()
                        .Subscribe(_ => 
                        {
                            _gameState.Value = GameState.Result;

                            rocket.Goaled.OnNext(false);
                        }).AddTo(gameObject);
                }).AddTo(gameObject);

            _onLounch.OnNext(Unit.Default);
        }
    }
}