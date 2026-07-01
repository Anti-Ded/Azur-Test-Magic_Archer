using System;
using System.Collections.Generic;
using MagicArcher.Gameplay.Combat;
using UnityEngine;

namespace MagicArcher.Gameplay.Economy
{
    public sealed class CoinFlyVfxService
    {
        const int BurstCount = 10;
        const float SpreadDuration = 0.2f;
        const float FlyDelay = 0.5f;
        const float FlyDuration = 0.5f;
        const float SpreadRadius = 72f;

        readonly CoinFlyVfx _prefab;
        readonly RectTransform _poolRoot;
        readonly CombatSceneRefs _refs;
        readonly List<CoinFlyVfx> _pool = new();

        public CoinFlyVfxService(CoinFlyVfx prefab, RectTransform poolRoot, CombatSceneRefs refs)
        {
            _prefab = prefab;
            _poolRoot = poolRoot;
            _refs = refs;
        }

        public void Play(Vector3 worldPosition, Action onRewardGranted)
        {
            if (_prefab == null || _poolRoot == null || _refs == null || _refs.CoinHudTarget == null)
            {
                onRewardGranted?.Invoke();
                return;
            }

            if (!TryGetLocalPositions(worldPosition, out var startLocal, out var targetLocal))
            {
                onRewardGranted?.Invoke();
                return;
            }

            WarmPool(BurstCount);
            var completed = 0;

            for (var i = 0; i < BurstCount; i++)
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
                        if (completed >= BurstCount)
                            onRewardGranted?.Invoke();
                    });
            }
        }

        Vector2 GetSpreadOffset(int index)
        {
            var angle = Mathf.Lerp(-55f, 55f, index / (float)(BurstCount - 1));
            var radians = angle * Mathf.Deg2Rad;
            var radius = SpreadRadius * UnityEngine.Random.Range(0.55f, 1f);
            return new Vector2(Mathf.Sin(radians) * radius, Mathf.Cos(radians) * radius * 0.35f);
        }

        bool TryGetLocalPositions(Vector3 worldPosition, out Vector2 startLocal, out Vector2 targetLocal)
        {
            startLocal = default;
            targetLocal = default;

            var canvas = _poolRoot.GetComponentInParent<Canvas>();
            var camera = Camera.main;
            if (canvas == null || camera == null)
                return false;

            var uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            var screen = camera.WorldToScreenPoint(worldPosition);

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_poolRoot, screen, uiCamera, out startLocal))
                return false;

            var hudScreen = RectTransformUtility.WorldToScreenPoint(uiCamera, _refs.CoinHudTarget.position);
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(_poolRoot, hudScreen, uiCamera, out targetLocal);
        }

        void WarmPool(int count)
        {
            while (_pool.Count < count)
                _pool.Add(CreateInstance());
        }

        CoinFlyVfx Rent()
        {
            for (var i = 0; i < _pool.Count; i++)
            {
                var coin = _pool[i];
                if (coin != null && !coin.gameObject.activeSelf)
                    return coin;
            }

            return CreateInstance();
        }

        CoinFlyVfx CreateInstance()
        {
            var instance = UnityEngine.Object.Instantiate(_prefab, _poolRoot);
            instance.transform.SetAsLastSibling();
            _pool.Add(instance);
            return instance;
        }
    }
}
