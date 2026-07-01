using MagicArcher.Core.Config;
using MagicArcher.Gameplay.Economy;
using MagicArcher.Gameplay.Enemies;
using MagicArcher.Gameplay.Units;
using UnityEngine;
using Zenject;

namespace MagicArcher.Gameplay.Combat
{
    public sealed class CombatSceneRefs : MonoBehaviour
    {
        [SerializeField] UnitView _archerPrefab;
        [SerializeField] EnemyView _orcPrefab;
        [SerializeField] ProjectileView _projectilePrefab;
        [SerializeField] Transform _projectilesRoot;
        [SerializeField] RectTransform _coinHudTarget;
        [SerializeField] CoinFlyVfx _coinFlyPrefab;
        [SerializeField] RectTransform _coinFlyPoolRoot;

        RegularUnitConfig _regularUnit;
        RegularEnemyConfig _regularEnemy;
        UnitView _resolvedArcher;
        EnemyView _resolvedOrc;

        [Inject]
        void Construct(
            RegularUnitConfig regularUnit,
            RegularEnemyConfig regularEnemy)
        {
            _regularUnit = regularUnit;
            _regularEnemy = regularEnemy;
        }

        public UnitView ArcherPrefab => ResolveArcherPrefab();
        public EnemyView OrcPrefab => ResolveOrcPrefab();
        public ProjectileView ProjectilePrefab => _projectilePrefab;
        public Transform ProjectilesRoot => _projectilesRoot;
        public RectTransform CoinHudTarget => _coinHudTarget;
        public CoinFlyVfx CoinFlyPrefab => _coinFlyPrefab;
        public RectTransform CoinFlyPoolRoot => _coinFlyPoolRoot;

        UnitView ResolveArcherPrefab()
        {
            if (_resolvedArcher != null)
                return _resolvedArcher;

            if (_archerPrefab != null)
                return _resolvedArcher = _archerPrefab;

            var prefab = _regularUnit != null ? _regularUnit.Prefab : null;
            if (prefab != null)
                return _resolvedArcher = prefab.GetComponent<UnitView>();

            return null;
        }

        EnemyView ResolveOrcPrefab()
        {
            if (_resolvedOrc != null)
                return _resolvedOrc;

            if (_orcPrefab != null)
                return _resolvedOrc = _orcPrefab;

            var prefab = _regularEnemy != null ? _regularEnemy.Prefab : null;
            if (prefab != null)
                return _resolvedOrc = prefab;

            return null;
        }
    }
}
