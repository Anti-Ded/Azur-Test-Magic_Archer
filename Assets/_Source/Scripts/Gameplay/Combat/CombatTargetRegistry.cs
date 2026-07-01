using System.Collections.Generic;
using MagicArcher.Gameplay.Units;
using UnityEngine;

namespace MagicArcher.Gameplay.Combat
{
    public sealed class CombatTargetRegistry
    {
        readonly List<ICombatTarget> _targets = new();
        readonly Dictionary<ICombatTarget, ArcherShooter> _exclusiveLocks = new();

        public void Register(ICombatTarget target)
        {
            if (target == null || _targets.Contains(target))
                return;

            _targets.Add(target);
        }

        public void Unregister(ICombatTarget target)
        {
            if (target == null)
                return;

            _targets.Remove(target);
            _exclusiveLocks.Remove(target);
        }

        public ICombatTarget GetTargetForArcher(ArcherShooter archer, Vector3 from, float maxRange)
        {
            if (archer == null)
                return null;

            CleanupLocks();

            var owned = GetOwnedTarget(archer, from, maxRange);
            if (owned != null)
                return owned;

            return FindNearestAvailable(archer, from, maxRange);
        }

        public void ReserveTarget(ICombatTarget target, ArcherShooter archer)
        {
            if (target == null || archer == null || target.AllowsMultipleAttackers)
                return;

            _exclusiveLocks[target] = archer;
        }

        public void ReleaseTarget(ICombatTarget target, ArcherShooter archer)
        {
            if (target == null || archer == null)
                return;

            if (_exclusiveLocks.TryGetValue(target, out var owner) && owner == archer)
                _exclusiveLocks.Remove(target);
        }

        public void ReleaseAllForArcher(ArcherShooter archer)
        {
            if (archer == null)
                return;

            var toRemove = new List<ICombatTarget>();
            foreach (var pair in _exclusiveLocks)
            {
                if (pair.Value == archer)
                    toRemove.Add(pair.Key);
            }

            for (var i = 0; i < toRemove.Count; i++)
                _exclusiveLocks.Remove(toRemove[i]);
        }

        ICombatTarget GetOwnedTarget(ArcherShooter archer, Vector3 from, float maxRange)
        {
            foreach (var pair in _exclusiveLocks)
            {
                if (pair.Value != archer)
                    continue;

                var target = pair.Key;
                if (!IsAlive(target) || !IsInRange(target, from, maxRange))
                {
                    _exclusiveLocks.Remove(target);
                    continue;
                }

                return target;
            }

            return null;
        }

        ICombatTarget FindNearestAvailable(ArcherShooter archer, Vector3 from, float maxRange)
        {
            ICombatTarget best = null;
            var bestSqr = maxRange * maxRange;

            for (var i = 0; i < _targets.Count; i++)
            {
                var target = _targets[i];
                if (!IsAlive(target) || !IsInRange(target, from, maxRange))
                    continue;

                if (!IsAvailableForArcher(target, archer))
                    continue;

                var aim = target.AimPoint;
                if (aim == null)
                    continue;

                var sqr = (aim.position - from).sqrMagnitude;
                if (sqr >= bestSqr)
                    continue;

                bestSqr = sqr;
                best = target;
            }

            return best;
        }

        bool IsAvailableForArcher(ICombatTarget target, ArcherShooter archer)
        {
            if (target.AllowsMultipleAttackers)
                return true;

            if (!_exclusiveLocks.TryGetValue(target, out var owner))
                return true;

            return owner == archer;
        }

        static bool IsAlive(ICombatTarget target)
        {
            return target != null && target.IsAlive;
        }

        static bool IsInRange(ICombatTarget target, Vector3 from, float maxRange)
        {
            var aim = target?.AimPoint;
            if (aim == null)
                return false;

            var maxSqr = maxRange * maxRange;
            return (aim.position - from).sqrMagnitude <= maxSqr;
        }

        void CleanupLocks()
        {
            var toRemove = new List<ICombatTarget>();
            foreach (var pair in _exclusiveLocks)
            {
                if (!IsAlive(pair.Key))
                    toRemove.Add(pair.Key);
            }

            for (var i = 0; i < toRemove.Count; i++)
                _exclusiveLocks.Remove(toRemove[i]);
        }
    }
}
