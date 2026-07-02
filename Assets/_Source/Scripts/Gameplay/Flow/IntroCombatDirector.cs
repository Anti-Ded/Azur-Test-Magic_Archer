using MagicArcher.Core.Config;
using MagicArcher.Gameplay.Combat;
using MagicArcher.Gameplay.Economy;
using MagicArcher.Gameplay.Enemies;
using MagicArcher.Gameplay.Grid;
using MagicArcher.Gameplay.Level;
using MagicArcher.Gameplay.Units;
using MagicArcher.StateMachine;
using UnityEngine;
using Zenject;

namespace MagicArcher.Gameplay.Flow
{
    public sealed class IntroCombatDirector : MonoBehaviour
    {
        const int ArcherGridX = 2;
        const int ArcherGridY = 1;

        DiContainer _container;
        LevelRoot _level;
        IGridService _grid;
        CombatSceneRefs _sceneRefs;
        IEconomyService _economy;
        GamePhaseService _phases;
        EnemyKillRewardService _killRewards;
        EnemyWaveController _wave;
        RegularEnemyConfig _regular;
        RegularUnitConfig _regularUnit;
        ProjectilePool _projectilePool;
        bool _introKillHandled;

        [Inject]
        void Construct(
            DiContainer container,
            LevelRoot level,
            IGridService grid,
            CombatSceneRefs sceneRefs,
            IEconomyService economy,
            GamePhaseService phases,
            EnemyKillRewardService killRewards,
            ProjectilePool projectilePool,
            EnemyWaveController wave,
            RegularEnemyConfig regular,
            RegularUnitConfig regularUnit)
        {
            _container = container;
            _level = level;
            _grid = grid;
            _sceneRefs = sceneRefs;
            _economy = economy;
            _phases = phases;
            _killRewards = killRewards;
            _projectilePool = projectilePool;
            _wave = wave;
            _regular = regular;
            _regularUnit = regularUnit;
        }

        public void Begin()
        {
            var enemyPath = _level != null && _level.EnemyPathRoot != null
                ? _level.EnemyPathRoot.GetComponent<EnemyPathView>()
                : null;

            _introKillHandled = false;
            _economy.ResetCoins();
            _wave.ResetWave(enemyPath);
            _wave.OrcDied += OnOrcDied;

            var projectile = _regularUnit != null ? _regularUnit.ProjectilePrefab : null;
            if (projectile != null && _sceneRefs.ProjectilesRoot != null)
                _projectilePool.Warmup(projectile, 8);

            SpawnArcher();
            SpawnOrcQueue(enemyPath);
        }

        void SpawnArcher()
        {
            var archerPrefab = _regularUnit != null ? _regularUnit.UnitViewPrefab : null;
            if (archerPrefab == null || _level.UnitsRoot == null)
                return;

            var archer = _container.InstantiatePrefabForComponent<UnitView>(
                archerPrefab,
                _level.UnitsRoot);

            if (!_grid.TryPlace(ArcherGridX, ArcherGridY, archer))
            {
                archer.transform.position = _grid.GetWorldPosition(ArcherGridX, ArcherGridY);
                archer.SetGridPosition(ArcherGridX, ArcherGridY);
            }

            archer.ApplyConfig(_regularUnit);
        }

        void SpawnOrcQueue(EnemyPathView enemyPath)
        {
            var prefab = _regular != null ? _regular.Prefab : null;
            if (prefab == null || enemyPath == null)
                return;

            _wave.PrewarmBoss(prefab);
            _wave.PrewarmRegularLine(prefab, _regular != null ? _regular.IntroQueueCount : 16);
        }

        void OnOrcDied(EnemyView orc)
        {
            if (_introKillHandled)
                return;

            _introKillHandled = true;
            _killRewards.Grant(orc);

            _wave.OrcDied -= OnOrcDied;
            _phases.ChangePhaseAfterDelay(GamePhase.TutorialPurchase, 1.2f);
        }
    }
}
