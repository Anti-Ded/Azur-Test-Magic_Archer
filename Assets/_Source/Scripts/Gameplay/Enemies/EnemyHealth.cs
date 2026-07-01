using System;
using MagicArcher.Core.Audio;
using MagicArcher.Gameplay.Combat;
using MagicArcher.UI;
using UnityEngine;

namespace MagicArcher.Gameplay.Enemies
{
    public sealed class EnemyHealth : MonoBehaviour, IDamageable, IHealthBarProvider
    {
        [SerializeField] Transform _aimPoint;

        float _maxHealth = 100f;
        float _currentHealth;
        bool _isBoss;
        IAudioService _audio;

        public bool IsAlive => _currentHealth > 0f;
        public bool IsBoss => _isBoss;
        public float MaxHealth => _maxHealth;
        public float CurrentHealth => _currentHealth;
        public float NormalizedHealth => _maxHealth <= 0f ? 0f : _currentHealth / _maxHealth;
        public event Action<IDamageable> Died;
        public event Action HealthChanged;

        [Zenject.Inject]
        void Construct([Zenject.Inject(Optional = true)] IAudioService audio = null)
        {
            _audio = audio;
        }

        public void Configure(float maxHealth, bool isBoss)
        {
            _maxHealth = Mathf.Max(1f, maxHealth);
            _currentHealth = _maxHealth;
            _isBoss = isBoss;
            RefreshBar();
        }

        public void TakeDamage(float damage)
        {
            if (!IsAlive || damage <= 0f)
                return;

            _currentHealth = Mathf.Max(0f, _currentHealth - damage);
            RefreshBar();
            _audio?.PlayOrcHit();

            if (IsAlive)
                return;

            Died?.Invoke(this);
        }

        void RefreshBar()
        {
            HealthChanged?.Invoke();
        }

        public Transform GetAimPoint()
        {
            return _aimPoint != null ? _aimPoint : transform;
        }
    }
}
