using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;
using DG.Tweening;

namespace Candle
{
    public enum GameState
    {
        Initialize,
        Ready,
        InGame,
        GameOver
    }

    public class GameManager : MonoBehaviour
    {
        public IReadOnlyReactiveProperty<GameState> OnGameState => _onGameState;
        [SerializeField]
        private ReactiveProperty<GameState> _onGameState = new ReactiveProperty<GameState>();
        public CandleProvider candleProvider;
        public InputProvider InputProvider;
        [SerializeField]
        private LightCore _light;
        [SerializeField]
        private Text Guid;
        [SerializeField]
        private Text cheerText;
        public LightCore Light => _light;
        [SerializeField]
        private List<string> guidString = new List<string>();
        [SerializeField]
        private List<string> cheerString = new List<string>();
        [SerializeField]
        private AudioClip bgm;
        [SerializeField]
        private AudioClip endSe;
        private AudioSource audioSource;

        private void Awake()
        {
            InputProvider = GetComponent<InputProvider>();
            audioSource = GetComponent<AudioSource>();
        }

        void Start()
        {
            _onGameState
                //.Do(state => Debug.Log(state))
                .Subscribe(state =>
                {
                    switch (state)
                    {
                        case GameState.Initialize:
                            OnInitialized();
                            break;
                        case GameState.Ready:
                            OnChangeGuid(state);
                            break;
                        case GameState.InGame:
                            OnChangeGuid(state);
                            candleProvider.OnLit
                                .Subscribe(index =>
                                {
                                    //Debug.Log("Cheer");
                                    var pos = cheerText.transform.position;
                                    cheerText.text = cheerString.ElementAt(index);
                                    cheerText.DOFade(1, 0.1f)
                                        .OnComplete(() => {
                                            cheerText.rectTransform.DOLocalMoveY(15, 1f).SetRelative()
                                                .OnComplete(() =>
                                                    cheerText.transform.position = pos
                                                );
                                            cheerText.DOFade(0, 0.9f);
                                        }
                                        );
                                }).AddTo(gameObject);

                            audioSource.clip = bgm;
                            audioSource.Play();

                            candleProvider.BgmPitch.Subscribe(pitch => audioSource.pitch = 1 + pitch).AddTo(gameObject);
                            break;
                        case GameState.GameOver:
                            OnChangeGuid(state);

                            Observable.Timer(System.TimeSpan.FromSeconds(0.5f))
                                .Where(_ => candleProvider.LitCandleList.Count == candleProvider.CandleList.Count)
                                .First()
                                .Subscribe(_ => Guid.text += "\n Congratulations!!!");

                            this.UpdateAsObservable()
                                .Select(_ => Input.GetKeyDown("r"))
                                .Where(x => x)
                                .Subscribe(_ => SceneManager.LoadScene(SceneManager.GetActiveScene().name));

                            audioSource.Stop();
                            audioSource.pitch = 1;
                            audioSource.PlayOneShot(endSe);
                            break;
                    }
                }).AddTo(gameObject);

            candleProvider.OnGameOver.First().Subscribe(_ => _onGameState.Value = GameState.GameOver);
        }

        void OnInitialized()
        {
            _light.Initialized
                .DelayFrame(3)
                //.Do(_ => Debug.Log(_onGameState.Value))
                .Subscribe(_ => _onGameState.Value = GameState.Ready);            
            _light.Initalize(this);
        }

        public void ChangeState(GameState state)
        {
            _onGameState.Value = state;
        }

        private void OnChangeGuid(GameState state)
        {
            if (state == GameState.Ready)
            {
                Guid.text = guidString.ElementAt(0);
            }
            else if (state == GameState.InGame)
            {
                Guid.text = guidString.ElementAt(1);
            }
            else if (state == GameState.GameOver)
            {
                Guid.text = $"ともした ろうそく\n{candleProvider.LitCandleList.Count()} / {candleProvider.CandleList.Count()} \n {guidString.ElementAt(2)}";
            }
        }
    }
}