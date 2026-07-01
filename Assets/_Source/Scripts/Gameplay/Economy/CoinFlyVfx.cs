using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace MagicArcher.Gameplay.Economy
{
    public sealed class CoinFlyVfx : MonoBehaviour
    {
        [SerializeField] Image _coinImage;

        RectTransform _rect;
        Tween _tween;

        void Awake()
        {
            _rect = transform as RectTransform;
            gameObject.SetActive(false);
        }

        public void Play(
            Vector2 startLocal,
            Vector2 spreadOffset,
            Vector2 targetLocal,
            float spreadDuration,
            float flyDelay,
            float flyDuration,
            Action onComplete)
        {
            if (_rect == null)
            {
                onComplete?.Invoke();
                return;
            }

            KillTween();
            _rect.anchoredPosition = startLocal;
            gameObject.SetActive(true);

            var spreadTarget = startLocal + spreadOffset;
            var waitBeforeFly = Mathf.Max(0f, flyDelay - spreadDuration);

            var sequence = DOTween.Sequence();
            sequence.Append(_rect.DOAnchorPos(spreadTarget, spreadDuration).SetEase(Ease.OutQuad));
            sequence.AppendInterval(waitBeforeFly);
            sequence.Append(_rect.DOAnchorPos(targetLocal, flyDuration).SetEase(Ease.InQuad));
            sequence.OnComplete(() =>
            {
                gameObject.SetActive(false);
                onComplete?.Invoke();
            });

            _tween = sequence;
        }

        public void Stop()
        {
            KillTween();
            gameObject.SetActive(false);
        }

        void OnDestroy()
        {
            KillTween();
        }

        void KillTween()
        {
            if (_tween == null || !_tween.IsActive())
                return;

            _tween.Kill();
            _tween = null;
        }
    }
}
