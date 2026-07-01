using MagicArcher.Core;
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

        CombatSceneRefs _refs;

        IEconomyService _economy;

        GamePhaseService _phases;

        CoinFlyVfxService _coinFly;

        EnemyWaveController _wave;

        RegularEnemyConfig _regular;



        ProjectilePool _projectilePool;

        bool _introKillHandled;



        [Inject]

        void Construct(

            DiContainer container,

            LevelRoot level,

            IGridService grid,

            CombatSceneRefs refs,

            IEconomyService economy,

            GamePhaseService phases,

            CoinFlyVfxService coinFly,

            ProjectilePool projectilePool,

            EnemyWaveController wave,

            RegularEnemyConfig regular)

        {

            _container = container;

            _level = level;

            _grid = grid;

            _refs = refs;

            _economy = economy;

            _phases = phases;

            _coinFly = coinFly;

            _projectilePool = projectilePool;

            _wave = wave;

            _regular = regular;

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



            if (_refs.ProjectilePrefab != null && _refs.ProjectilesRoot != null)

                _projectilePool.Warmup(8);



            SpawnArcher();

            SpawnOrcQueue(enemyPath);

        }



        void SpawnArcher()

        {

            if (_refs.ArcherPrefab == null || _level.UnitsRoot == null)

                return;



            var archer = _container.InstantiatePrefabForComponent<UnitView>(

                _refs.ArcherPrefab,

                _level.UnitsRoot);



            if (!_grid.TryPlace(ArcherGridX, ArcherGridY, archer))

            {

                archer.transform.position = _grid.GetWorldPosition(ArcherGridX, ArcherGridY);

                archer.SetGridPosition(ArcherGridX, ArcherGridY);

            }



            archer.SetUpgraded(false);

        }



        void SpawnOrcQueue(EnemyPathView enemyPath)

        {

            var prefab = ResolveEnemyPrefab();

            if (prefab == null || enemyPath == null)

                return;



            _wave.PrewarmRegularLine(prefab, _regular != null ? _regular.IntroQueueCount : 16);

        }



        EnemyView ResolveEnemyPrefab()
        {
            if (_regular != null && _regular.Prefab != null)
                return _regular.Prefab;

            return _refs != null ? _refs.OrcPrefab : null;
        }



        void OnOrcDied(EnemyView orc)

        {

            if (_introKillHandled)

                return;



            _introKillHandled = true;

            _coinFly.Play(orc.transform.position, () =>
                _economy.AddCoins(GameConstants.CoinsPerOrcKill));

            _wave.OrcDied -= OnOrcDied;

            _phases.ChangePhaseAfterDelay(GamePhase.TutorialPurchase, 1.2f);

        }

    }

}
