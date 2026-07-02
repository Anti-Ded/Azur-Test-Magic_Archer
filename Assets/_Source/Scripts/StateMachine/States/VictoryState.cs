using MagicArcher.Core.Audio;
using MagicArcher.Core.Cta;
using MagicArcher.Gameplay.Grid;
using MagicArcher.UI;
using UnityEngine;

namespace MagicArcher.StateMachine.States
{
    public sealed class VictoryState : GameStateBase
    {
        readonly ICtaService _ctaService;
        readonly IAudioService _audio;
        readonly IGridService _grid;
        readonly CombatUiRefs _ui;
        readonly EndgameOverlayView _overlay;

        public VictoryState(
            ICtaService ctaService,
            IGridService grid,
            [Zenject.Inject(Optional = true)] IAudioService audio = null,
            [Zenject.Inject(Optional = true)] CombatUiRefs ui = null,
            [Zenject.Inject(Optional = true)] EndgameOverlayView overlay = null)
        {
            _ctaService = ctaService;
            _grid = grid;
            _audio = audio;
            _ui = ui;
            _overlay = overlay;
        }

        public override GamePhase Phase => GamePhase.Victory;

        public override void Enter()
        {
            _ui?.TutorialPurchasePanel?.Hide();
            _ui?.TutorialOverlay?.Hide();
            _ui?.BuyUnitButton?.Hide();

            _audio?.PlayVictory();
            _grid?.ForEachUnit(unit => unit.PlayVictory());
            ResolveOverlay()?.ShowVictory();
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
