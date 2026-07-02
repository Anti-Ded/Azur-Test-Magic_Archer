using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace MagicArcher.UI
{
    public sealed class CtaFeedbackView : MonoBehaviour
    {
        [SerializeField] RectTransform _panel;
        [SerializeField] Text _label;
        [SerializeField] float _showDuration = 2f;

        CanvasGroup _canvasGroup;
        Sequence _sequence;

        void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();

            gameObject.SetActive(false);
        }

        public void Show(string message)
        {
            if (_panel == null || _label == null)
                return;

            KillSequence();
            _label.text = message;
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            _canvasGroup.alpha = 0f;
            _panel.localScale = Vector3.one * 0.92f;

            _sequence = DOTween.Sequence();
            _sequence.Append(_canvasGroup.DOFade(1f, 0.15f));
            _sequence.Join(_panel.DOScale(1f, 0.2f).SetEase(Ease.OutBack));
            _sequence.AppendInterval(_showDuration);
            _sequence.Append(_canvasGroup.DOFade(0f, 0.2f));
            _sequence.OnComplete(() => gameObject.SetActive(false));
        }

        void OnDestroy()
        {
            KillSequence();
        }

        void KillSequence()
        {
            if (_sequence == null || !_sequence.IsActive())
                return;

            _sequence.Kill();
            _sequence = null;
        }
    }
}
