using MagicArcher.Bootstrap;
using MagicArcher.Core.Audio;
using MagicArcher.Core.Config;
using MagicArcher.Core.Cta;
using MagicArcher.Gameplay.Combat;
using MagicArcher.Gameplay.Economy;
using MagicArcher.Gameplay.Flow;
using MagicArcher.Gameplay.Grid;
using MagicArcher.Gameplay.Level;
using MagicArcher.Gameplay.Units;
using MagicArcher.StateMachine;
using MagicArcher.StateMachine.States;
using MagicArcher.UI;
using UnityEngine;
using Zenject;

namespace MagicArcher.Installers
{
    public sealed class GameInstaller : MonoInstaller
    {
        [SerializeField] float _cellSize = 1f;
        [SerializeField] RegularEnemyConfig _regularEnemyConfig;
        [SerializeField] BossEnemyConfig _bossEnemyConfig;
        [SerializeField] RegularUnitConfig _regularUnitConfig;
        [SerializeField] UpgradedUnitConfig _upgradedUnitConfig;

        public override void InstallBindings()
        {
            Container.Bind<RegularEnemyConfig>().FromMethod(ResolveRegularEnemyConfig).AsSingle();
            Container.Bind<BossEnemyConfig>().FromMethod(ResolveBossEnemyConfig).AsSingle();
            Container.Bind<RegularUnitConfig>().FromMethod(ResolveRegularUnitConfig).AsSingle();
            Container.Bind<UpgradedUnitConfig>().FromMethod(ResolveUpgradedUnitConfig).AsSingle();
            Container.Bind<ICtaService>().To<CtaService>().AsSingle();
            Container.Bind<IAudioService>()
                .FromMethod(ctx =>
                {
                    var host = Object.FindFirstObjectByType<AudioHost>();
                    if (host == null)
                    {
                        var root = GameObject.Find("CombatSystems") ?? new GameObject("CombatSystems");
                        host = root.GetComponent<AudioHost>() ?? root.AddComponent<AudioHost>();
                    }

                    return host.CreateService();
                })
                .AsSingle();
            Container.Bind<GameStateMachine>().AsSingle();
            Container.Bind<CombatPhaseScheduler>()
                .FromMethod(_ =>
                {
                    var existing = Object.FindFirstObjectByType<CombatPhaseScheduler>();
                    if (existing != null)
                        return existing;

                    var root = GameObject.Find("CombatSystems") ?? new GameObject("CombatSystems");
                    return root.GetComponent<CombatPhaseScheduler>()
                           ?? root.AddComponent<CombatPhaseScheduler>();
                })
                .AsSingle();
            Container.Bind<GamePhaseService>().AsSingle();
            Container.BindInterfacesAndSelfTo<CombatThreatMonitor>().AsSingle();
            Container.Bind<CombatTargetRegistry>().AsSingle();
            Container.BindInterfacesAndSelfTo<EconomyService>().AsSingle();
            Container.BindInterfacesAndSelfTo<UnitPurchaseService>().AsSingle();
            Container.BindInterfacesAndSelfTo<UnitMergeService>().AsSingle();

            Container.BindInterfacesAndSelfTo<IntroCombatState>().AsSingle();
            Container.BindInterfacesAndSelfTo<TutorialPurchaseState>().AsSingle();
            Container.BindInterfacesAndSelfTo<PostPurchaseCombatState>().AsSingle();
            Container.BindInterfacesAndSelfTo<TutorialMergeState>().AsSingle();
            Container.BindInterfacesAndSelfTo<MainLoopState>().AsSingle();
            Container.BindInterfacesAndSelfTo<VictoryState>().AsSingle();
            Container.BindInterfacesAndSelfTo<DefeatState>().AsSingle();
            Container.BindInterfacesAndSelfTo<CtaState>().AsSingle();

            Container.Bind<LevelRoot>().FromComponentInHierarchy().AsSingle();
            Container.Bind<CombatSceneRefs>().FromComponentInHierarchy().AsSingle();
            Container.Bind<CoinFlyVfxService>()
                .FromMethod(ctx =>
                {
                    var refs = ctx.Container.Resolve<CombatSceneRefs>();
                    var poolRoot = refs.CoinFlyPoolRoot;
                    if (poolRoot == null)
                    {
                        var canvas = Object.FindFirstObjectByType<Canvas>();
                        poolRoot = canvas != null ? canvas.transform as RectTransform : null;
                    }

                    return new CoinFlyVfxService(refs.CoinFlyPrefab, poolRoot, refs);
                })
                .AsSingle();
            Container.Bind<CtaFeedbackView>().FromMethod(_ =>
            {
                var ui = Object.FindFirstObjectByType<CombatUiRefs>(FindObjectsInactive.Include);
                if (ui != null && ui.CtaFeedback != null)
                    return ui.CtaFeedback;

                return Object.FindFirstObjectByType<CtaFeedbackView>(FindObjectsInactive.Include);
            }).AsSingle();
            Container.Bind<CombatUiRefs>().FromMethod(_ =>
            {
                var refs = Object.FindFirstObjectByType<CombatUiRefs>(FindObjectsInactive.Include);
                if (refs != null)
                    return refs;

                var canvas = Object.FindFirstObjectByType<Canvas>();
                return canvas != null ? canvas.gameObject.AddComponent<CombatUiRefs>() : null;
            }).AsSingle();
            Container.BindInterfacesAndSelfTo<HealthBarUiService>()
                .FromMethod(ctx =>
                {
                    var ui = ctx.Container.Resolve<CombatUiRefs>();
                    var canvas = ui != null
                        ? ui.GetComponent<Canvas>() ?? ui.GetComponentInParent<Canvas>()
                        : Object.FindFirstObjectByType<Canvas>();
                    var service = new HealthBarUiService();
                    HealthBarUiBootstrap.Configure(service, ui, canvas);
                    return service;
                })
                .AsSingle()
                .NonLazy();
            Container.Bind<CurrencyHud>().FromComponentInHierarchy().AsSingle();
            Container.Bind<EndgameOverlayView>().FromMethod(_ =>
                Object.FindFirstObjectByType<EndgameOverlayView>(FindObjectsInactive.Include)).AsSingle();
            Container.BindInterfacesAndSelfTo<IntroCombatDirector>().FromComponentInHierarchy().AsSingle();
            Container.Bind<TutorialPurchaseDirector>().FromComponentInHierarchy().AsSingle();
            Container.Bind<PostPurchaseCombatDirector>().FromComponentInHierarchy().AsSingle();
            Container.Bind<TutorialMergeDirector>().FromComponentInHierarchy().AsSingle();
            Container.Bind<MainLoopDirector>().FromComponentInHierarchy().AsSingle();
            Container.Bind<EnemyWaveController>().FromComponentInHierarchy().AsSingle();
            Container.Bind<UnitDragController>()
                .FromMethod(_ =>
                {
                    var existing = Object.FindFirstObjectByType<UnitDragController>();
                    if (existing != null)
                        return existing;

                    var root = GameObject.Find("CombatSystems") ?? new GameObject("CombatSystems");
                    return root.GetComponent<UnitDragController>() ?? root.AddComponent<UnitDragController>();
                })
                .AsSingle();
            Container.Bind<MergeVfx>()
                .FromMethod(_ =>
                {
                    var existing = Object.FindFirstObjectByType<MergeVfx>();
                    if (existing != null)
                        return existing;

                    var root = GameObject.Find("CombatSystems") ?? new GameObject("CombatSystems");
                    return root.GetComponent<MergeVfx>() ?? root.AddComponent<MergeVfx>();
                })
                .AsSingle();

            Container.Bind<IGridService>()
                .FromMethod(ctx =>
                {
                    var level = ctx.Container.Resolve<LevelRoot>();
                    var origin = level.GridOrigin;
                    var board = level.GridRoot != null ? level.GridRoot.GetComponent<GridBoardView>() : null;
                    var cellSize = board != null ? board.CellSize : level.CellSize;
                    if (cellSize <= 0f)
                        cellSize = _cellSize;
                    return new GridService(origin, cellSize);
                })
                .AsSingle();

            Container.Bind<ProjectilePool>()
                .FromMethod(ctx =>
                {
                    var refs = ctx.Container.Resolve<CombatSceneRefs>();
                    var phases = ctx.Container.Resolve<GamePhaseService>();
                    var root = refs.ProjectilesRoot != null
                        ? refs.ProjectilesRoot
                        : ctx.Container.Resolve<LevelRoot>().transform;
                    return new ProjectilePool(refs.ProjectilePrefab, root, phases);
                })
                .AsSingle();
            Container.Bind<EnemyPool>()
                .FromMethod(ctx =>
                {
                    var refs = ctx.Container.Resolve<CombatSceneRefs>();
                    var regular = ctx.Container.Resolve<RegularEnemyConfig>();
                    var level = ctx.Container.Resolve<LevelRoot>();
                    var root = level.EnemiesRoot != null ? level.EnemiesRoot : level.transform;
                    var prefab = regular.Prefab != null
                        ? regular.Prefab
                        : refs.OrcPrefab;
                    return new EnemyPool(ctx.Container, prefab, root);
                })
                .AsSingle();

            Container.BindInterfacesAndSelfTo<GameBootstrap>().FromComponentInHierarchy().AsSingle();
            Container.BindInterfacesAndSelfTo<StateMachineInitializer>().AsSingle();
            Container.BindInterfacesAndSelfTo<GameEntryPoint>().AsSingle();

            Container.BindInitializableExecutionOrder<StateMachineInitializer>(-100);
            Container.BindInitializableExecutionOrder<CombatThreatMonitor>(-50);
            Container.BindInitializableExecutionOrder<GameEntryPoint>(0);
        }

