using System;

using System.Collections.Generic;

using MagicArcher.Core.Config;

using MagicArcher.Gameplay.Enemies;

using MagicArcher.Gameplay.Level;

using UnityEngine;

using Zenject;



namespace MagicArcher.Gameplay.Combat

{

    public sealed class EnemyWaveController : MonoBehaviour

    {

        readonly List<EnemyView> _queue = new();



        DiContainer _container;

        LevelRoot _level;

        EnemyPathView _path;

        EnemyPool _pool;

        RegularEnemyConfig _regular;

        BossEnemyConfig _boss;

        int _nextActivateIndex;



        public event Action<EnemyView> OrcDied;

        public event Action<EnemyView> EnemyReachedGrid;



        [Inject]

        void Construct(
            DiContainer container,
            LevelRoot level,
            EnemyPool pool,
            RegularEnemyConfig regular,
            BossEnemyConfig boss)

        {

            _container = container;

            _level = level;

            _pool = pool;

            _regular = regular;

            _boss = boss;

        }



        public void ResetWave(EnemyPathView path)

        {

            _path = path;

            _queue.Clear();

            _nextActivateIndex = 0;

        }



        public void PrewarmRegularLine(EnemyView prefab, int count)

        {

            if (prefab == null || _path == null || count <= 0)

                return;



            _pool?.Warmup(count + 1);



            var spacing = _regular != null ? _regular.SpawnSpacing : 1.5f;

            for (var i = 0; i < count; i++)

            {

                var position = EnemySpawnLayout.GetPrewarmPosition(_path, i, spacing);

                SpawnQueuedOrc(prefab, position, true);

            }

        }



        public EnemyView SpawnQueuedOrc(EnemyView prefab, Vector3 position, bool startMoving)

        {

            var maxHealth = _regular != null ? _regular.MaxHealth : 100f;

            return SpawnEnemy(

                prefab,

                position,

                Quaternion.identity,

                startMoving,

                maxHealth,

                false);

        }



        public EnemyView SpawnBoss(EnemyView fallbackPrefab, Vector3 position, bool startMoving)

        {

            var prefab = _boss != null && _boss.Prefab != null ? _boss.Prefab : fallbackPrefab;

            var maxHealth = _boss != null ? _boss.MaxHealth : 450f;



            var boss = SpawnEnemy(

                prefab,

                position,

                Quaternion.identity,

                startMoving,

                maxHealth,

                true);



            if (boss == null)

                return null;



            var scale = _boss != null ? _boss.ScaleMultiplier : 1.65f;

            var tint = _boss != null ? _boss.Tint : new Color(0.62f, 0.18f, 0.95f);

            boss.transform.localScale *= scale;

            ApplyBossTint(boss, tint);

            return boss;

        }



        EnemyView SpawnEnemy(

            EnemyView prefab,

            Vector3 position,

            Quaternion rotation,

            bool startMoving,

            float maxHealth,

            bool isBoss)

        {

            if (prefab == null || _level.EnemiesRoot == null)

                return null;



            var enemy = _pool != null

                ? _pool.Rent(position, rotation, maxHealth, isBoss, _path, startMoving)

                : _container.InstantiatePrefabForComponent<EnemyView>(

                    prefab,

                    position,

                    rotation,

                    _level.EnemiesRoot);



            if (enemy == null)

                return null;



            if (_pool == null)

                enemy.Configure(maxHealth, isBoss, _path, startMoving);



            enemy.Died -= OnOrcDied;

            enemy.Died += OnOrcDied;

            enemy.ReachedGrid -= OnEnemyReachedGrid;

            enemy.ReachedGrid += OnEnemyReachedGrid;

            _queue.Add(enemy);



            if (startMoving)

            {

                var addedIndex = _queue.Count - 1;

                if (addedIndex >= _nextActivateIndex)

                    _nextActivateIndex = addedIndex + 1;

            }



            return enemy;

        }



        public void ActivateNextWalker()

        {

            if (_path == null)

                return;



            while (_nextActivateIndex < _queue.Count)

            {

                var orc = _queue[_nextActivateIndex++];

                if (orc == null || !orc.IsAlive)

                    continue;



                if (orc.Motor != null && !orc.Motor.IsMoving)

                    orc.Motor.Begin(_path);



                return;

            }

        }



        public bool HasAliveRegularEnemies()

        {

            for (var i = 0; i < _queue.Count; i++)

            {

                var enemy = _queue[i];

                if (enemy == null || !enemy.IsAlive || enemy.Health == null)

                    continue;



                if (!enemy.Health.IsBoss)

                    return true;

            }



            return false;

        }



        void OnOrcDied(EnemyView orc)

        {

            orc.Died -= OnOrcDied;

            orc.ReachedGrid -= OnEnemyReachedGrid;

            OrcDied?.Invoke(orc);

        }



        void OnEnemyReachedGrid(EnemyView enemy)

        {

            EnemyReachedGrid?.Invoke(enemy);

        }



        public void ClearSubscriptions()

        {

            OrcDied = null;

            EnemyReachedGrid = null;

        }



        static void ApplyBossTint(EnemyView boss, Color tint)

        {

            foreach (var renderer in boss.GetComponentsInChildren<Renderer>())

            {

                if (renderer == null)

                    continue;



                var block = new MaterialPropertyBlock();

                renderer.GetPropertyBlock(block);

                block.SetColor("_Color", tint);

                block.SetColor("_BaseColor", tint);

                renderer.SetPropertyBlock(block);

            }

        }

    }

}
