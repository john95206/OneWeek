using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class InputProvider : MonoBehaviour
{
    public System.IObservable<float> OnHorizontal => _onHorizontal;
    private Subject<float> _onHorizontal = new Subject<float>();
    public System.IObservable<float> OnVertical => _onVertical;
    private Subject<float> _onVertical = new Subject<float>();
    public System.IObservable<Unit> OnFire => _onFire;
    private Subject<Unit> _onFire = new Subject<Unit>();

    void Start()
    {
        this.UpdateAsObservable()
            .Subscribe(_ => _onHorizontal.OnNext(Input.GetAxisRaw("Horizontal")));
        this.UpdateAsObservable()
            .Subscribe(_ => _onVertical.OnNext(Input.GetAxisRaw("Vertical")));
        this.UpdateAsObservable()
            .Where(_ => Input.GetKeyDown("f"))
            .Subscribe(fire => _onFire.OnNext(Unit.Default));
    }
}
