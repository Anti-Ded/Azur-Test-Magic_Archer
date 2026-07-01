using MagicArcher.Gameplay.Combat;
using MagicArcher.Gameplay.Economy;
using UnityEngine;

namespace MagicArcher.UI
{
    public sealed class CombatUiRefs : MonoBehaviour
    {
        [SerializeField] TutorialOverlayView _tutorialOverlay;
        [SerializeField] BuyUnitButtonView _buyUnitButton;
        [SerializeField] EndgameOverlayView _endgameOverlay;
        [SerializeField] CtaFeedbackView _ctaFeedback;
        [SerializeField] RectTransform _healthBarsRoot;
        [SerializeField] HealthBarUiView _healthBarPrefab;

        public TutorialOverlayView TutorialOverlay => _tutorialOverlay;
        public BuyUnitButtonView BuyUnitButton => _buyUnitButton;
        public EndgameOverlayView EndgameOverlay => _endgameOverlay;
        public CtaFeedbackView CtaFeedback => _ctaFeedback;
        public RectTransform HealthBarsRoot => _healthBarsRoot;
        public HealthBarUiView HealthBarPrefab => _healthBarPrefab;
    }
}
