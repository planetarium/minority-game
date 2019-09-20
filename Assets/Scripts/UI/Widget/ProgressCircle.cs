using System;
using TMPro;
using UnityEngine;

namespace MinorityGame.UI
{
    public class ProgressCircle : MonoBehaviour
    {
        private const float rotateEachTick = 360f / 12;

        public RectTransform imageTransform;
        public TextMeshProUGUI text;

        private float _progressValue;

        /// <summary>
        /// 0f - 1f
        /// </summary>
        public float ProgressValue
        {
            get => _progressValue;
            set
            {
                _progressValue = Math.Max(Math.Min(value, 1f), 0f);
                text.text = $"{_progressValue:P0}";
            }
        }

        private void OnEnable()
        {
            imageTransform.localRotation = Quaternion.identity;
        }

        private void Reset()
        {
            imageTransform = transform.Find("Image").GetComponent<RectTransform>();
            text = transform.Find("Text").GetComponent<TextMeshProUGUI>();
        }

        private void Update()
        {
            imageTransform.Rotate(0f, 0f, rotateEachTick);
        }
    }
}
