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
        const int TutorialBuyButtonSiblingIndex = 1;

        UnitPurchaseService _purchase;
        CombatUiRefs _ui;
        GamePhaseService _phases;

        BuyUnitButtonView _activeBuyButton;
        Transform _buyButtonOriginalParent;
        int _buyButtonOriginalSiblingIndex;
        bool _buyButtonPlacementSaved;

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

            var buyButton = ResolveBuyButton();
            var overlay = ResolveOverlay();
            if (buyButton == null || overlay == null)
            {
                Debug.LogError("TutorialPurchaseDirector: UI references are missing. Run Magic Archer → Setup Combat Scene.");
                return;
            }

            _activeBuyButton = buyButton;
            _purchase.SetCost(GameConstants.TutorialBuyUnitCost);
            buyButton.Clicked += OnBuyClicked;
            buyButton.Show(GameConstants.TutorialBuyUnitCost);

            ReparentBuyButtonForTutorial(buyButton, overlay);

            var buttonRect = buyButton.transform as RectTransform;
            overlay.Show(buttonRect);
        }

        BuyUnitButtonView ResolveBuyButton()
        {
            if (_ui != null && _ui.BuyUnitButton != null)
                return _ui.BuyUnitButton;

            var buttons = Object.FindObjectsByType<BuyUnitButtonView>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            foreach (var button in buttons)
            {
                if (button.transform.parent != null &&
                    button.transform.parent.name == "TutorialOverlay")
                    continue;

                return button;
            }

            return null;
        }

        TutorialOverlayView ResolveOverlay()
        {
            if (_ui != null && _ui.TutorialOverlay != null)
                return _ui.TutorialOverlay;

            return Object.FindFirstObjectByType<TutorialOverlayView>(FindObjectsInactive.Include);
        }

        public void End()
        {
            var buyButton = _activeBuyButton != null ? _activeBuyButton : ResolveBuyButton();
            var overlay = ResolveOverlay();

            if (buyButton != null)
            {
                buyButton.Clicked -= OnBuyClicked;
                buyButton.Hide();
                RestoreBuyButtonParent(buyButton);
            }

            _activeBuyButton = null;
            overlay?.Hide();
        }

        void ReparentBuyButtonForTutorial(BuyUnitButtonView buyButton, TutorialOverlayView overlay)
        {
            if (buyButton == null || overlay == null)
                return;

            var buttonTransform = buyButton.transform;
            if (!_buyButtonPlacementSaved)
            {
                _buyButtonOriginalParent = buttonTransform.parent;
                _buyButtonOriginalSiblingIndex = buttonTransform.GetSiblingIndex();
                _buyButtonPlacementSaved = true;
            }

            buttonTransform.SetParent(overlay.transform, true);
            buttonTransform.SetSiblingIndex(TutorialBuyButtonSiblingIndex);
        }

        void RestoreBuyButtonParent(BuyUnitButtonView buyButton)
        {
            if (buyButton == null || !_buyButtonPlacementSaved || _buyButtonOriginalParent == null)
                return;

            var buttonTransform = buyButton.transform;
            buttonTransform.SetParent(_buyButtonOriginalParent, true);
            buttonTransform.SetSiblingIndex(_buyButtonOriginalSiblingIndex);
            _buyButtonPlacementSaved = false;
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
