using System;

namespace MagicArcher.Gameplay.Economy
{
    public interface IEconomyService
    {
        int Coins { get; }
        event Action<int> CoinsChanged;
        void ResetCoins();
        void AddCoins(int amount);
        bool TrySpend(int amount);
    }
}
