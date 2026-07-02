using System.Collections.Generic;
using MagicArcher.Gameplay.Flow;
using UnityEngine;

namespace MagicArcher.Gameplay.Combat
{
    public sealed class ProjectilePool
    {
        readonly Transform _root;
        readonly GamePhaseService _phases;
        readonly Dictionary<ProjectileView, List<ProjectileView>> _pools = new();

        public ProjectilePool(Transform root, GamePhaseService phases)
        {
            _root = root;
            _phases = phases;
        }

        public void Warmup(ProjectileView prefab, int count)
        {
            if (prefab == null || count <= 0)
                return;

            var pool = GetOrCreatePool(prefab);
            while (pool.Count < count)
                pool.Add(CreateInstance(prefab));
        }

        public void Launch(ProjectileView prefab, Vector3 from, ICombatTarget target, float damage)
        {
            if (prefab == null || target == null)
                return;

            Rent(prefab).Launch(from, target, damage);
        }

        ProjectileView Rent(ProjectileView prefab)
        {
            var pool = GetOrCreatePool(prefab);
            for (var i = 0; i < pool.Count; i++)
            {
                var item = pool[i];
                if (item != null && !item.gameObject.activeSelf)
                    return item;
            }

            var instance = CreateInstance(prefab);
            pool.Add(instance);
            return instance;
        }

        List<ProjectileView> GetOrCreatePool(ProjectileView prefab)
        {
            if (!_pools.TryGetValue(prefab, out var pool))
            {
                pool = new List<ProjectileView>();
                _pools[prefab] = pool;
            }

            return pool;
        }

        ProjectileView CreateInstance(ProjectileView prefab)
        {
            var instance = Object.Instantiate(prefab, _root);
            instance.Configure(_phases);
            instance.gameObject.SetActive(false);
            return instance;
        }
    }
}
