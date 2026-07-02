using MagicArcher.Core.Audio;
using MagicArcher.Core.Cta;
using MagicArcher.UI;
using UnityEngine;

namespace MagicArcher.StateMachine.States
{
    public sealed class DefeatState : GameStateBase
    {
        readonly ICtaService _ctaService;
        readonly IAudioService _audio;
        readonly CombatUiRefs _ui;
        readonly EndgameOverlayView _overlay;

        public DefeatState(
            ICtaService ctaService,
            [Zenject.Inject(Optional = true)] IAudioService audio = null,
            [Zenject.Inject(Optional = true)] CombatUiRefs ui = null,
            [Zenject.Inject(Optional = true)] EndgameOverlayView overlay = null)
        {
            _ctaService = ctaService;
            _audio = audio;
            _ui = ui;
            _overlay = overlay;
        }

        public override GamePhase Phase => GamePhase.Defeat;

        public override void Enter()
        {
            _ui?.TutorialPurchasePanel?.Hide();
            _ui?.TutorialOverlay?.Hide();
            _ui?.BuyUnitButton?.Hide();

            _audio?.PlayDefeat();
            ResolveOverlay()?.ShowDefeat();
            _ctaService.Activate();
        }

        EndgameOverlayView ResolveOverlay()
        {
            if (_ui != null && _ui.EndgameOverlay != null)
                return _ui.EndgameOverlay;

            if (_overlay != null)
                return _overlay;

            return Object.FindFirstObjectByType<EndgameOverlayView>(FindObjectsInactive.Include);
        }
    }
}
