using MagicArcher.Core.Config;
using MagicArcher.Gameplay.Combat;

using MagicArcher.Gameplay.Units;

using UnityEngine;



namespace MagicArcher.Gameplay.Units

{

    public sealed class UnitView : MonoBehaviour

    {

        static readonly int VictoryHash = Animator.StringToHash("Victory");



        [SerializeField] ArcherShooter _shooter;

        [SerializeField] UnitHealth _health;

        [SerializeField] bool _isUpgraded;

        [SerializeField] int _gridX = -1;

        [SerializeField] int _gridY = -1;

        RegularUnitConfig _regular;
        UpgradedUnitConfig _upgraded;



        public ArcherShooter Shooter => _shooter;

        public UnitHealth Health => _health;

        public bool IsUpgraded => _isUpgraded;

        public int GridX => _gridX;

        public int GridY => _gridY;



        void Awake()

        {

            if (_health == null)

                _health = GetComponent<UnitHealth>();

        }



        [Zenject.Inject]

        void Construct(
            [Zenject.Inject(Optional = true)] RegularUnitConfig regular = null,
            [Zenject.Inject(Optional = true)] UpgradedUnitConfig upgraded = null)
        {
            _regular = regular;
            _upgraded = upgraded;
        }



        public void SetGridPosition(int x, int y)

        {

            _gridX = x;

            _gridY = y;

        }



        public void SetUpgraded(bool isUpgraded)

        {

            _isUpgraded = isUpgraded;

            if (_shooter != null)

                _shooter.SetTier(isUpgraded ? UnitTier.Upgraded : UnitTier.Normal);



            UnitConfigBase config = isUpgraded ? _upgraded : _regular;
            var maxHealth = config != null ? config.MaxHealth : isUpgraded ? 250f : 150f;
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
