using MagicArcher.Gameplay.Economy;
using UnityEngine;

namespace MagicArcher.UI
{
    public sealed class CombatUiRefs : MonoBehaviour
    {
        [SerializeField] TutorialOverlayView _tutorialOverlay;
        [SerializeField] TutorialPurchasePanelView _tutorialPurchasePanel;
        [SerializeField] BuyUnitButtonView _buyUnitButton;
        [SerializeField] EndgameOverlayView _endgameOverlay;
        [SerializeField] CtaFeedbackView _ctaFeedback;
        [SerializeField] RectTransform _healthBarsRoot;
        [SerializeField] HealthBarUiView _healthBarPrefab;
        [SerializeField] RectTransform _coinHudTarget;
        [SerializeField] CoinFlyVfx _coinFlyPrefab;
        [SerializeField] RectTransform _coinFlyPoolRoot;

        public TutorialOverlayView TutorialOverlay => _tutorialOverlay;
        public TutorialPurchasePanelView TutorialPurchasePanel => _tutorialPurchasePanel;
        public BuyUnitButtonView BuyUnitButton => _buyUnitButton;
        public EndgameOverlayView EndgameOverlay => _endgameOverlay;
        public CtaFeedbackView CtaFeedback => _ctaFeedback;
        public RectTransform HealthBarsRoot => _healthBarsRoot;
        public HealthBarUiView HealthBarPrefab => _healthBarPrefab;
        public RectTransform CoinHudTarget => _coinHudTarget;
        public CoinFlyVfx CoinFlyPrefab => _coinFlyPrefab;
        public RectTransform CoinFlyPoolRoot => _coinFlyPoolRoot;
    }
}
