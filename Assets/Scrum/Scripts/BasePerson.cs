using UnityEngine;
using UniRx;
using UniRx.Async;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Scrum
{
    public class BasePerson : MonoBehaviour
    {
        protected int mitsuCount;
        protected float socialDistance = 0.6f;
        protected Animator anim;
        [SerializeField]
        protected float sensitivility = 1.0f;
        protected SpriteRenderer renderer;
        protected CircleCollider2D collider2D;
        public Subject<Unit> DiedAsync = new Subject<Unit>();

        protected virtual void Awake()
        {
            renderer = GetComponent<SpriteRenderer>();
            collider2D = GetComponent<CircleCollider2D>();
            anim = GetComponent<Animator>();
        }

        public virtual void Initialize(PersonProvider provider)
        {
            socialDistance = provider.SocialDistance;
            mitsuCount = provider.MitsuCount;
        }

        /// <summary>
        /// 密になった時
        /// </summary>
        protected virtual void FixedUpdate()
        {
            // 決めた半径内にいる奴を全件取得
            var people = Physics2D.OverlapCircleAll(transform.position, sensitivility, 1 << LayerMask.NameToLayer("RayCast")).Where(p => p.gameObject != gameObject).ToList();
            // 密でなければ処理しない
            if (people.Count() < mitsuCount)
            {
                OnSocialDistance();
                return;
            }
            // ソーシャルディスタンスを乱す輩が存在する
            if (people.Any(p => p != GetComponent<Collider2D>() && p.Distance(GetComponent<Collider2D>()).distance < socialDistance))
            {
                // 密
                OnMitsu(people);
            }
            else
            {
                OnSocialDistance();
            }
        }

        protected virtual void OnMitsu(List<Collider2D> people)
        {

        }

        protected virtual void OnSocialDistance()
        {

        }
    }
}