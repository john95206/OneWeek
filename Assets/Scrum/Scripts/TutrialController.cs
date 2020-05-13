using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Async;
using System.Linq;
using Fungus;

namespace Scrum
{

    public class TutrialController : MonoBehaviour
    {
        [SerializeField]
        private Flowchart flowchart;
        [SerializeField]
        private string key;
        [SerializeField]
        public IntReactiveProperty ProgressObservable = new IntReactiveProperty(-1);
        [SerializeField]
        public bool isTutorial = true;

        private void Start()
        {
            ProgressObservable
                .Where(_ => isTutorial)
                .Distinct()
                .DelayFrame(1)
                .Subscribe(async x =>
                {
                    flowchart.SetIntegerVariable(key, x);
                    await UniTask.WaitUntil(() => !flowchart.GetExecutingBlocks().Any(b => b.BlockName == "Tutorial"));
                    flowchart.ExecuteBlock("Tutorial");
                }).AddTo(gameObject);
            flowchart.ObserveEveryValueChanged(f => f.GetIntegerVariable(key))
                .Subscribe(v => ProgressObservable.Value = v).AddTo(gameObject);
        }

        public void End()
        {
            flowchart.ExecuteBlock("End");
        }
    }
}