using MagicArcher.Core.Config;
using UnityEngine;
using Zenject;

namespace MagicArcher.Gameplay.Units
{
    public sealed class UnitView : MonoBehaviour
    {
        static readonly int VictoryHash = Animator.StringToHash("Victory");

        [SerializeField] ArcherShooter _shooter;
        [SerializeField] UnitHealth _health;
        [SerializeField] int _gridX = -1;
        [SerializeField] int _gridY = -1;

        UnitConfigBase _config;
        UnitConfigCatalog _unitConfigs;

        public ArcherShooter Shooter => _shooter;
        public UnitHealth Health => _health;
        public UnitConfigBase Config => _config;
        public bool CanMerge =>
            _config != null && _unitConfigs != null && _unitConfigs.IsMergeIngredient(_config);
        public int GridX => _gridX;
        public int GridY => _gridY;

        [Inject]
        void Construct([Inject(Optional = true)] UnitConfigCatalog unitConfigs = null)
        {
            _unitConfigs = unitConfigs;
        }

        void Awake()
        {
            if (_health == null)
                _health = GetComponent<UnitHealth>() ?? gameObject.AddComponent<UnitHealth>();

            UnitPhysicsBody.Ensure(gameObject);
        }

        public void SetGridPosition(int x, int y)
        {
            _gridX = x;
            _gridY = y;
        }

        public void ApplyConfig(UnitConfigBase config)
        {
            _config = config;

            if (_shooter != null)
                _shooter.ApplyConfig(config);

            var maxHealth = config != null ? config.MaxHealth : 150f;
            _health?.Configure(maxHealth);
        }

        public void PlayVictory()
        {
            if (_shooter != null)
                _shooter.enabled = false;

            var animator = GetComponentInChildren<Animator>();
            if (animator != null)
                animator.SetTrigger(VictoryHash);
        }
    }
}
