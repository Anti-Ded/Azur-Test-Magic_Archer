using System;
using MagicArcher.UI;
using UnityEngine;

namespace MagicArcher.Gameplay.Units
{
    [RequireComponent(typeof(UnitHealth))]
    public sealed class UnitHealthBarProvider : MonoBehaviour, IHealthBarProvider
    {
        UnitHealth _health;

        public float NormalizedHealth => _health != null ? _health.NormalizedHealth : 1f;

        public event Action HealthChanged
        {
            add
            {
                if (_health != null)
                    _health.HealthChanged += value;
            }
            remove
            {
                if (_health != null)
                    _health.HealthChanged -= value;
            }
        }

        void Awake()
        {
            _health = GetComponent<UnitHealth>();
        }
    }
}
