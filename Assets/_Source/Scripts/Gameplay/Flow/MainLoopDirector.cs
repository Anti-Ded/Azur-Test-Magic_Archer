using MagicArcher.Core;
using MagicArcher.Core.Config;

using MagicArcher.Gameplay.Combat;

using MagicArcher.Gameplay.Economy;

using MagicArcher.Gameplay.Enemies;

using MagicArcher.Gameplay.Level;

using MagicArcher.Gameplay.Units;

using MagicArcher.StateMachine;

using MagicArcher.UI;

using UnityEngine;

using Zenject;



namespace MagicArcher.Gameplay.Flow

{

    public sealed class MainLoopDirector : MonoBehaviour

    {

        UnitPurchaseService _purchase;

        EnemyWaveController _wave;

        IEconomyService _economy;

        CoinFlyVfxService _coinFly;

        GamePhaseService _phases;

        LevelRoot _level;

        CombatUiRefs _ui;

        CombatSceneRefs _refs;

        RegularEnemyConfig _regular;

        BossEnemyConfig _boss;



        bool _active;

        bool _bossSpawned;



        [Inject]

        void Construct(

            UnitPurchaseService purchase,

            EnemyWaveController wave,

            IEconomyService economy,

            CoinFlyVfxService coinFly,

            GamePhaseService phases,

            LevelRoot level,

            CombatSceneRefs refs,

            RegularEnemyConfig regular,

            BossEnemyConfig boss,

            [Inject(Optional = true)] CombatUiRefs ui = null)

        {

            _purchase = purchase;

            _wave = wave;

            _economy = economy;

            _coinFly = coinFly;

            _phases = phases;

            _level = level;

            _refs = refs;

            _regular = regular;

            _boss = boss;

            _ui = ui;

        }



        public void Begin()

        {

            _active = true;

            _bossSpawned = false;

            _purchase.SetCost(GameConstants.InitialBuyUnitCost);

            _purchase.UnitPurchased += OnUnitPurchased;

            _wave.OrcDied += OnOrcDied;



            var buyButton = ResolveBuyButton();

            if (buyButton != null)

            {

                buyButton.Clicked += OnBuyClicked;

                buyButton.Show(GameConstants.InitialBuyUnitCost);

            }



            _wave.ActivateNextWalker();

        }



        public void End()

        {

            _active = false;



            var buyButton = ResolveBuyButton();

            if (buyButton != null)

            {

                buyButton.Clicked -= OnBuyClicked;

                buyButton.Hide();

            }



            if (_purchase != null)

                _purchase.UnitPurchased -= OnUnitPurchased;



            if (_wave != null)

                _wave.OrcDied -= OnOrcDied;

        }



        void OnBuyClicked()

        {

            if (_purchase == null || !_purchase.TryPurchase())

                return;



            var buyButton = ResolveBuyButton();

            if (buyButton != null)

                buyButton.Show(_purchase.CurrentCost);

        }



        void OnUnitPurchased(UnitView _)

        {

            _purchase.SetCost(_purchase.CurrentCost + GameConstants.BuyUnitCostStep);

        }



        void OnOrcDied(EnemyView orc)

        {

            if (!_active || orc == null)

                return;



            if (orc.Health != null && orc.Health.IsBoss)

            {

                _coinFly.Play(orc.transform.position, () =>
                    _economy.AddCoins(GameConstants.CoinsPerOrcKill));

                HandleVictory();

                return;

            }



            _coinFly.Play(orc.transform.position, () =>
                _economy.AddCoins(GameConstants.CoinsPerOrcKill));

            _wave.ActivateNextWalker();

            TrySpawnBoss();

        }



        void TrySpawnBoss()

        {

            if (_bossSpawned || _wave.HasAliveRegularEnemies())

                return;



            if (ResolveEnemyPrefab() == null || _level == null || _level.EnemyPathRoot == null)

                return;



            var enemyPath = _level.EnemyPathRoot.GetComponent<EnemyPathView>();

            if (enemyPath == null)

                return;



            _bossSpawned = true;

            var spawnOrigin = enemyPath.GetWaypointPosition(0);

            _wave.SpawnBoss(ResolveEnemyPrefab(), spawnOrigin, true);

        }



        EnemyView ResolveEnemyPrefab()
        {
            if (_regular != null && _regular.Prefab != null)
                return _regular.Prefab;

            return _refs != null ? _refs.OrcPrefab : null;
        }



        void HandleVictory()

        {

            _active = false;

            End();

            _phases.ChangePhase(GamePhase.Victory);

        }



        BuyUnitButtonView ResolveBuyButton()

        {

            if (_ui != null && _ui.BuyUnitButton != null)

                return _ui.BuyUnitButton;



            return Object.FindFirstObjectByType<BuyUnitButtonView>(FindObjectsInactive.Include);

        }

    }

}
