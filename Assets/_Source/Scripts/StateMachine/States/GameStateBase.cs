using MagicArcher.StateMachine;

namespace MagicArcher.StateMachine.States
{
    public abstract class GameStateBase : IGameState
    {
        public abstract GamePhase Phase { get; }

        public virtual void Enter() { }
        public virtual void Exit() { }
        public virtual void Tick() { }
    }
}
