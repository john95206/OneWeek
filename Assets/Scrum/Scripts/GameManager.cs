using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Async;

namespace Scrum
{
    public enum GameState
    {
        None,
        Initialize,
        Tutorial,
        InGame,
        Result,
        Epilogue,
    }

    public class GameManager : MonoBehaviour
    {
        [SerializeField]
        private Player playerPrefab;
        [SerializeField]
        private CanvasGroup dialog;
        [SerializeField]
        private GameObject texts;
        [SerializeField]
        private Button button;
        [SerializeField]
        private AudioSource bgm;
        private PersonProvider provider;
        private TutrialController tutrialController;
        private ReactiveProperty<GameState> _gameStateObservable = new ReactiveProperty<GameState>(GameState.None);
        public IReadOnlyReactiveProperty<GameState> GameStateObservable => _gameStateObservable;
        public bool IsDebug = false;
        public Player Player { get; private set; }

        private void Awake()
        {
            provider = GetComponent<PersonProvider>();
            tutrialController = GetComponent<TutrialController>();
        }

        private async void Start()
        {
            provider.OnInitialized.Subscribe(_ => _gameStateObservable.Value = GameState.Tutorial).AddTo(gameObject);
            await UniTask.Yield();

            _gameStateObservable.Subscribe(async state =>
            {
                switch (state)
                {
                    case GameState.None:
                        await UniTask.WaitUntil(() => tutrialController.ProgressObservable.Value > -1);
                        _gameStateObservable.Value = GameState.Initialize;
                        break;
                    case GameState.Tutorial:
                        tutrialController.ProgressObservable.Value = 2;
                        await UniTask.WaitUntil(() => Input.GetMouseButtonDown(0));
                        bgm.Play();

                        Player = Instantiate(playerPrefab);
                        Player.Initialize(provider);
                        var cursorPosition = Input.mousePosition;
                        cursorPosition -= Vector3.forward * Camera.main.transform.position.z;
                        Player.transform.position = Camera.main.ScreenToWorldPoint(cursorPosition) - Vector3.down;
                        tutrialController.ProgressObservable.Value = 3;
                        _gameStateObservable.Value = GameState.InGame;
                        break;
                    case GameState.InGame:
                        provider.people.ObserveRemove().First().Delay(System.TimeSpan.FromSeconds(1.5f)).Subscribe(_ => tutrialController.ProgressObservable.Value = 4).AddTo(gameObject);
                        Player.Hp.Skip(1).Where(x => x < Player.weakedHp).First().Subscribe(_ =>
                        {
                            tutrialController.ProgressObservable.Value = 5;
                        }).AddTo(gameObject);
                        Player.DiedAsync.Subscribe(_ =>
                        {
                            tutrialController.ProgressObservable.Value = 6;
                            foreach(var person in provider.people)
                            {
                                if (person.isFriendly)
                                {
                                    person.isCatastorophy.Value = true;
                                }
                            }
                            _gameStateObservable.Value = GameState.Result;
                        }).AddTo(gameObject);
                        break;
                    case GameState.Result:
                        await UniTask.WaitUntil(() => Input.GetMouseButtonDown(0));
                        tutrialController.End();
                        await UniTask.WaitUntil(() => Input.GetMouseButtonDown(0));
                        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
                        break;
                }
            }).AddTo(gameObject);

            dialog.ObserveEveryValueChanged(d => d.alpha > 0)
                .Subscribe(x =>
                {
                    if (x)
                    {
                        bgm.Pause();
                        Time.timeScale = 0.0f;
                        button.OnClickAsObservable()
                        .Subscribe(_ =>
                        {
                            Time.timeScale = 1.0f;
                        }).AddTo(gameObject);
                    }
                    else
                    {
                        bgm.UnPause();
                    }
                    provider.OnPause.Value = x;
                }).AddTo(gameObject);
        }
    }
}