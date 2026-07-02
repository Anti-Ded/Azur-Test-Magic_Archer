using MagicArcher.Core.Config;
using MagicArcher.Gameplay.Combat;
using MagicArcher.Gameplay.Enemies;
using MagicArcher.Gameplay.Grid;
using MagicArcher.Gameplay.Units;
using MagicArcher.StateMachine;
using Zenject;

namespace MagicArcher.Gameplay.Flow
{
    public sealed class CombatThreatMonitor : IInitializable
    {
        readonly EnemyWaveController _wave;
        readonly IGridService _grid;
        readonly EnemyPool _enemyPool;
        readonly UnitDragController _drag;
        readonly GamePhaseService _phases;
        readonly GameStateMachine _stateMachine;

        bool _defeatHandled;

        [Inject]
        public CombatThreatMonitor(
            EnemyWaveController wave,
            IGridService grid,
            EnemyPool enemyPool,
            GamePhaseService phases,
            GameStateMachine stateMachine,
            [Inject(Optional = true)] UnitDragController drag = null)
        {
            _wave = wave;
            _grid = grid;
            _enemyPool = enemyPool;
            _phases = phases;
            _stateMachine = stateMachine;
            _drag = drag;
        }

        public void Initialize()
        {
            _wave.EnemyReachedGrid += OnEnemyReachedGrid;
        }

        public void TryHandleDefeat()
        {
            if (_defeatHandled)
                return;

            var phase = _stateMachine.CurrentPhase;
            if (phase is GamePhase.Defeat or GamePhase.Victory or GamePhase.Cta)
                return;

            if (HasAliveUnit())
                return;

            _defeatHandled = true;
            _drag?.SetDragEnabled(false);
            _phases.ChangePhase(GamePhase.Defeat);
        }

        void OnEnemyReachedGrid(EnemyView enemy)
        {
            if (enemy == null)
                return;

            var phase = _stateMachine.CurrentPhase;
            if (phase is GamePhase.Defeat or GamePhase.Victory or GamePhase.Cta)
                return;

            enemy.Motor?.Stop();
            _enemyPool?.Return(enemy);
        }

        bool HasAliveUnit()
        {
            var alive = false;
            _grid.ForEachUnit(unit =>
            {
                if (unit?.Health != null && unit.Health.IsAlive)
                    alive = true;
            });

            return alive;
        }
    }
}
