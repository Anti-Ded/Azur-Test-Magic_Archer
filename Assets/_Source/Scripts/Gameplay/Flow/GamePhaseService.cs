using MagicArcher.StateMachine;
using UnityEngine;

namespace MagicArcher.Gameplay.Flow
{
    public sealed class GamePhaseService
    {
        readonly GameStateMachine _stateMachine;
        readonly CombatPhaseScheduler _scheduler;

        public GamePhase CurrentPhase => _stateMachine.CurrentPhase;

        public bool IsTutorialCombatPaused =>
            CurrentPhase is GamePhase.TutorialPurchase or GamePhase.TutorialMerge;

        [Zenject.Inject]
        public GamePhaseService(GameStateMachine stateMachine, CombatPhaseScheduler scheduler)
        {
            _stateMachine = stateMachine;
            _scheduler = scheduler;
        }

        public void ChangePhase(GamePhase phase)
        {
            if (_stateMachine == null)
            {
                Debug.LogError("GamePhaseService: state machine is missing.");
                return;
            }

            _stateMachine.ChangeState(phase);
        }

        public void ChangePhaseAfterDelay(GamePhase phase, float delaySeconds)
        {
            if (_scheduler == null)
            {
                ChangePhase(phase);
                return;
            }

            _scheduler.RunAfterDelay(delaySeconds, () => ChangePhase(phase));
        }
    }
}
