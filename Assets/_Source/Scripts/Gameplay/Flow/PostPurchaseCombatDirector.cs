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
        EnemyKillRewardService _killRewards;
        GamePhaseService _phases;

        int _killsRemaining;
        bool _active;

        [Inject]
        void Construct(
            EnemyWaveController wave,
            EnemyKillRewardService killRewards,
            GamePhaseService phases)
        {
            _wave = wave;
            _killRewards = killRewards;
            _phases = phases;
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
            if (!_active || orc == null)
                return;

            _killRewards.Grant(orc);

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
