using MagicArcher.Core;
using MagicArcher.Gameplay.Grid;
using MagicArcher.Gameplay.Units;
using MagicArcher.StateMachine;
using MagicArcher.UI;
using UnityEngine;
using Zenject;

namespace MagicArcher.Gameplay.Flow
{
    public sealed class TutorialMergeDirector : MonoBehaviour
    {
        UnitMergeService _merge;
        UnitDragController _drag;
        IGridService _grid;
        GamePhaseService _phases;
        CombatUiRefs _ui;

        [Inject]
        void Construct(
            UnitMergeService merge,
            UnitDragController drag,
            IGridService grid,
            GamePhaseService phases,
            [Inject(Optional = true)] CombatUiRefs ui = null)
        {
            _merge = merge;
            _drag = drag;
            _grid = grid;
            _phases = phases;
            _ui = ui;
        }

        public void Begin()
        {
            if (_merge == null || _drag == null || _phases == null)
            {
                Debug.LogError("TutorialMergeDirector: missing dependencies.");
                return;
            }

            _merge.UnitMerged += OnUnitMerged;
            _drag.SetDragEnabled(true);
            ShowMergeHint();
        }

        public void End()
        {
            if (_merge != null)
                _merge.UnitMerged -= OnUnitMerged;

            _drag?.SetDragEnabled(false);
            ResolveOverlay()?.Hide();
        }

        bool TryFindMergeTargets(out UnitView sourceUnit, out Vector3 sourcePosition, out Vector3 targetPosition)
        {
            sourceUnit = null;
            sourcePosition = default;
            targetPosition = default;
            UnitView first = null;
            UnitView second = null;

            for (var y = 0; y < _grid.Height; y++)
            {
                for (var x = 0; x < _grid.Width; x++)
                {
                    if (!_grid.TryGetUnit(x, y, out var unit) || !unit.CanMerge)
                        continue;

                    if (first == null)
                        first = unit;
                    else
                    {
                        second = unit;
                        break;
                    }
                }

                if (second != null)
                    break;
            }

            if (first == null)
                return false;

            if (second == null)
                second = first;

            sourceUnit = first;
            sourcePosition = first.transform.position;
            targetPosition = second.transform.position;
            return true;
        }

        void ShowMergeHint()
        {
            var overlay = ResolveOverlay();
            if (overlay == null)
                return;

            if (!TryFindMergeTargets(out var sourceUnit, out var sourcePosition, out var targetPosition))
            {
                overlay.Show(null);
                return;
            }

            overlay.PlayMergeDragHint(sourceUnit, sourcePosition, targetPosition);
        }

        void OnUnitMerged(UnitView _)
        {
            End();
            _phases.ChangePhaseAfterDelay(GamePhase.MainLoop, 1f);
        }

        TutorialOverlayView ResolveOverlay()
        {
            if (_ui != null && _ui.TutorialOverlay != null)
                return _ui.TutorialOverlay;

            return Object.FindFirstObjectByType<TutorialOverlayView>(FindObjectsInactive.Include);
        }
    }
}
