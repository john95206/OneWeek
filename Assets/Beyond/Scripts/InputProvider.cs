using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace Beyond
{
    public class InputProvider : MonoBehaviour
    {
        public IReadOnlyReactiveProperty<bool> OnButtonDown => _onButtonDown;
        private BoolReactiveProperty _onButtonDown = new BoolReactiveProperty();
        public ISubject<bool> OnHold => _onHold;
        private Subject<bool> _onHold = new Subject<bool>();
        private GameManager gameManager;

        private void Awake()
        {
            gameManager = GetComponent<GameManager>();

            gameManager.OnInitialized
                .Subscribe(_ => Initialize());
        }

        private void Initialize()
        {
            this.UpdateAsObservable()
                .Select(_ => Input.GetKeyDown("space"))
                .Subscribe(x =>
                {
                    _onButtonDown.Value = x;
                });

            this.UpdateAsObservable()
                .Select(_ => Input.GetKey("space"))
                .Subscribe(x => 
                {
                    _onHold.OnNext(x);
                });
        }
    }
}