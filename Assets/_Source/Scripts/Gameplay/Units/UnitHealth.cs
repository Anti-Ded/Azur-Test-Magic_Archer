using System;
using MagicArcher.Gameplay.Combat;
using MagicArcher.UI;
using UnityEngine;

namespace MagicArcher.Gameplay.Units
{
    public sealed class UnitHealth : MonoBehaviour, IDamageable, IHealthBarProvider
    {
        float _maxHealth = 150f;
        float _currentHealth;

        public bool IsAlive => _currentHealth > 0f;
        public float MaxHealth => _maxHealth;
        public float CurrentHealth => _currentHealth;
        public float NormalizedHealth => _maxHealth <= 0f ? 0f : _currentHealth / _maxHealth;

        public event Action<IDamageable> Died;
        public event Action HealthChanged;

        public void Configure(float maxHealth)
        {
            _maxHealth = Mathf.Max(1f, maxHealth);
            _currentHealth = _maxHealth;
            HealthChanged?.Invoke();
        }

        public void TakeDamage(float damage)
        {
            if (!IsAlive || damage <= 0f)
                return;

            _currentHealth = Mathf.Max(0f, _currentHealth - damage);
            HealthChanged?.Invoke();

            if (IsAlive)
                return;

            Died?.Invoke(this);
        }
    }
}
