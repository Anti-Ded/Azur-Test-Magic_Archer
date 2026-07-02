using System.Collections;
using System.Collections.Generic;
using MagicArcher.Core.Config;
using MagicArcher.Gameplay.Flow;
using MagicArcher.Gameplay.Grid;
using MagicArcher.Gameplay.Units;
using UnityEngine;

namespace MagicArcher.Gameplay.Enemies
{
    public sealed class EnemyMeleeContact : MonoBehaviour
    {
        static readonly int AttackHash = Animator.StringToHash("Attack");

        readonly HashSet<UnitView> _targetsInRange = new();

        EnemyView _owner;
        EnemyMotor _motor;
        EnemyPathView _path;
        RegularEnemyConfig _regular;
        BossEnemyConfig _boss;
        IGridService _grid;
        CombatThreatMonitor _threat;
        GamePhaseService _phases;
        Animator _animator;

        bool _isBoss;
        bool _isAttacking;
        Coroutine _attackRoutine;

        public void Initialize(
            EnemyView owner,
            EnemyMotor motor,
            RegularEnemyConfig regular,
            BossEnemyConfig boss,
            IGridService grid,
            CombatThreatMonitor threat,
            GamePhaseService phases)
        {
            _owner = owner;
            _motor = motor;
            _regular = regular;
            _boss = boss;
            _grid = grid;
            _threat = threat;
            _phases = phases;
            _animator = owner != null ? owner.GetComponentInChildren<Animator>() : null;
        }

        public void Configure(EnemyPathView path, bool isBoss, float triggerRadius)
        {
            _path = path;
            _isBoss = isBoss;
            _isAttacking = false;
            _targetsInRange.Clear();

            if (_attackRoutine != null)
            {
                StopCoroutine(_attackRoutine);
                _attackRoutine = null;
            }

            EnsureTriggerPhysics(triggerRadius);
        }

        void OnDisable()
        {
            _targetsInRange.Clear();
            _isAttacking = false;

            if (_attackRoutine != null)
            {
                StopCoroutine(_attackRoutine);
                _attackRoutine = null;
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (!TryGetUnit(other, out var unit))
                return;

            _targetsInRange.Add(unit);
            TryStartAttack();
        }

        void OnTriggerStay(Collider other)
        {
            if (_isAttacking || IsCombatPaused())
                return;

            if (!TryGetUnit(other, out var unit))
                return;

            _targetsInRange.Add(unit);
            TryStartAttack();
        }

        void OnTriggerExit(Collider other)
        {
            if (!TryGetUnit(other, out var unit))
                return;

            _targetsInRange.Remove(unit);
        }

        void TryStartAttack()
        {
            if (_isAttacking || IsCombatPaused() || _owner == null || !_owner.IsAlive)
                return;

            if (!TryPickTarget(out var target))
                return;

            _attackRoutine = StartCoroutine(AttackRoutine(target));
        }

        bool TryPickTarget(out UnitView target)
        {
            target = null;

            foreach (var unit in _targetsInRange)
            {
                if (unit == null || unit.Health == null || !unit.Health.IsAlive)
                    continue;

                target = unit;
                return true;
            }

            return false;
        }

        IEnumerator AttackRoutine(UnitView target)
        {
            _isAttacking = true;
            _motor?.Stop();

            if (_animator != null)
                _animator.SetTrigger(AttackHash);

            var duration = ResolveAttackDuration();
            var hitDelay = duration * ResolveAttackHitNormalizedTime();
            if (hitDelay > 0f)
                yield return new WaitForSeconds(hitDelay);

            if (_owner != null && _owner.IsAlive && target != null && target.Health != null && target.Health.IsAlive)
                KillUnit(target);

            var remaining = duration - hitDelay;
            if (remaining > 0f)
                yield return new WaitForSeconds(remaining);

            _isAttacking = false;
            _attackRoutine = null;

            if (_owner != null && _owner.IsAlive && _motor != null && !_motor.IsMoving)
                _motor.Resume();

            TryStartAttack();
        }

        void KillUnit(UnitView unit)
        {
            if (unit.Shooter != null)
                unit.Shooter.enabled = false;

            unit.Health.TakeDamage(unit.Health.MaxHealth);

            if (unit.GridX >= 0 && unit.GridY >= 0)
                _grid?.TryRemove(unit.GridX, unit.GridY, out _);

            _targetsInRange.Remove(unit);
            Destroy(unit.gameObject);
            _threat?.TryHandleDefeat();
        }

        float ResolveAttackDuration()
        {
            if (_isBoss && _boss != null)
                return _boss.AttackDuration;

            return _regular != null ? _regular.AttackDuration : 2.125f;
        }

        float ResolveAttackHitNormalizedTime()
        {
            if (_isBoss && _boss != null)
                return _boss.AttackHitNormalizedTime;

            return _regular != null ? _regular.AttackHitNormalizedTime : 0.5f;
        }

        bool IsCombatPaused()
        {
            return _phases != null && _phases.IsTutorialCombatPaused;
        }

        static bool TryGetUnit(Collider other, out UnitView unit)
        {
            unit = other != null ? other.GetComponentInParent<UnitView>() : null;
            return unit != null;
        }

        void EnsureTriggerPhysics(float radius)
        {
            var body = GetComponent<Rigidbody>();
            if (body == null)
            {
                body = gameObject.AddComponent<Rigidbody>();
                body.isKinematic = true;
                body.useGravity = false;
            }

            if (!TryGetComponent(out SphereCollider trigger))
            {
                trigger = gameObject.AddComponent<SphereCollider>();
                trigger.isTrigger = true;
            }

            trigger.isTrigger = true;
            trigger.radius = Mathf.Max(0.1f, radius);
            trigger.center = new Vector3(0f, 1f, 0f);
        }
    }
}
