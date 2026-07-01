using MagicArcher.Core;
using MagicArcher.Gameplay.Combat;
using MagicArcher.Gameplay.Economy;
using MagicArcher.Gameplay.Enemies;
using MagicArcher.StateMachine;
using UnityEngine;
using Zenject;

namespace MagicArcher.Gameplay.Flow
{
    public sealed class PostPurchaseCombatDirector : MonoBehaviour
    {
        const int OrcsToKill = 2;

        EnemyWaveController _wave;
        IEconomyService _economy;
        CoinFlyVfxService _coinFly;
        GamePhaseService _phases;

        int _killsRemaining;
        bool _active;

        [Inject]
        void Construct(
            EnemyWaveController wave,
            IEconomyService economy,
            GamePhaseService phases,
            CoinFlyVfxService coinFly)
        {
            _wave = wave;
            _economy = economy;
            _phases = phases;
            _coinFly = coinFly;
        }

        public void Begin()
        {
            _killsRemaining = OrcsToKill;
            _active = true;
            _wave.OrcDied += OnOrcDied;
            _wave.ActivateNextWalker();
        }

        public void End()
        {
            _active = false;
            _wave.OrcDied -= OnOrcDied;
        }

        void OnOrcDied(EnemyView orc)
        {
            if (!_active)
                return;

            _coinFly.Play(orc.transform.position, () =>
                _economy.AddCoins(GameConstants.CoinsPerOrcKill));

            _killsRemaining--;
            if (_killsRemaining > 0)
            {
                _wave.ActivateNextWalker();
                return;
            }

            End();
            _phases.ChangePhaseAfterDelay(GamePhase.TutorialMerge, 1.2f);
        }
    }
}
