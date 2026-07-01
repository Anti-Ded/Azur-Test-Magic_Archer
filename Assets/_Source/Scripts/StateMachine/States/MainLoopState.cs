using MagicArcher.Gameplay.Flow;
using MagicArcher.StateMachine;

namespace MagicArcher.StateMachine.States
{
    public sealed class MainLoopState : GameStateBase
    {
        readonly MainLoopDirector _director;

        public MainLoopState(MainLoopDirector director)
        {
            _director = director;
        }

        public override GamePhase Phase => GamePhase.MainLoop;

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
