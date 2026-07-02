using MagicArcher.Core.Config;
using MagicArcher.Gameplay.Enemies;
using UnityEngine;

namespace MagicArcher.Gameplay.Economy
{
    public sealed class EnemyKillRewardService
    {
        readonly CoinFlyVfxService _coinFly;
        readonly IEconomyService _economy;
        readonly RegularEnemyConfig _regular;
        readonly BossEnemyConfig _boss;

        public EnemyKillRewardService(
            CoinFlyVfxService coinFly,
            IEconomyService economy,
            RegularEnemyConfig regular,
            BossEnemyConfig boss)
        {
            _coinFly = coinFly;
            _economy = economy;
            _regular = regular;
            _boss = boss;
        }

        public void Grant(EnemyView enemy)
        {
            if (enemy == null)
                return;

            var amount = ResolveReward(enemy);
            if (amount <= 0)
                return;

            var deathPosition = enemy.transform.position;
            _coinFly.Play(deathPosition, () => _economy.AddCoins(amount));
        }

        int ResolveReward(EnemyView enemy)
        {
            var isBoss = enemy.Health != null && enemy.Health.IsBoss;
            if (isBoss)
                return _boss != null ? _boss.CoinReward : 0;

            return _regular != null ? _regular.CoinReward : 0;
        }
    }
}
