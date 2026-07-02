using System;
using System.Collections.Generic;
using UnityEngine;

namespace MagicArcher.Gameplay.Economy
{
    public sealed class CoinFlyVfxService
    {
        const int CoinsPerBurst = 10;
        const int InitialPoolCapacity = 20;
        const int PoolGrowChunk = 10;
        const float SpreadDuration = 0.2f;
        const float FlyDelay = 0.5f;
        const float FlyDuration = 0.5f;
        const float SpreadRadius = 108f;

        readonly CoinFlyVfx _prefab;
        readonly RectTransform _poolRoot;
        readonly RectTransform _coinHudTarget;
        readonly List<CoinFlyVfx> _pool = new();

        public CoinFlyVfxService(CoinFlyVfx prefab, RectTransform poolRoot, RectTransform coinHudTarget)
        {
            _prefab = prefab;
            _poolRoot = poolRoot;
            _coinHudTarget = coinHudTarget;

            if (_prefab != null)
                EnsurePoolCapacity(InitialPoolCapacity);
        }

        public void Play(Vector3 worldPosition, Action onRewardGranted)
        {
            if (_prefab == null || _poolRoot == null || _coinHudTarget == null)
            {
                onRewardGranted?.Invoke();
                return;
            }

            if (!TryGetLocalPositions(worldPosition, out var startLocal, out var targetLocal))
            {
                onRewardGranted?.Invoke();
                return;
            }

            EnsurePoolCapacity(CoinsPerBurst);
            var completed = 0;

            for (var i = 0; i < CoinsPerBurst; i++)
            {
                var coin = Rent();
                var spreadOffset = GetSpreadOffset(i);
                coin.Play(
                    startLocal,
                    spreadOffset,
                    targetLocal,
                    SpreadDuration,
                    FlyDelay,
                    FlyDuration,
                    () =>
                    {
                        completed++;
                        if (completed >= CoinsPerBurst)
                            onRewardGranted?.Invoke();
                    });
            }
        }

        Vector2 GetSpreadOffset(int index)
        {
            var angle = Mathf.Lerp(-55f, 55f, index / (float)(CoinsPerBurst - 1));
            var radians = angle * Mathf.Deg2Rad;
            var radius = SpreadRadius * UnityEngine.Random.Range(0.55f, 1f);
            return new Vector2(Mathf.Sin(radians) * radius, Mathf.Cos(radians) * radius * 0.35f);
        }

        bool TryGetLocalPositions(Vector3 worldPosition, out Vector2 startLocal, out Vector2 targetLocal)
        {
            startLocal = default;
            targetLocal = default;

            var spaceRoot = GetSpaceRoot();
            if (spaceRoot == null || _coinHudTarget == null)
                return false;

            var canvas = spaceRoot.GetComponent<Canvas>() ?? spaceRoot.GetComponentInParent<Canvas>();
            var camera = Camera.main;
            if (camera == null)
                return false;

            var uiCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? canvas.worldCamera
                : null;

            var worldScreen = camera.WorldToScreenPoint(worldPosition);
            if (worldScreen.z <= 0f)
                return false;

            if (!TryScreenToLocal(spaceRoot, worldScreen, uiCamera, out startLocal))
                return false;

            var hudWorld = _coinHudTarget.TransformPoint(_coinHudTarget.rect.center);
            var hudScreen = RectTransformUtility.WorldToScreenPoint(uiCamera, hudWorld);
            if (!TryScreenToLocal(spaceRoot, hudScreen, uiCamera, out targetLocal))
            {
                var fallback = spaceRoot.InverseTransformPoint(hudWorld);
                targetLocal = new Vector2(fallback.x, fallback.y);
            }

            return true;
        }

        static bool TryScreenToLocal(RectTransform root, Vector3 screenPoint, Camera uiCamera, out Vector2 local)
        {
            var screen = new Vector2(screenPoint.x, screenPoint.y);
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(root, screen, uiCamera, out local))
                return true;

            var planeDistance = root.position.z;
            if (uiCamera != null)
            {
                var world = uiCamera.ScreenToWorldPoint(new Vector3(screen.x, screen.y, planeDistance));
                var inner = root.InverseTransformPoint(world);
                local = new Vector2(inner.x, inner.y);
                return true;
            }

            var inverse = root.InverseTransformPoint(screen);
            local = new Vector2(inverse.x, inverse.y);
            return true;
        }

        RectTransform GetSpaceRoot()
        {
            var canvas = _poolRoot.GetComponentInParent<Canvas>();
            if (canvas != null)
                return canvas.transform as RectTransform;

            return _poolRoot;
        }

        void EnsurePoolCapacity(int requiredAvailable)
        {
            var available = CountAvailable();
            var toCreate = Mathf.Max(0, requiredAvailable - available);
            if (toCreate == 0 && _pool.Count == 0)
                toCreate = InitialPoolCapacity;

            while (toCreate > 0)
            {
                var chunk = Mathf.Min(toCreate, PoolGrowChunk);
                for (var i = 0; i < chunk; i++)
                    CreateInstance();

                toCreate -= chunk;
            }
        }

        int CountAvailable()
        {
            var count = 0;
            for (var i = 0; i < _pool.Count; i++)
            {
                var coin = _pool[i];
                if (coin != null && coin.IsAvailable)
                    count++;
            }

            return count;
        }

        CoinFlyVfx Rent()
        {
            for (var i = 0; i < _pool.Count; i++)
            {
                var coin = _pool[i];
                if (coin != null && coin.IsAvailable)
                    return coin;
            }

            for (var i = 0; i < PoolGrowChunk; i++)
                CreateInstance();

            for (var i = 0; i < _pool.Count; i++)
            {
                var coin = _pool[i];
                if (coin != null && coin.IsAvailable)
                    return coin;
            }

            return CreateInstance();
        }

        CoinFlyVfx CreateInstance()
        {
            var parent = GetSpaceRoot() ?? _poolRoot;
            var instance = UnityEngine.Object.Instantiate(_prefab, parent);
            instance.transform.SetAsLastSibling();
            _pool.Add(instance);
            return instance;
        }
    }
}
