using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace MagicArcher.UI
{
    public sealed class TutorialOverlayView : MonoBehaviour
    {
        [SerializeField] Image _dimmer;
        [SerializeField] RectTransform _hand;
        [SerializeField] RectTransform _handPulseTarget;
        [SerializeField] Vector2 _uiTargetOffset = new Vector2(36f, -36f);
        [SerializeField] Vector2 _handEnterPadding = new Vector2(100f, 100f);
        [SerializeField] float _handEnterDuration = 1f;
        [SerializeField] float _mergeDragDuration = 0.9f;

        Tween _handTween;
        Sequence _handSequence;
        RectTransform _pointAtTarget;
        Vector3? _worldPointTarget;
        bool _mergeDragActive;
        Coroutine _repositionRoutine;

        public void Show(RectTransform pointAt)
        {
            gameObject.SetActive(true);
            StopHandMotion();
            _pointAtTarget = pointAt;
            _worldPointTarget = null;
            _mergeDragActive = false;

            if (_dimmer != null)
            {
                _dimmer.raycastTarget = true;
                _dimmer.transform.SetAsFirstSibling();
            }

            if (_hand == null)
                return;

            _hand.gameObject.SetActive(pointAt != null);
            if (pointAt == null)
                return;

            _hand.SetAsLastSibling();
            _hand.localScale = Vector3.one;
            AnimateHandToUiTarget(pointAt);
            ScheduleReposition();
        }

        public void ShowAtWorldPosition(Vector3 worldPosition)
        {
            gameObject.SetActive(true);
            StopHandMotion();
            _pointAtTarget = null;
            _worldPointTarget = worldPosition;
            _mergeDragActive = false;

            if (_dimmer != null)
            {
                _dimmer.raycastTarget = false;
                _dimmer.transform.SetAsFirstSibling();
            }

            if (_hand == null)
                return;

            _hand.gameObject.SetActive(true);
            _hand.SetAsLastSibling();
            _hand.localScale = Vector3.one;

            if (!TryGetWorldLocalPoint(worldPosition, out var localPoint))
                return;

            AnimateHandTo(localPoint + _uiTargetOffset, StartHandPulse);
        }

        public void PlayMergeDragHint(Vector3 sourceWorld, Vector3 targetWorld)
        {
            gameObject.SetActive(true);
            StopHandMotion();
            _pointAtTarget = null;
            _worldPointTarget = null;
            _mergeDragActive = true;

            if (_dimmer != null)
            {
                _dimmer.raycastTarget = false;
                _dimmer.transform.SetAsFirstSibling();
            }

            if (_hand == null)
                return;

            _hand.gameObject.SetActive(true);
            _hand.SetAsLastSibling();
            _hand.localScale = Vector3.one;

            if (!TryGetWorldLocalPoint(sourceWorld, out var sourceLocal) ||
                !TryGetWorldLocalPoint(targetWorld, out var targetLocal))
            {
                StartHandPulse();
                return;
            }

            sourceLocal += new Vector2(24f, -24f);
            targetLocal += new Vector2(24f, -24f);
            AnimateHandTo(sourceLocal, () => StartMergeDragLoop(sourceLocal, targetLocal));
        }

        public void Hide()
        {
            StopReposition();
            StopHandMotion();
            _pointAtTarget = null;
            _worldPointTarget = null;
            _mergeDragActive = false;
            gameObject.SetActive(false);
        }

        public void RefreshActiveHint()
        {
            if (!gameObject.activeInHierarchy)
                return;

            if (_pointAtTarget != null)
            {
                Show(_pointAtTarget);
                return;
            }

            if (_mergeDragActive)
                return;

            if (_worldPointTarget.HasValue)
                ShowAtWorldPosition(_worldPointTarget.Value);
        }

        void AnimateHandToUiTarget(RectTransform target)
        {
            Canvas.ForceUpdateCanvases();
            if (!TryGetOverlayLocalPoint(target, out var localPoint))
                return;

            AnimateHandTo(localPoint + _uiTargetOffset, StartHandPulse);
        }

        void AnimateHandTo(Vector2 targetLocal, Action onArrived = null)
        {
            if (_hand == null)
                return;

            _hand.anchoredPosition = GetHandEntryLocalPosition();

            _handSequence = DOTween.Sequence();
            _handSequence.Append(_hand.DOAnchorPos(targetLocal, _handEnterDuration).SetEase(Ease.OutCubic));
            if (onArrived != null)
                _handSequence.OnComplete(() => onArrived());
        }

        Vector2 GetHandEntryLocalPosition()
        {
            var overlay = transform as RectTransform;
            if (overlay == null)
                return Vector2.zero;

            var rect = overlay.rect;
            return new Vector2(
                rect.width * 0.5f + _handEnterPadding.x,
                -rect.height * 0.5f - _handEnterPadding.y);
        }

        void StartMergeDragLoop(Vector2 sourceLocal, Vector2 targetLocal)
        {
            if (_hand == null)
                return;

            _hand.anchoredPosition = sourceLocal;
            _handSequence = DOTween.Sequence();
            _handSequence.Append(_hand.DOAnchorPos(targetLocal, _mergeDragDuration).SetEase(Ease.InOutSine));
            _handSequence.AppendInterval(0.15f);
            _handSequence.Append(_hand.DOAnchorPos(sourceLocal, _mergeDragDuration * 0.75f).SetEase(Ease.InOutSine));
            _handSequence.AppendInterval(0.15f);
            _handSequence.SetLoops(-1, LoopType.Restart);
        }

        void ScheduleReposition()
        {
            StopReposition();
            _repositionRoutine = StartCoroutine(RepositionAfterLayout());
        }

        void StopReposition()
        {
            if (_repositionRoutine == null)
                return;

            StopCoroutine(_repositionRoutine);
            _repositionRoutine = null;
        }

        IEnumerator RepositionAfterLayout()
        {
            yield return null;
            Canvas.ForceUpdateCanvases();

            if (_pointAtTarget != null)
                AnimateHandToUiTarget(_pointAtTarget);
        }

        bool TryGetOverlayLocalPoint(RectTransform target, out Vector2 localPoint)
        {
            localPoint = default;
            var overlay = transform as RectTransform;
            if (_hand == null || overlay == null || target == null)
                return false;

            var canvas = _hand.GetComponentInParent<Canvas>();
            if (canvas == null)
                return false;

            var camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            var screenPoint = RectTransformUtility.WorldToScreenPoint(
                camera,
                target.TransformPoint(target.rect.center));

            return RectTransformUtility.ScreenPointToLocalPointInRectangle(
                overlay,
                screenPoint,
                camera,
                out localPoint);
        }

        bool TryGetWorldLocalPoint(Vector3 worldPosition, out Vector2 localPoint)
        {
            localPoint = default;
            var overlay = transform as RectTransform;
            if (_hand == null || overlay == null)
                return false;

            var canvas = _hand.GetComponentInParent<Canvas>();
            var camera = Camera.main;
            if (canvas == null || camera == null)
                return false;

            var screen = camera.WorldToScreenPoint(worldPosition);
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(
                overlay,
                screen,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
                out localPoint);
        }

        void StartHandPulse()
        {
            var pulseTarget = ResolvePulseTarget();
            if (pulseTarget == null)
                return;

            if (_handTween != null && _handTween.IsActive())
                _handTween.Kill();

            pulseTarget.localScale = Vector3.one;
            _handTween = pulseTarget
                .DOScale(1.12f, 0.45f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        RectTransform ResolvePulseTarget()
        {
            if (_handPulseTarget != null)
                return _handPulseTarget;

            if (_hand != null && _hand.childCount > 0)
                return null;

            return _hand;
        }

        void StopHandMotion()
        {
            if (_handTween != null && _handTween.IsActive())
                _handTween.Kill();

            if (_handSequence != null && _handSequence.IsActive())
                _handSequence.Kill();

            _handTween = null;
            _handSequence = null;
        }

        void OnDestroy()
        {
            StopReposition();
            StopHandMotion();
        }
    }
}
