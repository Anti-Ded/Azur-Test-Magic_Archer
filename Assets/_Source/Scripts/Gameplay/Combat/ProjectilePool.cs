using System.Collections.Generic;
using MagicArcher.Gameplay.Flow;
using UnityEngine;

namespace MagicArcher.Gameplay.Combat
{
    public sealed class ProjectilePool
    {
        readonly ProjectileView _prefab;
        readonly Transform _root;
        readonly GamePhaseService _phases;
        readonly List<ProjectileView> _pool = new();

        public ProjectilePool(ProjectileView prefab, Transform root, GamePhaseService phases)
        {
            _prefab = prefab;
            _root = root;
            _phases = phases;
        }

        public void Warmup(int count)
        {
            for (var i = _pool.Count; i < count; i++)
                CreateInstance();
        }

        public void Launch(Vector3 from, ICombatTarget target, float damage)
        {
            if (_prefab == null || target == null)
                return;

            var projectile = Rent();
            projectile.Launch(from, target, damage);
        }

        ProjectileView Rent()
        {
            for (var i = 0; i < _pool.Count; i++)
            {
                var item = _pool[i];
                if (!item.gameObject.activeSelf)
                    return item;
            }

            return CreateInstance();
        }

        ProjectileView CreateInstance()
        {
            var instance = Object.Instantiate(_prefab, _root);
            instance.Configure(_phases);
            instance.gameObject.SetActive(false);
            _pool.Add(instance);
            return instance;
        }
    }
}
