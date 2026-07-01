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
        readonly RegularEnemyConfig _regular;
        readonly BossEnemyConfig _boss;

        bool _defeatHandled;

        [Inject]
        public CombatThreatMonitor(
            EnemyWaveController wave,
            IGridService grid,
            EnemyPool enemyPool,
            GamePhaseService phases,
            GameStateMachine stateMachine,
            RegularEnemyConfig regular,
            BossEnemyConfig boss,
            [Inject(Optional = true)] UnitDragController drag = null)
        {
            _wave = wave;
            _grid = grid;
            _enemyPool = enemyPool;
            _phases = phases;
            _stateMachine = stateMachine;
            _regular = regular;
            _boss = boss;
            _drag = drag;
        }

        public void Initialize()
        {
            _wave.EnemyReachedGrid += OnEnemyReachedGrid;
        }

        void OnEnemyReachedGrid(EnemyView enemy)
        {
            if (_defeatHandled || enemy == null)
                return;

            var phase = _stateMachine.CurrentPhase;
            if (phase is GamePhase.Defeat or GamePhase.Victory or GamePhase.Cta)
                return;

            enemy.Motor?.Stop();
            var damage = ResolveContactDamage(enemy);
            DamageAllUnits(damage);
            RemoveDeadUnits();
            _enemyPool?.Return(enemy);

            if (HasAliveUnit())
                return;

            _defeatHandled = true;
            _drag?.SetDragEnabled(false);
            _phases.ChangePhase(GamePhase.Defeat);
        }

        float ResolveContactDamage(EnemyView enemy)
        {
            if (enemy.Health != null && enemy.Health.IsBoss && _boss != null)
                return _boss.GridContactDamage;

            return _regular != null ? _regular.GridContactDamage : 50f;
        }

        void DamageAllUnits(float damage)
        {
            _grid.ForEachUnit(unit =>
            {
                if (unit?.Health == null || !unit.Health.IsAlive)
                    return;

                unit.Health.TakeDamage(damage);
            });
        }

        void RemoveDeadUnits()
        {
            for (var y = 0; y < _grid.Height; y++)
            {
                for (var x = 0; x < _grid.Width; x++)
                {
                    if (!_grid.TryGetUnit(x, y, out var unit) || unit == null)
                        continue;

                    if (unit.Health != null && unit.Health.IsAlive)
                        continue;

                    _grid.TryRemove(x, y, out _);
                    if (unit != null)
                        UnityEngine.Object.Destroy(unit.gameObject);
                }
            }
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
