using MagicArcher.Core;
using MagicArcher.Gameplay.Flow;
using UnityEngine;

namespace MagicArcher.Gameplay.Combat
{
    public sealed class ProjectileView : MonoBehaviour
    {
        [SerializeField] float _speed = GameConstants.ProjectileSpeed;

        Transform _target;
        IDamageable _damageable;
        GamePhaseService _phases;
        float _damage;
        bool _active;

        public void Configure(GamePhaseService phases)
        {
            _phases = phases;
        }

        public void Launch(Vector3 position, ICombatTarget target, float damage)
        {
            if (target == null || !target.IsAlive || damage <= 0f)
            {
                gameObject.SetActive(false);
                return;
            }

            _target = target.AimPoint;
            _damageable = target.Damageable;
            _damage = damage;
            transform.position = position;
            _active = true;
            gameObject.SetActive(true);
        }

        void Update()
        {
            if (!_active)
                return;

            if (IsCombatPaused())
                return;

            if (_target == null || _damageable == null || !_damageable.IsAlive)
            {
                Deactivate();
                return;
            }

            var targetPosition = _target.position + Vector3.up * 1.2f;
            var step = _speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);

            var forward = targetPosition - transform.position;
            if (forward.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(forward);

            if ((transform.position - targetPosition).sqrMagnitude > 0.05f)
                return;

            _damageable.TakeDamage(_damage);
            Deactivate();
        }

        bool IsCombatPaused()
        {
            return _phases != null && _phases.IsTutorialCombatPaused;
        }

        public void Deactivate()
        {
            _active = false;
            _target = null;
            _damageable = null;
            _damage = 0f;
            gameObject.SetActive(false);
        }
    }
}
