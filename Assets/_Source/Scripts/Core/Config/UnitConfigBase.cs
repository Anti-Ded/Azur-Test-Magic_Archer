using UnityEngine;

namespace MagicArcher.Core.Config
{
    public abstract class UnitConfigBase : ScriptableObject
    {
        [Header("Prefab")]
        [SerializeField] GameObject _prefab;

        [Header("Health")]
        [SerializeField] float _maxHealth = 150f;

        [Header("Attack")]
        [SerializeField] float _damage = 50f;
        [SerializeField] float _attackCooldown = 1.1f;
        [SerializeField] float _attackRange = 25f;

        public GameObject Prefab => _prefab;
        public float MaxHealth => Mathf.Max(1f, _maxHealth);
        public float Damage => Mathf.Max(0f, _damage);
        public float AttackCooldown => Mathf.Max(0.05f, _attackCooldown);
        public float AttackRange => _attackRange;
    }
}
