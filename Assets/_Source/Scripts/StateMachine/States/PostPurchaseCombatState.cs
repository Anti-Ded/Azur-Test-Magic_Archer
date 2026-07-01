using MagicArcher.Gameplay.Flow;
using MagicArcher.StateMachine;

namespace MagicArcher.StateMachine.States
{
    public sealed class PostPurchaseCombatState : GameStateBase
    {
        readonly PostPurchaseCombatDirector _director;

        public PostPurchaseCombatState(PostPurchaseCombatDirector director)
        {
            _director = director;
        }

        public override GamePhase Phase => GamePhase.PostPurchaseCombat;

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
