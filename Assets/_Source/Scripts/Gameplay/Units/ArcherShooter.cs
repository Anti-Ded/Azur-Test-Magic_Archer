using MagicArcher.Core.Audio;
using MagicArcher.Core;
using MagicArcher.Core.Config;
using MagicArcher.Gameplay.Combat;
using MagicArcher.Gameplay.Flow;
using MagicArcher.StateMachine;
using UnityEngine;

namespace MagicArcher.Gameplay.Units
{
    public sealed class ArcherShooter : MonoBehaviour
    {
        [SerializeField] Transform _muzzle;
        [SerializeField] Animator _animator;

        CombatTargetRegistry _registry;
        ProjectilePool _projectilePool;
        IAudioService _audio;
        GamePhaseService _phases;
        UnitConfigBase _config;
        float _cooldownTimer;
        ICombatTarget _reservedTarget;

        static readonly int AttackHash = Animator.StringToHash("Attack");

        [Zenject.Inject]
        void Construct(
            CombatTargetRegistry registry,
            ProjectilePool projectilePool,
            GamePhaseService phases,
            [Zenject.Inject(Optional = true)] IAudioService audio = null)
        {
            _registry = registry;
            _projectilePool = projectilePool;
            _phases = phases;
            _audio = audio;
        }

        public void ApplyConfig(UnitConfigBase config)
        {
            _config = config;
        }

        void OnDisable()
        {
            ReleaseReservation();
        }

        void Update()
        {
            if (ShouldPauseCombat())
                return;

            _cooldownTimer -= Time.deltaTime;
            if (_cooldownTimer > 0f)
                return;

            var config = GetActiveConfig();
            var attackRange = config != null ? config.AttackRange : GameConstants.ArcherAttackRange;
            var target = _registry.GetTargetForArcher(this, GetMuzzlePosition(), attackRange);
            if (target == null)
            {
                ReleaseReservation();
                return;
            }

            UpdateReservation(target);
            Fire(target);
            _cooldownTimer = GetCooldown();
        }

        UnitConfigBase GetActiveConfig() => _config;

        void UpdateReservation(ICombatTarget target)
        {
            if (target.AllowsMultipleAttackers)
            {
                ReleaseReservation();
                return;
            }

            if (_reservedTarget == target)
                return;

            ReleaseReservation();
            _registry.ReserveTarget(target, this);
            _reservedTarget = target;
        }

        void ReleaseReservation()
        {
            if (_reservedTarget == null)
                return;

            _registry.ReleaseTarget(_reservedTarget, this);
            _reservedTarget = null;
        }

        Vector3 GetMuzzlePosition()
        {
            return _muzzle != null ? _muzzle.position : transform.position + Vector3.up * 1.2f;
        }

        float GetCooldown()
        {
            var config = GetActiveConfig();
            if (config != null)
                return config.AttackCooldown;

            return GameConstants.NormalArcherCooldown;
        }

        float GetDamage()
        {
            var config = GetActiveConfig();
            return config != null ? config.Damage : 50f;
        }

        void Fire(ICombatTarget target)
        {
            var config = GetActiveConfig();
            var projectilePrefab = config != null ? config.ProjectilePrefab : null;
            if (projectilePrefab == null)
                return;

            _projectilePool.Launch(projectilePrefab, GetMuzzlePosition(), target, GetDamage());
            _audio?.PlayBowRelease();

            if (_animator != null)
                _animator.SetTrigger(AttackHash);

            var look = target.AimPoint.position - transform.position;
            look.y = 0f;
            if (look.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(look);
        }

        bool ShouldPauseCombat()
        {
            if (_phases == null)
                return false;

            if (_phases.IsTutorialCombatPaused)
                return true;

            var phase = _phases.CurrentPhase;
            return phase is GamePhase.Victory or GamePhase.Defeat or GamePhase.Cta;
        }
    }
}
