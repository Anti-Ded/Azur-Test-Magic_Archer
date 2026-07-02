using System;
using MagicArcher.Core.Audio;
using MagicArcher.Core.Config;
using MagicArcher.Gameplay.Economy;
using MagicArcher.Gameplay.Grid;
using MagicArcher.Gameplay.Level;
using MagicArcher.Gameplay.Units;
using UnityEngine;
using Zenject;

namespace MagicArcher.Gameplay.Units
{
    public sealed class UnitPurchaseService
    {
        readonly DiContainer _container;
        readonly LevelRoot _level;
        readonly IGridService _grid;
        readonly RegularUnitConfig _regularUnit;
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
            RegularUnitConfig regularUnit,
            IEconomyService economy,
            [Inject(Optional = true)] IAudioService audio = null)
        {
            _container = container;
            _level = level;
            _grid = grid;
            _regularUnit = regularUnit;
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
            var archerPrefab = _regularUnit != null ? _regularUnit.UnitViewPrefab : null;
            if (archerPrefab == null || _level.UnitsRoot == null)
                return false;

            if (!_grid.TryGetEmptySlot(out var slot))
                return false;

            if (!_economy.TrySpend(_currentCost))
                return false;

            var archer = _container.InstantiatePrefabForComponent<UnitView>(
                archerPrefab,
                _level.UnitsRoot);

            if (!_grid.TryPlace(slot.X, slot.Y, archer))
            {
                _economy.AddCoins(_currentCost);
                UnityEngine.Object.Destroy(archer.gameObject);
                return false;
            }

            archer.ApplyConfig(_regularUnit);
            _audio?.PlayUnitBuy();
            UnitPurchased?.Invoke(archer);
            return true;
        }
    }
}
