using System;
using MagicArcher.Core.Audio;
using MagicArcher.Gameplay.Combat;
using MagicArcher.Gameplay.Economy;
using MagicArcher.Gameplay.Grid;
using MagicArcher.Gameplay.Level;
using UnityEngine;
using Zenject;

namespace MagicArcher.Gameplay.Units
{
    public sealed class UnitPurchaseService
    {
        readonly DiContainer _container;
        readonly LevelRoot _level;
        readonly IGridService _grid;
        readonly CombatSceneRefs _refs;
        readonly IEconomyService _economy;
        readonly IAudioService _audio;

        int _currentCost;

        public int CurrentCost => _currentCost;

        public event Action<UnitView> UnitPurchased;

        [Inject]
        public UnitPurchaseService(
            DiContainer container,
            LevelRoot level,
            IGridService grid,
            CombatSceneRefs refs,
            IEconomyService economy,
            [Inject(Optional = true)] IAudioService audio = null)
        {
            _container = container;
            _level = level;
            _grid = grid;
            _refs = refs;
            _economy = economy;
            _audio = audio;
        }

        public void SetCost(int cost)
        {
            _currentCost = Mathf.Max(0, cost);
        }

        public bool CanAfford()
        {
            return _economy.Coins >= _currentCost;
        }

        public bool TryPurchase()
        {
            if (_refs.ArcherPrefab == null || _level.UnitsRoot == null)
                return false;

            if (!_grid.TryGetEmptySlot(out var slot))
                return false;

            if (!_economy.TrySpend(_currentCost))
                return false;

            var archer = _container.InstantiatePrefabForComponent<UnitView>(
                _refs.ArcherPrefab,
                _level.UnitsRoot);

            if (!_grid.TryPlace(slot.X, slot.Y, archer))
            {
                _economy.AddCoins(_currentCost);
                UnityEngine.Object.Destroy(archer.gameObject);
                return false;
            }

            archer.SetUpgraded(false);
            _audio?.PlayUnitBuy();
            UnitPurchased?.Invoke(archer);
            return true;
        }
    }
}
