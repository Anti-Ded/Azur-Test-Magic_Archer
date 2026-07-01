using MagicArcher.Gameplay.Flow;
using MagicArcher.StateMachine;

namespace MagicArcher.StateMachine.States
{
    public sealed class TutorialMergeState : GameStateBase
    {
        readonly TutorialMergeDirector _director;

        public TutorialMergeState(TutorialMergeDirector director)
        {
            _director = director;
        }

        public override GamePhase Phase => GamePhase.TutorialMerge;

        public override void Enter()
        {
            _director.Begin();
        }

        public override void Exit()
        {
            _director.End();
        }
    }
}
