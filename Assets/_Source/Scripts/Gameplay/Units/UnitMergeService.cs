using System;
using MagicArcher.Core.Audio;
using MagicArcher.Gameplay.Combat;
using MagicArcher.Gameplay.Grid;
using MagicArcher.Gameplay.Level;
using UnityEngine;
using Zenject;

namespace MagicArcher.Gameplay.Units
{
    public sealed class UnitMergeService
    {
        readonly DiContainer _container;
        readonly LevelRoot _level;
        readonly IGridService _grid;
        readonly CombatSceneRefs _refs;
        readonly MergeVfx _mergeVfx;
        readonly IAudioService _audio;

        public event Action<UnitView> UnitMerged;

        [Inject]
        public UnitMergeService(
            DiContainer container,
            LevelRoot level,
            IGridService grid,
            CombatSceneRefs refs,
            MergeVfx mergeVfx,
            [Inject(Optional = true)] IAudioService audio = null)
        {
            _container = container;
            _level = level;
            _grid = grid;
            _refs = refs;
            _mergeVfx = mergeVfx;
            _audio = audio;
        }

        public bool CanMerge(UnitView source, UnitView target)
        {
            if (source == null || target == null || source == target)
                return false;

            return !source.IsUpgraded && !target.IsUpgraded;
        }

        public bool TryMerge(UnitView source, int targetX, int targetY)
        {
            if (source == null || !_grid.TryGetUnit(targetX, targetY, out var target))
                return false;

            if (!CanMerge(source, target))
                return false;

            var mergeX = targetX;
            var mergeY = targetY;
            var sourceX = source.GridX;
            var sourceY = source.GridY;

            _grid.TryRemove(sourceX, sourceY, out _);
            _grid.TryRemove(targetX, targetY, out _);
            UnityEngine.Object.Destroy(source.gameObject);
            UnityEngine.Object.Destroy(target.gameObject);

            var upgraded = _container.InstantiatePrefabForComponent<UnitView>(
                _refs.ArcherPrefab,
                _level.UnitsRoot);

            upgraded.SetUpgraded(true);
            _grid.TryPlace(mergeX, mergeY, upgraded);
            _mergeVfx?.Play(upgraded.transform);
            _audio?.PlayMerge();
            UnitMerged?.Invoke(upgraded);
            return true;
        }
    }
}
