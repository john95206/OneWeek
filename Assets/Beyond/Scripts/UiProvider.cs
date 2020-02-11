using UnityEngine;
using UnityEngine.UI;
using System;

namespace Beyond
{
    public class UiProvider : MonoBehaviour
    {
        public Image Gauge => gauge;
        [SerializeField]
        private Image gauge;
        public SpriteRenderer Rocket => rocket;
        [SerializeField]
        private SpriteRenderer rocket;
    }
}