using System;
using MagicArcher.Gameplay.Combat;
using MagicArcher.Gameplay.Units;
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
        [SerializeField] ProjectileView _projectilePrefab;
        [SerializeField] float _damage = 50f;
        [SerializeField] float _attackCooldown = 1.1f;
        [SerializeField] float _attackRange = 25f;

        [Header("Merge")]
        [SerializeField] UnitConfigBase[] _mergeChildren;

        public GameObject Prefab => _prefab;
        public UnitView UnitViewPrefab => _prefab != null ? _prefab.GetComponent<UnitView>() : null;
        public ProjectileView ProjectilePrefab => _projectilePrefab;
        public float MaxHealth => Mathf.Max(1f, _maxHealth);
        public float Damage => Mathf.Max(0f, _damage);
        public float AttackCooldown => Mathf.Max(0.05f, _attackCooldown);
        public float AttackRange => _attackRange;
        public UnitConfigBase[] MergeChildren => _mergeChildren;

        public bool MatchesMergeIngredients(UnitConfigBase first, UnitConfigBase second)
        {
            if (_mergeChildren == null || _mergeChildren.Length != 2 || first == null || second == null)
                return false;

            return (_mergeChildren[0] == first && _mergeChildren[1] == second)
                || (_mergeChildren[0] == second && _mergeChildren[1] == first);
        }

        public bool ContainsMergeChild(UnitConfigBase config)
        {
            if (_mergeChildren == null || config == null)
                return false;

            for (var i = 0; i < _mergeChildren.Length; i++)
            {
                if (_mergeChildren[i] == config)
                    return true;
            }

            return false;
        }
    }
}
