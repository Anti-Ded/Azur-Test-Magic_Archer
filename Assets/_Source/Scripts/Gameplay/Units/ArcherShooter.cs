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
        RegularUnitConfig _regular;
        UpgradedUnitConfig _upgraded;
        IAudioService _audio;
        GamePhaseService _phases;
        UnitTier _tier = UnitTier.Normal;
        float _cooldownTimer;
        ICombatTarget _reservedTarget;

        static readonly int AttackHash = Animator.StringToHash("Attack");

        [Zenject.Inject]
        void Construct(
            CombatTargetRegistry registry,
            ProjectilePool projectilePool,
            RegularUnitConfig regular,
            UpgradedUnitConfig upgraded,
            GamePhaseService phases,
            [Zenject.Inject(Optional = true)] IAudioService audio = null)
        {
            _registry = registry;
            _projectilePool = projectilePool;
            _regular = regular;
            _upgraded = upgraded;
            _phases = phases;
            _audio = audio;
        }

        public void SetTier(UnitTier tier)
        {
            _tier = tier;
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

        UnitConfigBase GetActiveConfig()
        {
            return _tier == UnitTier.Upgraded
                ? _upgraded != null ? _upgraded : _regular
                : _regular;
        }

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

            return _tier == UnitTier.Upgraded
                ? GameConstants.UpgradedArcherCooldown
                : GameConstants.NormalArcherCooldown;
        }

        float GetDamage()
        {
            var config = GetActiveConfig();
            return config != null ? config.Damage : _tier == UnitTier.Upgraded ? 100f : 50f;
        }

        void Fire(ICombatTarget target)
        {
            _projectilePool.Launch(GetMuzzlePosition(), target, GetDamage());
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
