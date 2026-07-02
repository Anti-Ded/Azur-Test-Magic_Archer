using MagicArcher.Core;
using MagicArcher.Gameplay.Units;
using MagicArcher.StateMachine;
using MagicArcher.UI;
using UnityEngine;
using Zenject;

namespace MagicArcher.Gameplay.Flow
{
    public sealed class TutorialPurchaseDirector : MonoBehaviour
    {
        UnitPurchaseService _purchase;
        CombatUiRefs _ui;
        GamePhaseService _phases;

        TutorialPurchasePanelView _panel;
        BuyUnitButtonView _mainBuyButton;

        [Inject]
        void Construct(
            UnitPurchaseService purchase,
            GamePhaseService phases,
            [Inject(Optional = true)] CombatUiRefs ui = null)
        {
            _purchase = purchase;
            _ui = ui;
            _phases = phases;
        }

        public void Begin()
        {
            if (_purchase == null || _phases == null)
            {
                Debug.LogError("TutorialPurchaseDirector: missing dependencies.");
                return;
            }

            _panel = ResolvePanel();
            if (_panel == null || _panel.BuyButton == null)
            {
                Debug.LogError("TutorialPurchaseDirector: tutorial purchase panel is missing.");
                return;
            }

            _mainBuyButton = ResolveMainBuyButton();
            _mainBuyButton?.Hide();

            _purchase.SetCost(GameConstants.TutorialBuyUnitCost);
            _panel.Show(GameConstants.TutorialBuyUnitCost);
            _panel.BuyButton.Clicked += OnBuyClicked;

            var overlay = ResolveOverlay();
            overlay?.Show(_panel.BuyButtonRect, useDimmer: false);
        }

        public void End()
        {
            if (_panel != null && _panel.BuyButton != null)
                _panel.BuyButton.Clicked -= OnBuyClicked;

            _panel?.Hide();
            _panel = null;

            ResolveOverlay()?.Hide();
            _mainBuyButton?.Hide();
            _mainBuyButton = null;
        }

        TutorialPurchasePanelView ResolvePanel()
        {
            if (_ui != null && _ui.TutorialPurchasePanel != null)
                return _ui.TutorialPurchasePanel;

            return Object.FindFirstObjectByType<TutorialPurchasePanelView>(FindObjectsInactive.Include);
        }

        BuyUnitButtonView ResolveMainBuyButton()
        {
            if (_ui != null && _ui.BuyUnitButton != null)
                return _ui.BuyUnitButton;

            return Object.FindFirstObjectByType<BuyUnitButtonView>(FindObjectsInactive.Include);
        }

        TutorialOverlayView ResolveOverlay()
        {
            if (_ui != null && _ui.TutorialOverlay != null)
                return _ui.TutorialOverlay;

            return Object.FindFirstObjectByType<TutorialOverlayView>(FindObjectsInactive.Include);
        }

        void OnBuyClicked()
        {
            if (_purchase == null || !_purchase.TryPurchase())
                return;

            End();
            _phases.ChangePhase(GamePhase.PostPurchaseCombat);
        }
    }
}
