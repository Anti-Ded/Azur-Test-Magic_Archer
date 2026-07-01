using System.Collections.Generic;
using MagicArcher.Gameplay.Enemies;
using UnityEngine;
using Zenject;

namespace MagicArcher.Gameplay.Combat
{
    public sealed class EnemyPool
    {
        readonly DiContainer _container;
        readonly EnemyView _prefab;
        readonly Transform _root;
        readonly List<EnemyView> _pool = new();

        public EnemyPool(DiContainer container, EnemyView prefab, Transform root)
        {
            _container = container;
            _prefab = prefab;
            _root = root;
        }

        public void Warmup(int count)
        {
            if (_prefab == null || _root == null)
                return;

            for (var i = _pool.Count; i < count; i++)
                CreateInstance();
        }

        public EnemyView Rent(
            Vector3 position,
            Quaternion rotation,
            float maxHealth,
            bool isBoss,
            EnemyPathView path,
            bool startMoving)
        {
            if (_prefab == null || _root == null)
                return null;

            var enemy = GetInactiveOrCreate();
            enemy.PrepareForReuse(position, rotation);
            enemy.Configure(maxHealth, isBoss, path, startMoving);
            return enemy;
        }

        public void Return(EnemyView enemy)
        {
            if (enemy == null)
                return;

            enemy.gameObject.SetActive(false);
        }

        EnemyView GetInactiveOrCreate()
        {
            for (var i = 0; i < _pool.Count; i++)
            {
                var item = _pool[i];
                if (item != null && !item.gameObject.activeSelf)
                    return item;
            }

            return CreateInstance();
        }

        EnemyView CreateInstance()
        {
            var instance = _container.InstantiatePrefabForComponent<EnemyView>(
                _prefab,
                _root);

            instance.gameObject.SetActive(false);
            _pool.Add(instance);
            return instance;
        }
    }
}
