using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Fungus;
using UniRx;


namespace Beyond
{
    public class Narrative : MonoBehaviour
    {
        public List<string> IntroductionTexts = new List<string>();
        public List<string> InGameTexts = new List<string>();
        [SerializeField]
        private GameManager gameManager;
        [SerializeField]
        Flowchart flowchart;
        [SerializeField]
        private Character doctor;
        [SerializeField]
        private Character girl;
        public ISubject<Block> ExecutingBlock => executingBlock;
        private Subject<Block> executingBlock = new Subject<Block>();

        private void Awake()
        {
            gameManager.OnInitialized.Subscribe(_ => Initialize());
        }

        private void Initialize()
        {
            gameManager.GameStateObservable
                .Subscribe(state => 
                {
                    flowchart.SetStringVariable("GameState", state.ToString());
                    ExecuteNarrative(state);
                });

            gameManager.IsPlayable.Where(x => x)
                .SelectMany(gameManager.PowerGauge.CurrentPower)
                .TakeUntil(gameManager.GameStateObservable.Where(state => state != GameState.InGame))
                .Subscribe(power =>
                {
                    flowchart.SetFloatVariable("Power", power);
                });

            gameManager.Rocket.Height
                .TakeUntil(gameManager.GameStateObservable.Where(state => state != GameState.Ending))
                .Subscribe(height =>
                {
                    flowchart.SetFloatVariable("Height", height);
                });
        }

        private void ExecuteNarrative(GameState state)
        {
            var name = string.Empty;
            switch (state)
            {
                case GameState.Introduction:
                    name = "Intro";
                    break;
                case GameState.InGame:
                    name = "InGame";
                    break;
                case GameState.Ending:
                    name = "Ending";
                    break;
                default:
                    break;
            }
            ExecuteBlock(name);
        }

        private void ExecuteBlock(string name)
        {
            if (flowchart.HasBlock(name))
            {
                flowchart.ExecuteBlock(name);
                executingBlock.OnNext(flowchart.FindBlock(name));
            }
            else
            {
                if (Debug.isDebugBuild)
                {
                    Debug.Log($"No Block");
                }
            }
        }
    }
}