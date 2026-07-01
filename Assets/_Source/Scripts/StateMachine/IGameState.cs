namespace MagicArcher.StateMachine
{
    public interface IGameState
    {
        GamePhase Phase { get; }
        void Enter();
        void Exit();
        void Tick();
    }
}
