using MagicArcher.Core;
using MagicArcher.Gameplay.Combat;
using MagicArcher.Gameplay.Economy;
using MagicArcher.Gameplay.Enemies;
using MagicArcher.Gameplay.Units;
using MagicArcher.StateMachine;
using MagicArcher.UI;
using UnityEngine;
using Zenject;

namespace MagicArcher.Gameplay.Flow
{
    public sealed class MainLoopDirector : MonoBehaviour
    {
        UnitPurchaseService _purchase;
        EnemyWaveController _wave;
        EnemyKillRewardService _killRewards;
        GamePhaseService _phases;
        CombatUiRefs _ui;

        bool _active;

        [Inject]
        void Construct(
            UnitPurchaseService purchase,
            EnemyWaveController wave,
            EnemyKillRewardService killRewards,
            GamePhaseService phases,
            [Inject(Optional = true)] CombatUiRefs ui = null)
        {
            _purchase = purchase;
            _wave = wave;
            _killRewards = killRewards;
            _phases = phases;
            _ui = ui;
        }

        public void Begin()
        {
            _active = true;
            _purchase.SetCost(GameConstants.InitialBuyUnitCost);
            _purchase.UnitPurchased += OnUnitPurchased;
            _wave.OrcDied += OnOrcDied;

            var buyButton = ResolveBuyButton();
            if (buyButton != null)
            {
                buyButton.Clicked += OnBuyClicked;
                buyButton.Show(GameConstants.InitialBuyUnitCost);
            }

            _wave.ActivateNextWalker();
        }

        public void End()
        {
            _active = false;

            var buyButton = ResolveBuyButton();
            if (buyButton != null)
            {
                buyButton.Clicked -= OnBuyClicked;
                buyButton.Hide();
            }

            if (_purchase != null)
                _purchase.UnitPurchased -= OnUnitPurchased;

            if (_wave != null)
                _wave.OrcDied -= OnOrcDied;
        }

        void OnBuyClicked()
        {
            if (_purchase == null || !_purchase.TryPurchase())
                return;

            var buyButton = ResolveBuyButton();
            if (buyButton != null)
                buyButton.Show(_purchase.CurrentCost);
        }

        void OnUnitPurchased(UnitView _)
        {
            _purchase.SetCost(_purchase.CurrentCost + GameConstants.BuyUnitCostStep);
        }

        void OnOrcDied(EnemyView orc)
        {
            if (!_active || orc == null)
                return;

            _killRewards.Grant(orc);

            if (orc.Health != null && orc.Health.IsBoss)
            {
                HandleVictory();
                return;
            }

            _wave.ActivateNextWalker();
        }

        void HandleVictory()
        {
            _active = false;
            End();
            _phases.ChangePhase(GamePhase.Victory);
        }

        BuyUnitButtonView ResolveBuyButton()
        {
            if (_ui != null && _ui.BuyUnitButton != null)
                return _ui.BuyUnitButton;

            return Object.FindFirstObjectByType<BuyUnitButtonView>(FindObjectsInactive.Include);
        }
    }
}
