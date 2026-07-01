using System;

namespace MagicArcher.Gameplay.Combat
{
    public interface IDamageable
    {
        bool IsAlive { get; }
        float MaxHealth { get; }
        float CurrentHealth { get; }
        event Action<IDamageable> Died;
        void TakeDamage(float damage);
    }
}
