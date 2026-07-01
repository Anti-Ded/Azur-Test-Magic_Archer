using MagicArcher.StateMachine;
using MagicArcher.StateMachine.States;
using Zenject;

namespace MagicArcher.Installers
{
    public sealed class StateMachineInitializer : IInitializable
    {
        readonly GameStateMachine _stateMachine;
        readonly IntroCombatState _introCombatState;
        readonly TutorialPurchaseState _tutorialPurchaseState;
        readonly PostPurchaseCombatState _postPurchaseCombatState;
        readonly TutorialMergeState _tutorialMergeState;
        readonly MainLoopState _mainLoopState;
        readonly VictoryState _victoryState;
        readonly DefeatState _defeatState;
        readonly CtaState _ctaState;

        public StateMachineInitializer(
            GameStateMachine stateMachine,
            IntroCombatState introCombatState,
            TutorialPurchaseState tutorialPurchaseState,
            PostPurchaseCombatState postPurchaseCombatState,
            TutorialMergeState tutorialMergeState,
            MainLoopState mainLoopState,
            VictoryState victoryState,
            DefeatState defeatState,
            CtaState ctaState)
        {
            _stateMachine = stateMachine;
            _introCombatState = introCombatState;
            _tutorialPurchaseState = tutorialPurchaseState;
            _postPurchaseCombatState = postPurchaseCombatState;
            _tutorialMergeState = tutorialMergeState;
            _mainLoopState = mainLoopState;
            _victoryState = victoryState;
            _defeatState = defeatState;
            _ctaState = ctaState;
        }

        public void Initialize()
        {
            _stateMachine.Register(_introCombatState);
            _stateMachine.Register(_tutorialPurchaseState);
            _stateMachine.Register(_postPurchaseCombatState);
            _stateMachine.Register(_tutorialMergeState);
            _stateMachine.Register(_mainLoopState);
            _stateMachine.Register(_victoryState);
            _stateMachine.Register(_defeatState);
            _stateMachine.Register(_ctaState);
        }
    }
}
