using MagicArcher.Gameplay.Economy;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace MagicArcher.UI
{
    public sealed class CurrencyHud : MonoBehaviour
    {
        [SerializeField] Text _coinsLabel;

        IEconomyService _economy;

        [Inject]
        void Construct(IEconomyService economy)
        {
            _economy = economy;
        }

        void OnEnable()
        {
            if (_economy == null)
                return;

            _economy.CoinsChanged += OnCoinsChanged;
            OnCoinsChanged(_economy.Coins);
        }

        void OnDisable()
        {
            if (_economy == null)
                return;

            _economy.CoinsChanged -= OnCoinsChanged;
        }

        void OnCoinsChanged(int coins)
        {
            if (_coinsLabel != null)
                _coinsLabel.text = coins.ToString();
        }
    }
}
