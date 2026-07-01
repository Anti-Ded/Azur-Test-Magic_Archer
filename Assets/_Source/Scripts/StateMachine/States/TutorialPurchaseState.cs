using MagicArcher.Gameplay.Flow;
using MagicArcher.StateMachine;

namespace MagicArcher.StateMachine.States
{
    public sealed class TutorialPurchaseState : GameStateBase
    {
        readonly TutorialPurchaseDirector _director;

        public TutorialPurchaseState(TutorialPurchaseDirector director)
        {
            _director = director;
        }

        public override GamePhase Phase => GamePhase.TutorialPurchase;

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
