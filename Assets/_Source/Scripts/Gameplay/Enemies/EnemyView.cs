using System;
using System.Collections;
using MagicArcher.Core.Audio;
using MagicArcher.Core.Config;
using MagicArcher.Gameplay.Combat;
using MagicArcher.Gameplay.Flow;
using MagicArcher.Gameplay.Grid;
using MagicArcher.UI;
using UnityEngine;
using Zenject;

namespace MagicArcher.Gameplay.Enemies
{
    public sealed class EnemyView : MonoBehaviour, ICombatTarget
    {
        static readonly int DeathHash = Animator.StringToHash("Death");

        [SerializeField] EnemyHealth _health;
        [SerializeField] EnemyMotor _motor;

        CombatTargetRegistry _registry;
        IAudioService _audio;
        RegularEnemyConfig _regular;
        BossEnemyConfig _boss;
        IGridService _grid;
        CombatThreatMonitor _threat;
        GamePhaseService _phases;
        Animator _animator;
        EnemyPool _pool;
        EnemyMeleeContact _melee;

        public Transform AimPoint => _health != null ? _health.GetAimPoint() : transform;
        public bool IsAlive => _health != null && _health.IsAlive;
        public bool AllowsMultipleAttackers => _health != null && _health.IsBoss;
        public IDamageable Damageable => _health;
        public EnemyHealth Health => _health;
        public EnemyMotor Motor => _motor;

        public event Action<EnemyView> Died;
        public event Action<EnemyView> ReachedGrid;

        [Inject]
        void Construct(
            CombatTargetRegistry registry,
            RegularEnemyConfig regular,
            BossEnemyConfig boss,
            IGridService grid,
            CombatThreatMonitor threat,
            GamePhaseService phases,
            [Inject(Optional = true)] EnemyPool pool = null,
            [Inject(Optional = true)] IAudioService audio = null)
        {
            _registry = registry;
            _regular = regular;
            _boss = boss;
            _grid = grid;
            _threat = threat;
            _phases = phases;
            _pool = pool;
            _audio = audio;

            EnsureMeleeComponent();
            _melee.Initialize(this, _motor, _regular, _boss, _grid, _threat, _phases);
        }

        void Awake()
        {
            _animator = GetComponentInChildren<Animator>();
            EnsureMeleeComponent();
        }

        void EnsureMeleeComponent()
        {
            if (_melee != null)
                return;

            _melee = GetComponent<EnemyMeleeContact>();
            if (_melee == null)
                _melee = gameObject.AddComponent<EnemyMeleeContact>();
        }

        void OnEnable()
        {
            if (_health != null)
                _health.Died += OnHealthDied;

            if (_motor != null)
                _motor.ReachedGrid += OnMotorReachedGrid;

            _registry?.Register(this);
        }

        void OnDisable()
        {
            if (_health != null)
                _health.Died -= OnHealthDied;

            if (_motor != null)
                _motor.ReachedGrid -= OnMotorReachedGrid;

            _registry?.Unregister(this);
        }

        public void PrepareForReuse(Vector3 position, Quaternion rotation)
        {
            transform.SetPositionAndRotation(position, rotation);
            transform.localScale = Vector3.one;
            ResetVisualTint();
            gameObject.SetActive(true);
        }

        public void Configure(float maxHealth, bool isBoss, EnemyPathView path, bool startMoving)
        {
            _health.Configure(maxHealth, isBoss);

            var presenter = GetComponent<HealthBarPresenter>();
            if (presenter != null)
            {
                if (isBoss && _boss != null)
                    presenter.SetProfile(_boss.CreateHealthBarProfile());
                else if (!isBoss && _regular != null)
                    presenter.SetProfile(_regular.CreateHealthBarProfile());
            }

            if (_motor != null)
            {
                var moveSpeed = isBoss
                    ? _boss != null ? _boss.MoveSpeed : 1.6f
                    : _regular != null ? _regular.MoveSpeed : 1.6f;
                _motor.ConfigureMoveSpeed(moveSpeed);
            }

            if (startMoving && _motor != null && path != null)
                _motor.Begin(path);
            else
                _motor?.Stop();

            var triggerRadius = isBoss
                ? _boss != null ? _boss.AttackTriggerRadius : 2.4f
                : _regular != null ? _regular.AttackTriggerRadius : 1.8f;
            _melee?.Configure(path, isBoss, triggerRadius);
        }

        void OnHealthDied(IDamageable _)
        {
            _motor?.Stop();
            _audio?.PlayOrcDeath();
            Died?.Invoke(this);
            StartCoroutine(PlayDeathAndReturn());
        }

        IEnumerator PlayDeathAndReturn()
        {
            if (_animator != null)
                _animator.SetTrigger(DeathHash);

            var deathDelay = _health.IsBoss
                ? _boss != null ? _boss.DeathReturnDelay : 2.83f
                : _regular != null ? _regular.DeathReturnDelay : 2.83f;
            yield return new WaitForSeconds(deathDelay);

            if (_pool != null)
                _pool.Return(this);
            else
                gameObject.SetActive(false);
        }

        void OnMotorReachedGrid()
        {
            ReachedGrid?.Invoke(this);
        }

        void ResetVisualTint()
        {
            foreach (var renderer in GetComponentsInChildren<Renderer>())
            {
                if (renderer == null)
                    continue;

                renderer.SetPropertyBlock(null);
            }
        }
    }
}
