using MagicArcher.Gameplay.Flow;
using MagicArcher.StateMachine;

namespace MagicArcher.StateMachine.States
{
    public sealed class IntroCombatState : GameStateBase
    {
        readonly IntroCombatDirector _director;

        public IntroCombatState(IntroCombatDirector director)
        {
            _director = director;
        }

        public override GamePhase Phase => GamePhase.IntroCombat;

        public override void Enter()
        {
            _director.Begin();
        }
    }
}
