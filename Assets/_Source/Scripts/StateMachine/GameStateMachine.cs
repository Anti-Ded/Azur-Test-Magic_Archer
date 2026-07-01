using System.Collections.Generic;

namespace MagicArcher.StateMachine
{
    public sealed class GameStateMachine
    {
        readonly Dictionary<GamePhase, IGameState> _states = new();
        IGameState _current;

        public GamePhase CurrentPhase => _current?.Phase ?? GamePhase.IntroCombat;

        public void Register(IGameState state)
        {
            _states[state.Phase] = state;
        }

        public void ChangeState(GamePhase phase)
        {
            if (_current != null && _current.Phase == phase)
                return;

            if (_current != null)
                _current.Exit();

            _current = _states[phase];
            _current.Enter();
        }

        public void Tick()
        {
            _current?.Tick();
        }
    }
}
