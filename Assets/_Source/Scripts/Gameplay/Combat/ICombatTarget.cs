using UnityEngine;

namespace MagicArcher.Gameplay.Combat
{
    public interface ICombatTarget
    {
        Transform AimPoint { get; }
        bool IsAlive { get; }
        IDamageable Damageable { get; }
        bool AllowsMultipleAttackers { get; }
    }
}
