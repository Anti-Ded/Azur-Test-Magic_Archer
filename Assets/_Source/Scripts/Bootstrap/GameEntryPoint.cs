using MagicArcher.Core.Audio;
using MagicArcher.StateMachine;
using Zenject;

namespace MagicArcher.Bootstrap
{
    public sealed class GameEntryPoint : IInitializable
    {
        readonly GameStateMachine _stateMachine;
        readonly IAudioService _audio;

        public GameEntryPoint(
            GameStateMachine stateMachine,
            [Inject(Optional = true)] IAudioService audio = null)
        {
            _stateMachine = stateMachine;
            _audio = audio;
        }

        public void Initialize()
        {
            _audio?.PlayMusic();
            _stateMachine.ChangeState(GamePhase.IntroCombat);
        }
    }
}
