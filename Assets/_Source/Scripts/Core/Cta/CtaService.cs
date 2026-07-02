using MagicArcher.UI;
using UnityEngine;

namespace MagicArcher.Core.Cta
{
    public sealed class CtaService : ICtaService
    {
        readonly CtaFeedbackView _feedback;

        public bool IsActive { get; private set; }

        public CtaService([Zenject.Inject(Optional = true)] CtaFeedbackView feedback = null)
        {
            _feedback = feedback;
        }

        public void Activate()
        {
            IsActive = true;
            _activatedAtFrame = UnityEngine.Time.frameCount;
        }

        public void TryInvoke()
        {
            if (!IsActive)
                return;

            if (UnityEngine.Time.frameCount < _activatedAtFrame)
                return;

            if (_feedback != null)
            {
                _feedback.Show("CTA BUTTON CLICKED");
                return;
            }

            Debug.Log("CTA BUTTON CLICKED");
        }

        int _activatedAtFrame = -1;
    }
}
