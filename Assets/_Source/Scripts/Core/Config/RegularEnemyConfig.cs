using MagicArcher.Gameplay.Enemies;
using MagicArcher.UI;
using UnityEngine;

namespace MagicArcher.Core.Config
{
    [CreateAssetMenu(fileName = "RegularEnemyConfig", menuName = "Magic Archer/Regular Enemy Config")]
    public sealed class RegularEnemyConfig : ScriptableObject
    {
        public const string DefaultAssetPath = "Assets/_Source/Configs/RegularEnemyConfig.asset";

        [Header("Prefab")]
        [SerializeField] EnemyView _prefab;

        [Header("Combat")]
        [SerializeField] float _maxHealth = 100f;
        [SerializeField] float _attackTriggerRadius = 1.8f;
        [SerializeField] float _attackDuration = 2.125f;
        [SerializeField] float _attackHitNormalizedTime = 0.5f;
        [SerializeField] float _moveSpeed = 1.6f;

        [Header("Wave")]
        [SerializeField] float _spawnSpacing = 1.5f;
        [SerializeField] int _introQueueCount = 16;

        [Header("Reward")]
        [SerializeField] int _coinReward = 5;

        [Header("Death")]
        [SerializeField] int _deathAnimationFrames = 170;
        [SerializeField] float _deathAnimationFps = 60f;

        [Header("Health Bar")]
        [SerializeField] float _healthBarScale = 1f;

        public EnemyView Prefab => _prefab;
        public float MaxHealth => Mathf.Max(1f, _maxHealth);
        public float AttackTriggerRadius => Mathf.Max(0.1f, _attackTriggerRadius);
        public float AttackDuration => Mathf.Max(0.05f, _attackDuration);
        public float AttackHitNormalizedTime => Mathf.Clamp01(_attackHitNormalizedTime);
        public float MoveSpeed => _moveSpeed;
        public float SpawnSpacing => _spawnSpacing;
        public int IntroQueueCount => Mathf.Max(1, _introQueueCount);
        public int CoinReward => Mathf.Max(0, _coinReward);
        public float DeathReturnDelay =>
            _deathAnimationFps <= 0f ? 0f : _deathAnimationFrames / _deathAnimationFps;

        public HealthBarProfile CreateHealthBarProfile()
        {
            return new HealthBarProfile
            {
                ScreenOffsetY = 90f,
                Size = new Vector2(48f, 7f),
                BarScale = _healthBarScale,
                FillColor = new Color(0.95f, 0.2f, 0.2f, 1f),
                HideWhenFull = false
            };
        }
    }
}
