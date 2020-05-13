using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using UniRx.Async;
using Random = UnityEngine.Random;

namespace Scrum
{
    public class PersonProvider : MonoBehaviour
    {
        [SerializeField]
        private float socialDistance = 6.0f;
        public float SocialDistance => socialDistance;
        [SerializeField]
        private float killTime = 3.0f;
        [SerializeField]
        private float killInterval = 1.0f;
        public float KillTime => killTime;
        [SerializeField]
        private int mitsuCount = 3;
        public int MitsuCount => mitsuCount;
        [SerializeField]
        private Executioner mitsuPrefab;
        [SerializeField]
        private GameManager gameManager;
        [SerializeField]
        private int spawnCount = 100;
        [SerializeField]
        private Person personPrefab;
        public Subject<Unit> OnInitialized = new Subject<Unit>();
        public ReactiveCollection<Person> people = new ReactiveCollection<Person>();
        private List<Executioner> pooledMitsu = new List<Executioner>();
        public Subject<Vector3> MitsuPositionAsync = new Subject<Vector3>();
        public BoolReactiveProperty OnPause = new BoolReactiveProperty();

        private void Start()
        {
            #region Debug
#if UNITY_EDITOR
            if (gameManager.IsDebug)
            {
                this.UpdateAsObservable().Select(_ => Input.GetMouseButtonDown(0)).Where(x => x).Subscribe(_ =>
                {
                    var clickedPosition = Input.mousePosition;
                    clickedPosition -= Vector3.forward * Camera.main.transform.position.z;
                    Spawn(Camera.main.ScreenToWorldPoint(clickedPosition));
                }).AddTo(gameObject);
            }
#endif
            #endregion

            gameManager.GameStateObservable
                .Subscribe(async state =>
                {
                    switch (state)
                    {
                        case GameState.Initialize:
                            await StartCoroutine(SpawnPeopleAsync());
                            OnInitialized.OnNext(Unit.Default);
                            OnInitialized.OnCompleted();
                            break;
                        case GameState.InGame:
                            people.ObserveRemove()
                                .Where(_ => people.Count < 10)
                                .Subscribe(_ =>
                                {
                                    foreach(var person in people)
                                    {
                                        person.isCatastorophy.Value = true;
                                    }
                                }).AddTo(gameObject);

                            MitsuPositionAsync
                            .Where(v => v.x <= 32 && v.x >= -32 && v.y <= 24 && v.y >= -24)
                            .ThrottleFirst(TimeSpan.FromSeconds(2))
                            .Subscribe(pos =>
                            {
                                var m = pooledMitsu.FirstOrDefault(p => !p.gameObject.activeInHierarchy);
                                if(m == null)
                                {
                                    m = Instantiate(mitsuPrefab);
                                    m.Initialize(gameManager.Player, this);
                                    m.transform.position = pos;
                                    pooledMitsu.Add(m);
                                }
                                else
                                {
                                    m.gameObject.SetActive(true);
                                    m.transform.position = pos;
                                }
                            }).AddTo(gameObject);
                            break;
                        case GameState.Result:
                            MitsuPositionAsync.OnCompleted();
                            break;
                    }
                }).AddTo(gameObject);
        }

        private Person Spawn(Vector2 position)
        {
            var person = Instantiate(personPrefab, transform);
            person.Initialize(this);
            person.transform.position = position;
            people.Add(person);
            return person;
        }

        private IEnumerator SpawnPeopleAsync()
        {
            while(transform.childCount < spawnCount)
            {
                Spawn(new Vector2(Random.Range(-32, 32), Random.Range(-24, 24)));

                yield return null;
            }
        }
    }
}