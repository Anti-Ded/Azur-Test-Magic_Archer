using System;

namespace MagicArcher.UI
{
    public interface IHealthBarProvider
    {
        float NormalizedHealth { get; }
        event Action HealthChanged;
    }
}
