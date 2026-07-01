using System;
using MagicArcher.Core.Audio;

namespace MagicArcher.Gameplay.Economy
{
    public sealed class EconomyService : IEconomyService
    {
        readonly IAudioService _audio;

        public int Coins { get; private set; }
        public event Action<int> CoinsChanged;

        [Zenject.Inject]
        public EconomyService([Zenject.Inject(Optional = true)] IAudioService audio = null)
        {
            _audio = audio;
        }

        public void ResetCoins()
        {
            Coins = 0;
            CoinsChanged?.Invoke(Coins);
        }

        public void AddCoins(int amount)
        {
            if (amount <= 0)
                return;

            Coins += amount;
            CoinsChanged?.Invoke(Coins);
            if (amount > 0)
                _audio?.PlayCoins();
        }

        public bool TrySpend(int amount)
        {
            if (amount <= 0 || Coins < amount)
                return false;

            Coins -= amount;
            CoinsChanged?.Invoke(Coins);
            return true;
        }
    }
}
