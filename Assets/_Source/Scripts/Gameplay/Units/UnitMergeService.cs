using System;
using MagicArcher.Core.Audio;
using MagicArcher.Core.Config;
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
        readonly UnitConfigCatalog _unitConfigs;
        readonly MergeVfx _mergeVfx;
        readonly IAudioService _audio;

        public event Action<UnitView> UnitMerged;

        [Inject]
        public UnitMergeService(
            DiContainer container,
            LevelRoot level,
            IGridService grid,
            UnitConfigCatalog unitConfigs,
            MergeVfx mergeVfx,
            [Inject(Optional = true)] IAudioService audio = null)
        {
            _container = container;
            _level = level;
            _grid = grid;
            _unitConfigs = unitConfigs;
            _mergeVfx = mergeVfx;
            _audio = audio;
        }

        public bool CanMerge(UnitView source, UnitView target)
        {
            if (source == null || target == null || source == target)
                return false;

            if (source.Config == null || target.Config == null || _unitConfigs == null)
                return false;

            return _unitConfigs.ResolveMergeResult(source.Config, target.Config) != null;
        }

        public bool TryMerge(UnitView source, int targetX, int targetY)
        {
            if (source == null || !_grid.TryGetUnit(targetX, targetY, out var target))
                return false;

            if (!CanMerge(source, target))
                return false;

            var resultConfig = _unitConfigs.ResolveMergeResult(source.Config, target.Config);
            var resultPrefab = resultConfig?.Prefab != null
                ? resultConfig.Prefab.GetComponent<UnitView>()
                : null;
            if (resultConfig == null || resultPrefab == null || _level.UnitsRoot == null)
                return false;

            var mergeX = targetX;
            var mergeY = targetY;
            var sourceX = source.GridX;
            var sourceY = source.GridY;

            _grid.TryRemove(sourceX, sourceY, out _);
            _grid.TryRemove(targetX, targetY, out _);
            UnityEngine.Object.Destroy(source.gameObject);
            UnityEngine.Object.Destroy(target.gameObject);

            var merged = _container.InstantiatePrefabForComponent<UnitView>(
                resultPrefab,
                _level.UnitsRoot);

            merged.ApplyConfig(resultConfig);
            _grid.TryPlace(mergeX, mergeY, merged);
            _mergeVfx?.Play(merged.transform);
            _audio?.PlayMerge();
            UnitMerged?.Invoke(merged);
            return true;
        }
    }
}