        RegularEnemyConfig ResolveRegularEnemyConfig()
        {
            if (_regularEnemyConfig != null)
                return _regularEnemyConfig;

            var loaded = Resources.Load<RegularEnemyConfig>("RegularEnemyConfig");
            if (loaded != null)
                return loaded;

            Debug.LogError("GameInstaller: RegularEnemyConfig is missing. Run Magic Archer → Ensure Config Assets.");
            return ScriptableObject.CreateInstance<RegularEnemyConfig>();
        }

        BossEnemyConfig ResolveBossEnemyConfig()
        {
            if (_bossEnemyConfig != null)
                return _bossEnemyConfig;

            var loaded = Resources.Load<BossEnemyConfig>("BossEnemyConfig");
            if (loaded != null)
                return loaded;

            Debug.LogError("GameInstaller: BossEnemyConfig is missing. Run Magic Archer → Ensure Config Assets.");
            return ScriptableObject.CreateInstance<BossEnemyConfig>();
        }

        RegularUnitConfig ResolveRegularUnitConfig()
        {
            if (_regularUnitConfig != null)
                return _regularUnitConfig;

            var loaded = Resources.Load<RegularUnitConfig>("RegularUnitConfig");
            if (loaded != null)
                return loaded;

            Debug.LogError("GameInstaller: RegularUnitConfig is missing. Run Magic Archer → Ensure Config Assets.");
            return ScriptableObject.CreateInstance<RegularUnitConfig>();
        }

        UpgradedUnitConfig ResolveUpgradedUnitConfig()
        {
            if (_upgradedUnitConfig != null)
                return _upgradedUnitConfig;

            var loaded = Resources.Load<UpgradedUnitConfig>("UpgradedUnitConfig");
            if (loaded != null)
                return loaded;

            Debug.LogError("GameInstaller: UpgradedUnitConfig is missing. Run Magic Archer → Ensure Config Assets.");
            return ScriptableObject.CreateInstance<UpgradedUnitConfig>();
        }
    }
}
