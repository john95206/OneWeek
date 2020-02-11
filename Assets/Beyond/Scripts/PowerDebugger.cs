using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

namespace Beyond
{
    public class PowerDebugger : MonoBehaviour
    {
        [SerializeField]
        private PowerGauge powerGauge;
        [SerializeField]
        private Text rush;
        [SerializeField]
        private Text hold;
        [SerializeField]
        private Text power;
        [SerializeField]
        private Text puls;
        [SerializeField]
        private Text go;
        [SerializeField]
        private Text gauge;
        [SerializeField]
        private Button launch;
        [SerializeField]
        private float launchPower = 150.0f;

        private void Awake()
        {
            if (!Debug.isDebugBuild)
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            go.text = "False";

            powerGauge.RushScore.Subscribe(value => rush.text = value.ToString());
            powerGauge.HoldFrame.Subscribe(value => hold.text = value.ToString());
            powerGauge.CurrentForce.Subscribe(value => power.text = value.ToString());
            powerGauge.CurrentPuls.Subscribe(value => puls.text = value.ToString());
            powerGauge.IsReadyLaunch.Subscribe(flag => go.text = powerGauge.CurrentPower.Value.ToString());
            powerGauge.CurrentPower.Subscribe(value => gauge.text = value.ToString()).AddTo(gameObject);
            launch.OnClickAsObservable().Subscribe(_ => powerGauge.Launch(launchPower));
        }
    }
}