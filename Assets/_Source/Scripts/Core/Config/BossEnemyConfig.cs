using MagicArcher.Gameplay.Enemies;
using MagicArcher.UI;
using UnityEngine;

namespace MagicArcher.Core.Config
{
    [CreateAssetMenu(fileName = "BossEnemyConfig", menuName = "Magic Archer/Boss Enemy Config")]
    public sealed class BossEnemyConfig : ScriptableObject
    {
        public const string DefaultAssetPath = "Assets/_Source/Configs/BossEnemyConfig.asset";

        [Header("Prefab")]
        [SerializeField] EnemyView _prefab;

        [Header("Combat")]
        [SerializeField] float _maxHealth = 450f;
        [SerializeField] float _attackTriggerRadius = 2.4f;
        [SerializeField] float _attackDuration = 2.125f;
        [SerializeField] float _attackHitNormalizedTime = 0.5f;
        [SerializeField] float _moveSpeed = 1.6f;

        [Header("Visual")]
        [SerializeField] float _scaleMultiplier = 1.65f;
        [SerializeField] Color _tint = new(0.62f, 0.18f, 0.95f, 1f);

        [Header("Reward")]
        [SerializeField] int _coinReward = 5;

        [Header("Death")]
        [SerializeField] int _deathAnimationFrames = 170;
        [SerializeField] float _deathAnimationFps = 60f;

        [Header("Health Bar")]
        [SerializeField] float _healthBarScale = 1.65f;

        public EnemyView Prefab => _prefab;
        public float MaxHealth => Mathf.Max(1f, _maxHealth);
        public float AttackTriggerRadius => Mathf.Max(0.1f, _attackTriggerRadius);
        public float AttackDuration => Mathf.Max(0.05f, _attackDuration);
        public float AttackHitNormalizedTime => Mathf.Clamp01(_attackHitNormalizedTime);
        public float MoveSpeed => _moveSpeed;
        public float ScaleMultiplier => Mathf.Max(0.01f, _scaleMultiplier);
        public Color Tint => _tint;
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
