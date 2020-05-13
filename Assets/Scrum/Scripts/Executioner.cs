using System.Linq;
using UnityEngine;
using UniRx.Triggers;
using UniRx;

namespace Scrum
{
    public class Executioner : MonoBehaviour
    {
        private Player player;
        private PersonProvider provider;
        private SpriteRenderer render;
        private float restExecteTime;
        [SerializeField]
        private float colorSeed;
        private float safeTime;
        [SerializeField]
        AudioSource audioSource;
        [SerializeField]
        private GameObject soscialDistance;
        private Collider2D collider;
        private float radius;
        private int mitsuCount;
        private bool isExecuting = false;

        public void Initialize(Player player, PersonProvider provider)
        {
            this.player = player;
            this.provider = provider;
            var col = GetComponent<CircleCollider2D>();
            collider = col.GetComponent<Collider2D>();
            render = GetComponent<SpriteRenderer>();
            radius = col.radius;
            col.enabled = false;
            mitsuCount = provider.MitsuCount;
            safeTime = provider.KillTime;
            restExecteTime = provider.KillTime;

            provider.OnPause
                .Subscribe(x =>
                {
                    if (x)
                    {
                        audioSource.Pause();
                    }
                    else
                    {
                        audioSource.UnPause();
                    }
                }).AddTo(gameObject);
        }

        private void OnEnable()
        {
            if (provider != null)
            {
                restExecteTime = provider.KillTime;
            }
            audioSource.volume = 0.0f;
            audioSource.pitch = 1.0f;
        }

        private void Update()
        {
            colorSeed = (safeTime - restExecteTime) / safeTime;
            render.color = new Color(1, 1, 1, colorSeed);
            audioSource.volume = colorSeed;
            audioSource.pitch = 1 + colorSeed / 2;
        }

        private async void FixedUpdate()
        {
            var people = Physics2D.OverlapCircleAll(transform.position, radius).Where(p => p != collider).ToList();

            // 密なら殺しのカウントダウン
            if (people.Count > mitsuCount + mitsuCount)
            {
                restExecteTime -= Time.deltaTime;
            }
            // 自殺
            else if(!isExecuting)
            {
                var so = Instantiate(soscialDistance);
                so.transform.position = transform.position;
                Destroy(so, 1);
                gameObject.SetActive(false);
                return;
            }

            // 範囲内にいる全てを殺す
            if (restExecteTime < 0)
            {
                isExecuting = true;
                foreach (var person in people)
                {
                    var p = person.GetComponent<Person>();
                    p?.Die();
                }
                isExecuting = false;
                gameObject.SetActive(false);
            }
        }
    }
}